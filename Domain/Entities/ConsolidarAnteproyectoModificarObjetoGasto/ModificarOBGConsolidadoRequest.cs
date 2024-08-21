using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class ModificarOBGConsolidadoRequest
{
    public int CodigoFuenteFinanciamiento { get; set; }
    public int CodigoOrganismoFinanciador { get; set; }
    public int Evento { get; set; }
    public decimal PresupuestoInicial { get; set; }
    public decimal Modificaciones { get; set; }

    public int CodigoAnteProyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int ConfiguracionPresupuestaria { get; set; }
}
public class ModificarOBGConsolidadoRequestValidator : AbstractValidator<ModificarOBGConsolidadoRequest>
{
    public ModificarOBGConsolidadoRequestValidator()
    {
        RuleFor(x => x.CodigoFuenteFinanciamiento)
          .NotNull().WithMessage("El código de fuente de financiamiento no puede ser nulo")
          .GreaterThan(0).WithMessage("El código de fuente de financiamiento debe ser mayor que cero");

        RuleFor(x => x.CodigoOrganismoFinanciador)
            .NotNull().WithMessage("El código de organismo financiador no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de organismo financiador debe ser mayor que cero");

        RuleFor(x => x.Evento)
            .NotNull().WithMessage("El evento no puede ser nulo")
              .GreaterThan(0).WithMessage("El Evento debe ser mayor que cero");

        RuleFor(x => x.PresupuestoInicial)
            .NotNull().WithMessage("El presupuesto inicial no puede ser nulo");

        RuleFor(x => x.Modificaciones)
            .NotNull().WithMessage("Las modificaciones no pueden ser nulas");           

        RuleFor(x => x.CodigoAnteProyectoObjeto)
            .NotNull().WithMessage("El código de anteproyecto objeto no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de anteproyecto objeto debe ser mayor que cero");

        RuleFor(x => x.CodigoVersion)
            .NotNull().WithMessage("El código de versión no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de versión debe ser mayor que cero");

        RuleFor(x => x.ConfiguracionPresupuestaria)
            .NotNull().WithMessage("La configuración presupuestaria no puede ser nula")
            .GreaterThan(0).WithMessage("La configuración presupuestaria debe ser mayor que cero");
    }
}
