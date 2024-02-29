using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using App.Extensions;
using App.Services;
using App.Settings;
using App.Validation;
using Identity.Consumer.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

var builder = WebApplication.CreateBuilder(args);

// Configure configuration for the application.
builder.Services.Configure<Configuration>(builder.Configuration.GetRequiredSection("Configuration"));
builder.Services.Configure<IdentityConsumerConfiguration>(builder.Configuration.GetRequiredSection("Configuration:Authentication"));

// Add services to the container.
builder.Services.AddControllers();

// Add cors.
builder.Services.AddCors();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database services
builder.Services.AddApplicationDatabase();
builder.Services.AddApplicationServices();
builder.Services.AddValidators();

// Add built-in application authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = builder.Configuration["Configuration:Token:Audience"],
                ValidIssuer = builder.Configuration["Configuration:Token:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Configuration:Token:Secret"]!)
                ),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromSeconds(5)
            };
        }
    );

builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy(
            name: Policies.MFA_MISSING_POLICY, 
            configurePolicy: p => p
                .RequireAuthenticatedUser()
                .RequireAssertion(
                    c =>
                    {
                        var acrClaimType = JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[JwtRegisteredClaimNames.Acr];
                        var amrClaimType = JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[JwtRegisteredClaimNames.Amr];

                        var loaClaim = c.User.Claims.FirstOrDefault(l => l.Type == acrClaimType);
                        if (loaClaim is null) return false;
                        if (!int.TryParse(loaClaim.Value, out var loa)) return false;
                        if (!c.User.HasClaim(amrClaimType, "otp")) return false;

                        return loa == 1;
                    }
            )
        );
        
        options.AddPolicy(
            name: Policies.MFA_REQUIRED_IF_ENABLED_POLICY,
            configurePolicy: p => p
                .RequireAssertion(
                    c =>
                    {
                        var acrClaimType = JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[JwtRegisteredClaimNames.Acr];
                        var amrClaimType = JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[JwtRegisteredClaimNames.Amr];
                        
                        var loaClaim = c.User.Claims.FirstOrDefault(l => l.Type == acrClaimType);
                        if (loaClaim is null) return true;
                        if (!int.TryParse(loaClaim.Value, out var loa)) return true;
                        if (!c.User.HasClaim(amrClaimType, "otp")) return true;

                        return loa == 2;
                    }    
                )
        );
    }
);

// Add mediator
builder.Services.AddMediatR(
    cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly)
);

var app = builder.Build();

// Configure swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure middleware
app.UseMiddleware<MfaUnauthorizedRedirectMiddleware>();

// Configure cors
app.UseCors(options => options
    .WithOrigins(
        "http://localhost:3000"    
    )
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
);

app.UseAuthorization();

app.MapControllers();

app.Run();