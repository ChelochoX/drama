using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class CodigoAnteproyectoOBJparaPlanificacionFinancieraRequest
{
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoFF { get; set; }
    public int CodigoOG { get; set; }
}
public class CodigoAnteproyectoOBJparaPlanificacionFinancieraValidator : AbstractValidator<CodigoAnteproyectoOBJparaPlanificacionFinancieraRequest>
{
    public CodigoAnteproyectoOBJparaPlanificacionFinancieraValidator()
    {
        RuleFor(x => x.CodigoCentroResponsabilidad).NotEmpty().WithMessage("el Codigo Centro Responsabilidad es obligatoria.");
        RuleFor(x => x.CodigoMateria).NotEmpty().WithMessage("El Codigo Materia es obligatoria.");
        RuleFor(x => x.CodigoObjetoGasto).NotEmpty().WithMessage("Codigo Objeto de Gasto es obligatoria.");
        RuleFor(x => x.CodigoFF).NotEmpty().WithMessage("El Codigo Fuente Financiador es obligatoria.");
        RuleFor(x => x.CodigoOG).NotEmpty().WithMessage("El Codigo Organismo Financiador es obligatoria.");
        RuleFor(x => x.CodigoVersion).NotEmpty().WithMessage("Codigo Version es obligatoria.");
    }
}
