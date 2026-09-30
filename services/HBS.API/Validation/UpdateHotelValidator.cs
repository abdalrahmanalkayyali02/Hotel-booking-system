using FluentValidation;
using HBS.API.Dtos.Hotels.UpdateHotel;

namespace HBS.API.Validation;

public class UpdateHotelValidator : AbstractValidator<UpdateHotelRequest>
{
  public UpdateHotelValidator()
  {
    RuleFor(x => x.StarRating)
      .InclusiveBetween(1.0f, 5.0f)
      .WithMessage("Star rating must be between 1.0 and 5.0")
      .Must(rating =>
        Math.Abs(rating * 10 - Math.Round(rating * 10)) < 0.0001)
      .WithMessage(
        "Star rating can have at most one decimal place.");

    RuleFor(x => x.PhoneNumberCountryCode)
      .Matches(@"^\+[0-9]{1,3}$")
      .WithMessage(
        "Country code must begin with a + followed by 1-3 digits only");

    RuleFor(x => x.PhoneNumber)
      .Matches(@"^[0-9]{9}$")
      .WithMessage(
        "Phone Number must be 9 digits long and contain digits only");

    RuleFor(x => x.Email)
      .EmailAddress()
      .When(x => !string.IsNullOrWhiteSpace(x.Email))
      .WithMessage("Invalid email address");

    RuleFor(x => x.Translations)
      .NotEmpty()
      .WithMessage("At least one translation is required");

    RuleForEach(x => x.Translations)
      .SetValidator(new UpdateHotelTranslationValidator());

    RuleFor(x => x.Translations)
      .Must(translations =>
        translations
          .Select(x => x.LanguageCode)
          .Distinct(StringComparer.OrdinalIgnoreCase)
          .Count() == translations.Count)
      .WithMessage("A language can only appear once.");
  }
}
