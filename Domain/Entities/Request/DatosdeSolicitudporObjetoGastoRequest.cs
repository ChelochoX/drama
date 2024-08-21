using Domain.Entities.Request;
using FluentValidation;

namespace Domain.Entities.Request
{
    public class DatosdeSolicitudporObjetoGastoRequest
    {
        public int CodigoObjetoGasto { get; set; }
        public int Ejercicio { get; set; }
        public int CodigoMateria { get; set; }
        public int CodigoCentroResponsabilidad { get; set; }
    }
}

public class DatosparaSolicitudporOBGValidator : AbstractValidator<DatosdeSolicitudporObjetoGastoRequest>
{
    public DatosparaSolicitudporOBGValidator()
    {
        RuleFor(x => x.CodigoObjetoGasto)
            .NotNull().WithMessage("El parámetro objeto de gasto no puede ser nulo.")
            .GreaterThan(0).WithMessage("El parámetro objeto de gasto debe ser mayor que cero.");  

        RuleFor(x => x.Ejercicio)
            .NotEmpty().WithMessage("El parámetro ejercicio no puede ser nulo.")
            .GreaterThan(0).WithMessage("El parámetro ejercicio debe ser mayor que cero.");

        RuleFor(x => x.CodigoMateria)
            .NotEmpty().WithMessage("El parámetro CodigoMateria no puede ser nulo.")
            .GreaterThan(0).WithMessage("El parámetro CodigoMateria debe ser mayor que cero.");

        RuleFor(x => x.CodigoCentroResponsabilidad)
            .NotEmpty().WithMessage("El parámetro CodigoCentroResponsabilidad no puede ser nulo.")
            .GreaterThan(0).WithMessage("El parámetro CodigoCentroResponsabilidad debe ser mayor que cero.");
    }
}