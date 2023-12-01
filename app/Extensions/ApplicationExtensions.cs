using App.Database.Contexts;
using App.Database.Repositories;
using App.Database.Services;
using App.Models;
using App.Services;
using App.Validation.Users;
using FluentValidation;
using Identity.Consumer.Models.Code;
using Identity.Consumer.Services;
using Identity.Consumer.Services.Consumers.Code;
using Identity.Consumer.Services.Consumers.Oidc;
using Identity.Consumer.Services.Consumers.WebAuthn;
using HashCode = Identity.Consumer.Models.Code.HashCode;

namespace App.Extensions;

public static class ApplicationExtensions
{
    /// <summary>
    /// Add database services to the current service collection.
    /// </summary>
    /// <param name="services">The current service collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when the given service collection is null.</exception>
    public static void AddApplicationDatabase(this IServiceCollection services)
    {
        // Services guard
        ArgumentNullException.ThrowIfNull(services);

        // Add contexts
        services.AddDbContext<UserContext>();
        
        // Add repositories
        services.AddTransient<IRepository<User>, UserRepository>();
        
        // Add services
        services.AddTransient<IUserService, UserService>();
    }
    
    /// <summary>
    /// Add application services to the current service collection.
    /// </summary>
    /// <param name="services">The current service collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when the given service collection is null.</exception>
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Services guard
        ArgumentNullException.ThrowIfNull(services);

        // Add authentication services
        services.AddTransient<IAuthenticationService, AuthenticationService>();

        // Add identity consumers
        services.AddSingleton<GoogleIdentityConsumer>();
        services.AddSingleton<MicrosoftIdentityConsumer>();
        
        services.AddSingleton<IWebAuthnConsumer, LocalWebAuthnConsumer>();
        
        services.AddSingleton<ICodeConsumer<HashCode>, HashCodeConsumer>();
        services.AddSingleton<ICodeConsumer<TimeCode>, TimeCodeConsumer>();
    }

    /// <summary>
    /// Add a range of validators to the current service collection.
    /// </summary>
    /// <param name="services">The current service collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when the given service collection is null.</exception>
    public static void AddValidators(this IServiceCollection services)
    {
        // Services guard
        ArgumentNullException.ThrowIfNull(services);
        
        // Add validators
        services.AddScoped<IValidator<User>, UserValidator>();
    }
}