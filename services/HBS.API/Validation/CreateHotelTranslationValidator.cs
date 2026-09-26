using FluentValidation;
using HBS.API.Dtos.Hotels.CreateHotel;

namespace HBS.API.Validation;

public class CreateHotelTranslationValidator : AbstractValidator<CreateHotelTranslationRequest>
{
    public CreateHotelTranslationValidator()
    {
        RuleFor(translation => translation.LanguageCode)
            .NotEmpty()
            .WithMessage("Language code is required")
            .MaximumLength(10)
            .WithMessage("Language code cannot exceed 10 characters");

        RuleFor(translation => translation.Name)
            .NotEmpty()
            .WithMessage("Hotel name is required")
            .MaximumLength(150)
            .WithMessage("Hotel name cannot exceed 150 characters");

        RuleFor(translation => translation.Description)
            .MaximumLength(500)
            .WithMessage("Hotel description cannot exceed 500 characters");

        RuleFor(translation => translation.Address)
            .NotEmpty()
            .WithMessage("Hotel address is required")
            .MaximumLength(255)
            .WithMessage("Hotel address cannot exceed 255 characters");
    }
}