using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class MesesparaPlanificacionFinancieraRequest
{
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; } 
}
public class MesesparaPlanificacionFinancieraRequestValidator : AbstractValidator<MesesparaPlanificacionFinancieraRequest>
{
    public MesesparaPlanificacionFinancieraRequestValidator()
    {
        RuleFor(x => x.CodigoAnteproyectoObjeto).NotEmpty().WithMessage("El Codigo Anteproyecto Objeto es obligatoria");
        RuleFor(x => x.CodigoVersion).NotEmpty().WithMessage("El Codigo Version es obligatoria");
    }
}
