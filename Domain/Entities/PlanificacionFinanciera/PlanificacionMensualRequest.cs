using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class PlanificacionMensualRequest
{  
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
}
public class PlanificacioMensualValidator : AbstractValidator<PlanificacionMensualRequest>
{
    public PlanificacioMensualValidator()
    {
        RuleFor(x => x.CodigoAnteproyectoObjeto).NotEmpty().WithMessage("El Codigo Anteproyecto Objeto es obligatorio");       
        RuleFor(x => x.CodigoVersion).NotEmpty().WithMessage("Codigo Version es obligatoria.");
    }
}
