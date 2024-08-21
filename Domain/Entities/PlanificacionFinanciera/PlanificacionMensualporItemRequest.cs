using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class PlanificacionMensualporItemRequest
{
    public int CodigoAnteproyectoPlanificacion { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
}
public class PlanificacionMensualporItemRequestValidator : AbstractValidator<PlanificacionMensualporItemRequest>
{
    public PlanificacionMensualporItemRequestValidator()
    {
        RuleFor(x => x.CodigoAnteproyectoPlanificacion).NotEmpty().WithMessage("El Codigo Anteproyecto Planificacion es obligatorio");
        RuleFor(x => x.CodigoAnteproyectoObjeto).NotEmpty().WithMessage("El Codigo Anteproyecto Objeto es obligatorio");       
        RuleFor(x => x.CodigoVersion).NotEmpty().WithMessage("Codigo Version es obligatoria.");
    }
}
