using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class PresupuestoInicialyModificacionesConsolidadoRequest
{
    public int CodigoObjetoGasto { get; set; }
    public int CodigoFF { get; set; }
    public int CodigoOF { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
    public int Ejercicio { get; set; }
}
public class ObtenerPresupuestoInicialyModificacionesConsolidadoRequestValidator : AbstractValidator<PresupuestoInicialyModificacionesConsolidadoRequest>
{
    public ObtenerPresupuestoInicialyModificacionesConsolidadoRequestValidator()
    {
        RuleFor(x => x.CodigoObjetoGasto)
            .NotEmpty().WithMessage("El Código Objeto Gasto no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Objeto Gasto debe ser mayor que cero.");

        RuleFor(x => x.CodigoFF)
            .NotEmpty().WithMessage("El CodigoOFno debe estar vacío.")
            .GreaterThan(0).WithMessage("El CodigoOF debe ser mayor que cero.");

        RuleFor(x => x.CodigoOF)
            .NotEmpty().WithMessage("El CodigoOF no debe estar vacío.")
            .GreaterThan(0).WithMessage("El CodigoOF debe ser mayor que cero.");       

        RuleFor(x => x.CodigoCentroResponsabilidad)
            .NotEmpty().WithMessage("El Código Centro Responsabilidad no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Centro Responsabilidad debe ser mayor que cero.");

        RuleFor(x => x.CodigoMateria)
            .NotEmpty().WithMessage("El Código Materia no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Materia debe ser mayor que cero.");       

        RuleFor(x => x.Ejercicio)
            .NotEmpty().WithMessage("El Ejercicio no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Ejercicio debe ser mayor que cero.");

    }
}
