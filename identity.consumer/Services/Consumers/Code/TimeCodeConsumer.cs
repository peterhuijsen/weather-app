using System.Security.Cryptography;
using Identity.Consumer.Configuration;
using Identity.Consumer.Helpers;
using Identity.Consumer.Models.Code;
using Microsoft.Extensions.Options;
using OtpNet;

namespace Identity.Consumer.Services.Consumers.Code;

public class TimeCodeConsumer : CodeConsumer<TimeCode>
{
    private readonly IdentityConsumerConfiguration _configuration;

    public TimeCodeConsumer(IOptions<IdentityConsumerConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }
    
    public override string GenerateSecret()
    {
        var secretBytes = SHA1.HashData(RandomNumberGenerator.GetBytes(count: 1024));
        var secret = Base32.Encode(secretBytes);

        return secret;
    }
    
    public override string GenerateUri(string secret, string email, long counter = 0)
        => $"otpauth://totp/{_configuration.Otp.Issuer}:{email}?secret={secret}&issuer={_configuration.Otp.Issuer}&digits={_configuration.Otp.Digits}&period={_configuration.Otp.Period}";

    public override string GenerateOneTimeCode(string secret, TimeCode context)
    {
        var totp = new Totp(Base32.Decode(secret), totpSize: _configuration.Otp.Digits);
        return totp.ComputeTotp(context.Timestamp);
    }

    public override bool VerifyOneTimeCode(string secret, TimeCode code)
    {
        var hotp = new Totp(Base32.Decode(secret), totpSize: _configuration.Otp.Digits);
        return hotp.VerifyTotp(timestamp: code.Timestamp, totp: code.Value, timeStepMatched: out var _);
    }
}