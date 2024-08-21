using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class EliminarPlanificacionFinancieraRequest
{
    public int CodigoAnteproyectoPlanificacion { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }   
}
public class EliminarPlanificacionFinancieraRequestValidator : AbstractValidator<EliminarPlanificacionFinancieraRequest>
{
    public EliminarPlanificacionFinancieraRequestValidator()
    {
        RuleFor(x => x.CodigoAnteproyectoPlanificacion)
            .GreaterThan(0).WithMessage("El código de planificación debe ser mayor a 0.");

        RuleFor(x => x.CodigoAnteproyectoObjeto)
            .GreaterThan(0).WithMessage("El código de anteproyecto objeto debe ser mayor a 0.");

        RuleFor(x => x.CodigoVersion)
            .GreaterThan(0).WithMessage("El código de versión debe ser mayor a 0.");       
    }
}
