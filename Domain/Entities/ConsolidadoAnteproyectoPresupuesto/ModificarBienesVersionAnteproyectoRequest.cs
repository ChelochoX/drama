using FluentValidation;
using System.Numerics;

namespace Domain.Entities.ConsolidadoAnteproyectoPresupuesto;

public class ModificarBienesVersionAnteproyectoRequest
{ 

    public long Cantidad { get; set; }
    public long ValorUnitario { get; set; }
    public string? CedulaUsuario { get; set; }
    public int CodigoAnteproyectoBien { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoConfiguracionPresupuestaria { get; set; }
}
public class ModificarBienesVersionAnteproyectoRequestValidator : AbstractValidator<ModificarBienesVersionAnteproyectoRequest>
{
    public ModificarBienesVersionAnteproyectoRequestValidator()
    {
        RuleFor(x => x.Cantidad)
            .NotEmpty().WithMessage("La cantidad no puede estar vacía.")
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");

        RuleFor(x => x.ValorUnitario)
            .NotEmpty().WithMessage("El valor unitario no puede estar vacío.")
            .GreaterThan(0).WithMessage("El valor unitario debe ser mayor que cero.");
            
        RuleFor(x => x.CodigoAnteproyectoBien)
            .NotEmpty().WithMessage("El código de anteproyecto del bien no puede estar vacío.");

        RuleFor(x => x.CodigoAnteproyectoObjeto)
            .NotEmpty().WithMessage("El código de anteproyecto del objeto no puede estar vacío.");

        RuleFor(x => x.CodigoVersion)
            .NotEmpty().WithMessage("El código de versión no puede estar vacío.");

        RuleFor(x => x.CodigoConfiguracionPresupuestaria)
           .NotEmpty().WithMessage("El código de ConfiguracionPresupuestaria no puede estar vacío.");
    }
}
