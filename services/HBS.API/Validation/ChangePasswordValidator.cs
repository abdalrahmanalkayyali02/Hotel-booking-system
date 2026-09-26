using FluentValidation;
using HBS.API.Dtos.Authentication.ChangePassword;

namespace HBS.API.Validation;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Password is required");
            /*.MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one capital letter")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lower case letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches(@"[^A-Za-z0-9\s]")
            .WithMessage("Password must contain at least one special character")
            .Must(password => !password.Any(char.IsWhiteSpace))
            .WithMessage("Password must not Contain any white spaces");*/
        
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one capital letter")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lower case letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches(@"[^A-Za-z0-9\s]")
            .WithMessage("Password must contain at least one special character")
            .Must(password => !password.Any(char.IsWhiteSpace))
            .WithMessage("Password must not Contain any white spaces");
    }
    
}