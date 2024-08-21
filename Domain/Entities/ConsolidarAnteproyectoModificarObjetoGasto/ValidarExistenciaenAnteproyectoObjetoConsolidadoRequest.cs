using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class ValidarExistenciaenAnteproyectoObjetoConsolidadoRequest
{
    public int CodigoOBG { get; set; }
    public int CodigoFF { get; set; }
    public int CodigoOF { get; set; }
    public decimal CodigoCentro { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoCircunscripcion { get; set; }

}
public class ValidarExistenciaenAnteproyectoObjetoConsolidadoRequestValidator : AbstractValidator<ValidarExistenciaenAnteproyectoObjetoConsolidadoRequest>
{
    public ValidarExistenciaenAnteproyectoObjetoConsolidadoRequestValidator()
    {
        RuleFor(x => x.CodigoOBG)
           .NotNull().WithMessage("El código OBG no puede ser nulo")
           .GreaterThan(0).WithMessage("El código OBG debe ser mayor que cero");

        RuleFor(x => x.CodigoFF)
            .NotNull().WithMessage("El código FF no puede ser nulo")
            .GreaterThan(0).WithMessage("El código FF debe ser mayor que cero");

        RuleFor(x => x.CodigoOF)
            .NotNull().WithMessage("El código OF no puede ser nulo")
            .GreaterThan(0).WithMessage("El código OF debe ser mayor que cero");

        RuleFor(x => x.CodigoCentro)
            .NotNull().WithMessage("El código Centro no puede ser nulo")
            .GreaterThan(0).WithMessage("El código Centro debe ser mayor que cero");

        RuleFor(x => x.CodigoVersion)
            .NotNull().WithMessage("El código de versión no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de versión debe ser mayor que cero");

        RuleFor(x => x.CodigoCircunscripcion)
            .NotNull().WithMessage("El código de circunscripción no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de circunscripción debe ser mayor que cero");

    }
}
