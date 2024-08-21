using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class InsertarPlanificacionFinancieraRequest
{
    public int CodigoAnteproyectoPlanificacion { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int Mes { get; set; }
    public decimal Monto { get; set; }
    public int UsuarioInserto { get; set; }   
}
public class InsertarPlanificacionFinancieraRequestValidator : AbstractValidator<InsertarPlanificacionFinancieraRequest>
{
    public InsertarPlanificacionFinancieraRequestValidator()
    {
        RuleFor(x => x.CodigoAnteproyectoPlanificacion)
            .GreaterThan(0).WithMessage("El código de planificación debe ser mayor a 0.");

        RuleFor(x => x.CodigoAnteproyectoObjeto)
            .GreaterThan(0).WithMessage("El código de anteproyecto objeto debe ser mayor a 0.");

        RuleFor(x => x.CodigoVersion)
            .GreaterThan(0).WithMessage("El código de versión debe ser mayor a 0.");

        RuleFor(x => x.Mes)
            .InclusiveBetween(1, 12).WithMessage("El mes debe estar entre 1 y 12.");

        RuleFor(x => x.Monto)
            .GreaterThanOrEqualTo(0).WithMessage("El monto debe ser mayor o igual a 0.");

        RuleFor(x => x.UsuarioInserto)
            .GreaterThan(0).WithMessage("El código de usuario que inserta debe ser mayor a 0."); 
    }
}
