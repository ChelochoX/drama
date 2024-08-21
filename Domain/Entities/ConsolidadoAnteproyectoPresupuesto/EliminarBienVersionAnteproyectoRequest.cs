using FluentValidation;

namespace Domain.Entities.ConsolidadoAnteproyectoPresupuesto;

public class EliminarBienVersionAnteproyectoRequest
{   
    public int CodigoAnteproyectoBien { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoConfiguracionPresupuestaria { get; set; }
}
public class EliminarBienVersionAnteproyectoRequestValidator : AbstractValidator<EliminarBienVersionAnteproyectoRequest>
{
    public EliminarBienVersionAnteproyectoRequestValidator()
    {   
            
        RuleFor(x => x.CodigoAnteproyectoBien)
            .NotEmpty().WithMessage("El código de anteproyecto del bien no puede estar vacío.");

        RuleFor(x => x.CodigoAnteproyectoObjeto)
            .NotEmpty().WithMessage("El código de anteproyecto del objeto no puede estar vacío.");

        RuleFor(x => x.CodigoVersion)
            .NotEmpty().WithMessage("El código de versión no puede estar vacío.");

        RuleFor(x => x.CodigoConfiguracionPresupuestaria)
            .NotEmpty().WithMessage("El código de Configuracion Presupuestaria no puede estar vacío.");
    }
}
