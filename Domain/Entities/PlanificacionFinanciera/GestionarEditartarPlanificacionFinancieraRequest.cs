using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class GestionarEditartarPlanificacionFinancieraRequest
{
    //validar Monto
    public decimal MontoPlanificacion { get; set; }

    //Validar Cantidad
    public long MontoAnterior { get; set; }

    //Para Insertar
    public int CodigoAnteproyectoPlanificacion { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int Mes { get; set; }
    public decimal Monto { get; set; }
    public string? CedulaUsuario { get; set; }   
}
public class GestionarEditartarPlanificacionFinancieraRequestValidator : AbstractValidator<GestionarEditartarPlanificacionFinancieraRequest>
{
    public GestionarEditartarPlanificacionFinancieraRequestValidator()
    {
        RuleFor(x => x.CodigoAnteproyectoObjeto)
            .GreaterThan(0).WithMessage("El código de anteproyecto objeto debe ser mayor a 0.");

        RuleFor(x => x.CodigoVersion)
            .GreaterThan(0).WithMessage("El código de versión debe ser mayor a 0.");

        RuleFor(x => x.Mes)
            .InclusiveBetween(1, 12).WithMessage("El mes debe estar entre 1 y 12.");

        RuleFor(x => x.Monto)
            .GreaterThanOrEqualTo(0).WithMessage("El monto debe ser mayor o igual a 0.");
    }
}
