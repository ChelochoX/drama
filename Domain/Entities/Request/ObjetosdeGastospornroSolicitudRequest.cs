using FluentValidation;

namespace Domain.Entities.Request;

public class ObjetosdeGastospornroSolicitudRequest
{ 
    public int CodigoObjetoGasto { get; set; }
    public string? CedulaUsuario { get; set; }
    public int CodigoSolicitud { get; set; } 
}
public class ObjetosdeGastospornroSolicitudRequestValidator : AbstractValidator<ObjetosdeGastospornroSolicitudRequest>
{
    public ObjetosdeGastospornroSolicitudRequestValidator()
    {
        RuleFor(x => x.CodigoObjetoGasto)
         .GreaterThan(0).WithMessage("El código del objeto de gasto debe ser mayor que cero.")
         .NotEmpty().WithMessage("El código del objeto de gasto no puede estar vacío.");      

        RuleFor(x => x.CodigoSolicitud)
            .GreaterThan(0).WithMessage("El código de solicitud debe ser mayor que cero.")
            .NotEmpty().WithMessage("El código de solicitud no puede estar vacío.");

    }
}