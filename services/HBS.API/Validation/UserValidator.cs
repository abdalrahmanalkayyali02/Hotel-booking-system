using FluentValidation;
using HBS.API.Dtos.Users.Registeration;

namespace HBS.API.Validation;

public class UserValidator : AbstractValidator<RegisterStandardUsersDtos>
{
    public UserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MinimumLength(3)
            .WithMessage("First name must be at least 3 characters long")
            .MaximumLength(15)
            .WithMessage("FirstName cannot exceed 15 characters")
            .Matches(@"^[A-Za-z]+$")
            .WithMessage("First Name cannot have any spaces, numbers, or special characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MinimumLength(3)
            .WithMessage("Last name must be at least 3 characters long")
            .MaximumLength(10)
            .WithMessage("LastName cannot exceed 10 characters")
            .Matches(@"^[A-Za-z]+$")
            .WithMessage("Last name cannot have any spaces, numbers, or special characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email address");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required")
            .Must(birthDate => birthDate <= DateOnly.FromDateTime(DateTime.Today).AddYears(-18))
            .WithMessage("User must be at least 18 years old")
            .Must(birthDate => birthDate >= DateOnly.FromDateTime(DateTime.Today).AddYears(-80))
            .WithMessage("User cannot be older than 80 years old");

        RuleFor(x => x.Password)
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

        RuleFor(x => x.PhoneNumberCountryCode)
            .Matches(@"^\+[0-9]{1,3}$")
            .WithMessage("Country code must begin with a + followed by 1-3 digits only");

        RuleFor(x => x.PhoneNumberValue)
            .Matches(@"^[0-9]{9}$")
            .WithMessage("Phone Number must be 9 digits long and must consist of digits only");
    }

}