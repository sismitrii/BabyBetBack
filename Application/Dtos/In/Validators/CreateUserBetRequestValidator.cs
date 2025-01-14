using FluentValidation;

namespace Application.Dtos.In.Validators;

public class CreateUserBetRequestValidator : AbstractValidator<CreateUserBetRequest>
{
    public CreateUserBetRequestValidator()
    {
        RuleFor(x => x.Gender)
            .NotNull().WithMessage("Un sexe est obligatoire");
        
        RuleFor(x => x.BirthDate)
            .NotNull().WithMessage("Une date de naissance est obligatoire")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now)).WithMessage("La date de naissance doit être supérieur à aujourd'hui");

        RuleFor(x => x.Size)
            .NotNull().WithMessage("Une taille est obligatoire")
            .GreaterThan(0).WithMessage("La taille doit être un chiffre positif. Ex: 60");

        RuleFor(x => x.NameByUser)
            .NotNull().WithMessage("Le nom de la personne qui a fait le pari est obligatoire");
        
        RuleFor(x => x.Weight)
            .NotNull().WithMessage("Un poids est obligatoire")
            .GreaterThan(0).WithMessage("Le poids doit être un chiffre positif Ex: 4.5");

        RuleFor(x => x.Names)
            .NotEmpty().WithMessage("Au moins une prénom est obligatoire")
            .Must(x => x.Count() <= 3).WithMessage("Impossible de séléctionner plus de 3 prénoms");
        
    }
}