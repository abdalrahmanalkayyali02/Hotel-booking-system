using FluentValidation;
using HBS.API.Dtos.Hotels.UpdateHotel;

namespace HBS.API.Validation;

public class UpdateHotelTranslationValidator : AbstractValidator<UpdateHotelTranslationRequest>
{
  public UpdateHotelTranslationValidator()
  {
    RuleFor(x => x.LanguageCode)
      .NotEmpty()
      .WithMessage("Language code is required")
      .MaximumLength(10);

    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Hotel name is required")
      .MaximumLength(150);

    RuleFor(x => x.Description)
      .MaximumLength(500);

    RuleFor(x => x.Address)
      .NotEmpty()
      .WithMessage("Address is required")
      .MaximumLength(255);
  }
}

