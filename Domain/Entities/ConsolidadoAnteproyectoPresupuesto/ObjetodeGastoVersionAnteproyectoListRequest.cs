using FluentValidation;

namespace Domain.Entities.ConsolidadoAnteproyectoPresupuesto;

public class ObjetodeGastoVersionAnteproyectoListRequest
{
    public List<ObjetodeGastoVersionAnteproyectoDTO> Items { get; set; }
}
public class ObjetodeGastoVersionAnteproyectoListRequestValidator : AbstractValidator<ObjetodeGastoVersionAnteproyectoListRequest>
{
    public ObjetodeGastoVersionAnteproyectoListRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("La lista de objetos de gasto no puede ser nula.")
            .NotEmpty().WithMessage("La lista de objetos de gasto no puede estar vacía.")
            .Must(items => items.All(item => item != null)).WithMessage("La lista no puede contener elementos nulos.")
            .ForEach(item =>
            {
                item.NotNull().WithMessage("El objeto de gasto no puede ser nulo.");
                item.SetValidator(new ObjetodeGastoVersionAnteproyectoValidator());
            });
    }
}

public class ObjetodeGastoVersionAnteproyectoValidator : AbstractValidator<ObjetodeGastoVersionAnteproyectoDTO>
{
    public ObjetodeGastoVersionAnteproyectoValidator()
    {
        RuleFor(x => x.CodigoAnteproyectoObjeto)
            .GreaterThan(0).WithMessage("El CódigoAnteproyectoObjeto debe ser mayor a cero.");

        RuleFor(x => x.CodigoVersion)
            .GreaterThan(0).WithMessage("El CódigoVersion debe ser mayor a cero.");
    }
}
