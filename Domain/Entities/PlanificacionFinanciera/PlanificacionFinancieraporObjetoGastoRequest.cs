using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class PlanificacionFinancieraporObjetoGastoRequest
{
    public int CodigoObjetoGasto { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoFuenteFinanciador { get; set; }
    public int CodigoOrganismoFinanciador { get; set; }   
}
public class PlanificacionFinancieraporOBGValidator : AbstractValidator<PlanificacionFinancieraporObjetoGastoRequest>
{
    public PlanificacionFinancieraporOBGValidator()
    {
        RuleFor(x => x.CodigoObjetoGasto).NotEmpty().WithMessage("El Codigo Objeto de Gasto es obligatoria");
        RuleFor(x => x.CodigoVersion).NotEmpty().WithMessage("El Codigo Version es obligatoria");
        RuleFor(x => x.CodigoFuenteFinanciador).NotEmpty().WithMessage("El Codigo Fuente Financiador es obligatoria");
        RuleFor(x => x.CodigoOrganismoFinanciador).NotEmpty().WithMessage("El Codigo Organismo Financiador es obligatoria");
    }
}
