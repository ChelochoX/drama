using FluentValidation;

namespace Domain.Entities.Request;

public class ObtenerEventoAnteproyectoObjetoRequest
{   
    public int CodigoVersion { get; set; }
    public int CodigoConfiguracionPresupuestaria { get; set; }   
}
public class ObtenerEventoVersionesAnteproyectoObjetoValidator : AbstractValidator<ObtenerEventoAnteproyectoObjetoRequest>
{
    public ObtenerEventoVersionesAnteproyectoObjetoValidator()
    {         

        RuleFor(x => x.CodigoVersion)
            .NotEmpty().WithMessage("El campo Parametro_CodigoVersion no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_CodigoVersion debe ser mayor que cero");

        RuleFor(x => x.CodigoConfiguracionPresupuestaria)
            .NotEmpty().WithMessage("El campo Parametro_Config_Presupuestaria no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Parametro_Config_Presupuestaria debe ser mayor que cero");
    }
}