using FluentValidation;

namespace Domain.Entities.Request;

public class VerionesAnteproyectoObjetosEliminarRequest
{
    public int Parametro_CodigoAnteProyectoObjeto { get; set; }
    public int Parametro_CodigoVersion { get; set; }
    public int Parametro_Config_Presupuestaria { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
}
public class EliminarVersionesAnteproyectoObjetoValidator : AbstractValidator<VerionesAnteproyectoObjetosEliminarRequest>
{
    public EliminarVersionesAnteproyectoObjetoValidator()
    {

        RuleFor(x => x.Parametro_CodigoAnteProyectoObjeto)
            .NotEmpty().WithMessage("El campo Parametro_CodigoAnteProyectoObjeto no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_CodigoAnteProyectoObjeto debe ser mayor que cero");

        RuleFor(x => x.Parametro_CodigoVersion)
            .NotEmpty().WithMessage("El campo Parametro_CodigoVersion no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_CodigoVersion debe ser mayor que cero");

        RuleFor(x => x.Parametro_Config_Presupuestaria)
            .NotEmpty().WithMessage("El campo Parametro_Config_Presupuestaria no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_Config_Presupuestaria debe ser mayor que cero");

        RuleFor(x => x.CodigoCentroResponsabilidad)
           .NotEmpty().WithMessage("El campo CodigoCentroResponsabilidad no puede estar vacío")
           .GreaterThan(0).WithMessage("El campo CodigoCentroResponsabilidad debe ser mayor que cero");

        RuleFor(x => x.CodigoMateria)
           .NotEmpty().WithMessage("El campo CodigoMateria no puede estar vacío")
           .GreaterThan(0).WithMessage("El campo CodigoMateria debe ser mayor que cero");
    }
}