using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class PlanificacionFinancieraRequest
{
    public int? Ejercicio { get; set; }
    public int? NumeroObjetoGasto { get; set; }
    public int? NumeroOrgFinanciador { get; set; }
    public int? NumeroFuenteFinanciador { get; set; }
    public int? NumeroDpto { get; set; }
    public string? Fundamentacion { get; set; }
    public string? PresupuestoAprobado { get; set; }
    public string? Diferencia { get; set; }
   
    public string? TotalMensual { get; set; }
    public string? TerminoDeBusqueda { get; set; }

    public int CodigoVersion { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class PlanificacionFinancieraValidator : AbstractValidator<PlanificacionFinancieraRequest>
{
    public PlanificacionFinancieraValidator()
    {
        RuleFor(x => x.Pagina).NotEmpty().WithMessage("La página es obligatoria.");
        RuleFor(x => x.CantidadRegistros).NotEmpty().WithMessage("La cantidad de registros es obligatoria.");
        RuleFor(x => x.CodigoVersion).NotEmpty().WithMessage("Codigo Version es obligatoria.");
    }
}
