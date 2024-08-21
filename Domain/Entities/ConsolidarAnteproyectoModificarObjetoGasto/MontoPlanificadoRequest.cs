using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class MontoPlanificadoRequest
{
    public int CodigoConfiguracionPresupuestaria { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoAnteProyectoObjeto { get; set; }
}
public class MontoPlanificadoRequestValidator : AbstractValidator<MontoPlanificadoRequest>
{
    public MontoPlanificadoRequestValidator()
    {
        RuleFor(x => x.CodigoConfiguracionPresupuestaria)
            .NotNull().WithMessage("El código de ConfiguracionPresupuestaria bien no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de ConfiguracionPresupuestaria bien debe ser mayor que cero");

        RuleFor(x => x.CodigoVersion)
            .NotNull().WithMessage("El código de versión no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de versión debe ser mayor que cero");

        RuleFor(x => x.CodigoAnteProyectoObjeto)
            .NotNull().WithMessage("El código de anteproyecto objeto no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de anteproyecto objeto debe ser mayor que cero");
    }
}
