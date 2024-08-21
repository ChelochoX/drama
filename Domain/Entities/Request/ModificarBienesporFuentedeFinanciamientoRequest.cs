using FluentValidation;

namespace Domain.Entities.Request;

public class ModificarBienesporFuentedeFinanciamientoRequest
{
    public int[] CodigoVersionAnteproyectoBienes { get; set; }
    public int CodigoAnteProyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
}


public class ModificarBienesporFFValidator : AbstractValidator<ModificarBienesporFuentedeFinanciamientoRequest>
{
    public ModificarBienesporFFValidator()
    {
       
        RuleFor(x => x.CodigoAnteProyectoObjeto)
            .NotEmpty().WithMessage("El campo Parametro_CodigoAnteProyectoObjeto no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_CodigoAnteProyectoObjeto debe ser mayor que cero");

        RuleFor(x => x.CodigoVersion)
            .NotEmpty().WithMessage("El campo Parametro_CodigoVersion no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_CodigoVersion debe ser mayor que cero");
    }
}