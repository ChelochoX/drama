using Domain.Entities.Request;
using FluentValidation;

namespace Domain.Entities.Request
{
    public class DatosparaConfiguracionPresupuestariaRequest
    {
        public int CodigoObjetoGasto { get; set; }
        public int CentrodeResponsabilidad { get; set; }
        public int Ejercicio { get; set; }
        public int CodigoMateria { get; set; }
    }
}

public class DatosparaConfiguracionPresupuestariaValidator : AbstractValidator<DatosparaConfiguracionPresupuestariaRequest>
{
    public DatosparaConfiguracionPresupuestariaValidator()
    {
        RuleFor(x => x.CodigoObjetoGasto)
            .NotNull().WithMessage("El parámetro objeto de gasto no puede ser nulo.")
            .GreaterThan(0).WithMessage("El parámetro objeto de gasto debe ser mayor que cero.");

        RuleFor(x => x.CentrodeResponsabilidad)
            .NotNull().WithMessage("El parámetro centro de responsabilidad no puede ser nulo.")
            .GreaterThan(0).WithMessage("El parámetro centro de responsabilidad debe ser mayor que cero.");

        RuleFor(x => x.Ejercicio)
            .NotEmpty().WithMessage("El parámetro ejercicio no puede ser nulo.")
            .GreaterThan(0).WithMessage("El parámetro ejercicio debe ser mayor que cero.");

        RuleFor(x => x.CodigoMateria)
           .NotEmpty().WithMessage("El CodigoMateria no puede ser nulo.")
           .GreaterThan(0).WithMessage("El CodigoMateria debe ser mayor que cero.");
    }
}