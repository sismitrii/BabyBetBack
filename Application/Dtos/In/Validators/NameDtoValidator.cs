using Application.Dtos.Common;
using FluentValidation;

namespace Application.Dtos.In.Validators;

public class NameDtoValidator : AbstractValidator<NameDto>
{
    public NameDtoValidator()
    {
        RuleFor(x => x.Value)
            .Matches("^[a-zA-Z ]+$").WithMessage("Un prénom ne peut être composé que de lettres. Ex : Tarzan");
    }
}