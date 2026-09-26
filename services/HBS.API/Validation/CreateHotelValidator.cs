using FluentValidation;
using HBS.API.Dtos.Hotels.CreateHotel;

namespace HBS.API.Validation;

public class CreateHotelValidator : AbstractValidator<CreateHotelRequest>
{
    public CreateHotelValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required");
        
        RuleFor(x=>x.Country)
            .NotEmpty()
            .WithMessage("Country is required");

        RuleFor(x => x.StarRating)
            .NotEmpty()
            .WithMessage("Star rating is required")
            .InclusiveBetween(1.0f, 5.0f)
            .WithMessage("Star rating must be between 1.0 and 5.0")
            .Must(rating => Math.Abs(rating * 10 - Math.Round(rating * 10)) < 0.0001)
            .WithMessage("Star rating can have at most one decimal place.");
        
        RuleFor(x => x.PhoneNumberCountryCode)
            .Matches(@"^\+[0-9]{1,3}$")
            .WithMessage("Country code must begin with a + followed by 1-3 digits only");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[0-9]{9}$")
            .WithMessage("Phone Number must be 9 digits long and must consist of digits only");
        
        RuleFor(x=>x.Email)
            .EmailAddress()
            .WithMessage("Invalid email address");
        
        RuleForEach(x => x.Translations)
            .SetValidator(new CreateHotelTranslationValidator());
        
        RuleFor(x=>x.Translations)
            .Must(translations =>
                translations.Select(t => t.LanguageCode).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                == translations.Count)
            .WithMessage("A language can only appear once.")
            .NotEmpty()
            .WithMessage("At least one hotel translation is required");

    }
}