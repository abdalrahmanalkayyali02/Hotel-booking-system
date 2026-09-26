using FluentValidation;
using HBS.API.Dtos.Users.Update;

namespace HBS.API.Validation;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MinimumLength(3)
            .WithMessage("First name must be at least 3 characters long")
            .MaximumLength(15)
            .WithMessage("First name cannot exceed 15 characters")
            .Matches(@"^[A-Za-z]+$")
            .WithMessage("First name cannot have any spaces, numbers, or special characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MinimumLength(3)
            .WithMessage("Last name must be at least 3 characters long")
            .MaximumLength(10)
            .WithMessage("Last name cannot exceed 10 characters")
            .Matches(@"^[A-Za-z]+$")
            .WithMessage("Last name cannot have any spaces, numbers, or special characters");

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage("Birth date is required")
            .Must(birthDate =>
                birthDate <= DateOnly.FromDateTime(DateTime.Today).AddYears(-18))
            .WithMessage("User must be at least 18 years old")
            .Must(birthDate =>
                birthDate >= DateOnly.FromDateTime(DateTime.Today).AddYears(-80))
            .WithMessage("User cannot be older than 80 years old");

        RuleFor(x => x.PhoneNumberCountryCode)
            .NotEmpty()
            .WithMessage("Phone number country code is required")
            .Matches(@"^\+[0-9]{1,3}$")
            .WithMessage("Country code must begin with a + followed by 1-3 digits only");

        RuleFor(x => x.PhoneNumberValue)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^[0-9]{9}$")
            .WithMessage("Phone number must be 9 digits long and must consist of digits only");

        RuleFor(x => x.CountryId)
            .NotEmpty()
            .WithMessage("Country is required");

        RuleFor(x => x.CityId)
            .NotEmpty()
            .WithMessage("City is required");
    }
}