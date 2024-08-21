using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class DatosparaConfiguracionPresupuestariaConsolidadoRequest
{
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public int Ejercicio { get; set; }
}
public class DatosparaConfiguracionPresupuestariaRequestValidator : AbstractValidator<DatosparaConfiguracionPresupuestariaConsolidadoRequest>
{
    public DatosparaConfiguracionPresupuestariaRequestValidator()
    {
        RuleFor(x => x.CodigoCentroResponsabilidad)
            .GreaterThan(0).WithMessage("El código de planificación debe ser mayor a 0.");

        RuleFor(x => x.CodigoObjetoGasto)
            .GreaterThan(0).WithMessage("El código de anteproyecto objeto debe ser mayor a 0.");

        RuleFor(x => x.Ejercicio)
            .GreaterThan(0).WithMessage("El código de versión debe ser mayor a 0.");
    }
}
