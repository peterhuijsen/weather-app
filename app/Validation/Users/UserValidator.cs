using App.Models;
using FluentValidation;

namespace App.Validation.Users;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Uuid).NotNull();
        RuleFor(u => u.Username).Length(min: 3, max: 20).NotNull();
        RuleFor(u => u.Email).EmailAddress().NotNull();
        
        RuleFor(u => u.Credentials).NotNull()
            .ChildRules(
                v =>
                {
                    v.RuleFor(c => c!.Hash)
                        .Length(25);
                }
            );
    }
}