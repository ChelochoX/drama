using FluentValidation;

namespace Domain.Entities.ConsolidadoAnteproyectoPresupuesto;

public class ObjetodeGastoVersionAnteproyectoRequest
{ 
    public int VersionConsolidado { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public int CodigoFF { get; set; }
    public int CodigoOG { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public int CodigoVersion { get; set; }
}
public class ObjetodeGastoVersionAnteproyectoRequestValidator : AbstractValidator<ObjetodeGastoVersionAnteproyectoRequest>
{
    public ObjetodeGastoVersionAnteproyectoRequestValidator()
    {
        RuleFor(x => x.VersionConsolidado)
            .NotEmpty().WithMessage("El campo VersionConsolidado es obligatorio.")
            .GreaterThan(0).WithMessage("El campo VersionConsolidado debe ser mayor a 0.");

        RuleFor(x => x.CodigoObjetoGasto)
            .NotEmpty().WithMessage("El campo CodigoObjetoGasto es obligatorio.")
            .GreaterThan(0).WithMessage("El campo CodigoObjetoGasto debe ser mayor a 0.");

        RuleFor(x => x.CodigoFF)
            .NotEmpty().WithMessage("El campo CodigoFF es obligatorio.")
            .GreaterThan(0).WithMessage("El campo CodigoFF debe ser mayor a 0.");

        RuleFor(x => x.CodigoOG)
            .NotEmpty().WithMessage("El campo CodigoOG es obligatorio.")
            .GreaterThan(0).WithMessage("El campo CodigoOG debe ser mayor a 0.");

        RuleFor(x => x.CodigoCircunscripcion)
            .NotEmpty().WithMessage("El campo CodigoCircunscripcion es obligatorio.")
            .GreaterThan(0).WithMessage("El campo CodigoCircunscripcion debe ser mayor a 0.");

        RuleFor(x => x.CodigoVersion)
            .NotEmpty().WithMessage("El campo CodigoVersion es obligatorio.")
            .GreaterThan(0).WithMessage("El campo CodigoVersion debe ser mayor a 0.");
    }
}
