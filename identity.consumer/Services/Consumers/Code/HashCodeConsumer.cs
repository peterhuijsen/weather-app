using System.Security.Cryptography;
using Identity.Consumer.Configuration;
using Identity.Consumer.Helpers;
using Microsoft.Extensions.Options;
using OtpNet;
using HashCode = Identity.Consumer.Models.Code.HashCode;

namespace Identity.Consumer.Services.Consumers.Code;

public class HashCodeConsumer : CodeConsumer<HashCode>
{
    private readonly IdentityConsumerConfiguration _configuration;

    public HashCodeConsumer(IOptions<IdentityConsumerConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    /// <inheritdoc cref="CodeConsumer{TCode}.GenerateSecret"/>
    public override string GenerateSecret()
    {
        var secretBytes = SHA1.HashData(RandomNumberGenerator.GetBytes(count: 1024));
        var secret = Base32.Encode(secretBytes);

        return secret;
    }

    /// <inheritdoc cref="CodeConsumer{TCode}.GenerateUri"/>
    public override string GenerateUri(string secret, string email, long counter = 0)
        => $"otpauth://hotp/{_configuration.Otp.Issuer}:{email}?secret={secret}&issuer={_configuration.Otp.Issuer}&digits={_configuration.Otp.Digits}&counter={counter}";

    /// <inheritdoc cref="CodeConsumer{TCode}.GenerateOneTimeCode"/>
    public override string GenerateOneTimeCode(string secret, HashCode context)
    {
        var hotp = new Hotp(Base32.Decode(secret), hotpSize: _configuration.Otp.Digits);
        return hotp.ComputeHOTP(context.Counter);
    }

    /// <inheritdoc cref="CodeConsumer{TCode}.VerifyOneTimeCode"/>
    public override bool VerifyOneTimeCode(string secret, HashCode code)
    {
        var hotp = new Hotp(Base32.Decode(secret), hotpSize: _configuration.Otp.Digits);
        return hotp.VerifyHotp(code.Value, code.Counter);
    }
}