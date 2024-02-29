using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Models;
using App.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace App.Services;

public interface IAuthenticationService
{
    /// <summary>
    /// Hash a string by using the <see cref="Rfc2898DeriveBytes"/> class and generating a random salt.
    /// Format of the hash is as follows: SALT.HASH
    /// </summary>
    /// <param name="password">The string of characters which should be hashed.</param>
    /// <returns>The combined salt and hash string.</returns>
    string Hash(string password);

    /// <summary>
    /// Verify whether or not the given password matches the given hash.
    /// </summary>
    /// <param name="hash">The hash which has been challenged.</param>
    /// <param name="password">The password which should be compared to the hash.</param>
    /// <returns>Whether or not the password is identical to the hashed password.</returns>
    bool Challenge(string hash, string password);
    
    /// <summary>
    /// Generate a new access token for the given <see cref="User"/>.
    /// </summary>
    /// <returns>The newly generated access token for the given <see cref="User"/>.</returns>
    string Token(User user, int loa);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly Configuration _configuration;

    public AuthenticationService(IOptions<Configuration> configuration)
        => _configuration = configuration.Value;
    
    /// <inheritdoc cref="IAuthenticationService.Hash"/>
    public string Hash(string password)
    {
        var salt = new byte[_configuration.Hashing.Size / 16];
        using (var rng = RandomNumberGenerator.Create())
            rng.GetBytes(salt);

        var pbkfd2 = new Rfc2898DeriveBytes(password, salt, _configuration.Hashing.Iterations, HashAlgorithmName.SHA384)
            .GetBytes(_configuration.Hashing.Size / 16);
        
        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(pbkfd2);
    }

    /// <inheritdoc cref="IAuthenticationService.Challenge"/>
    public bool Challenge(string hash, string password)
    {
        try
        {
            var split = hash.Split('.');
            
            var salt = Convert.FromBase64String(split[0]);
            var challenge = Convert.FromBase64String(split[1]);
            
            var attempt = new Rfc2898DeriveBytes(password, salt, _configuration.Hashing.Iterations, HashAlgorithmName.SHA384)
                .GetBytes(_configuration.Hashing.Size / 16);

            return attempt.SequenceEqual(challenge);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc cref="IAuthenticationService.Token"/>
    public string Token(User user, int loa)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Uuid.ToString()),
            new(JwtRegisteredClaimNames.Name, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Acr, loa.ToString())
        };
        
        if (user.Credentials.MFA)
            claims.Add(new(JwtRegisteredClaimNames.Amr, "otp"));
        if (user.Credentials.Hash is not null)
            claims.Add(new (JwtRegisteredClaimNames.Amr, "pwd"));
        if (user.Credentials.Passkeys is not null) 
            claims.Add(new (JwtRegisteredClaimNames.Amr, "pky"));
        if (user.Credentials.Google is not null || user.Credentials.Microsoft is not null)
            claims.Add(new (JwtRegisteredClaimNames.Amr, "oic"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_configuration.Token.Lifespan),
            Issuer = _configuration.Token.Issuer,
            Audience = _configuration.Token.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Token.Secret)), 
                SecurityAlgorithms.HmacSha384Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return jwt;
    }
}