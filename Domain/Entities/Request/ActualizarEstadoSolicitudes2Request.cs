using FluentValidation;

namespace Domain.Entities.Request;

public class ActualizarEstadoSolicitudes2Request
{
    public int[] CodigosSolicitud { get; set; }
    public int UsuarioAnulacion { get; set; }
}
public class ActualizarEstadoSolicitudes2RequestValidator : AbstractValidator<ActualizarEstadoSolicitudes2Request>
{
    public ActualizarEstadoSolicitudes2RequestValidator()
    {
        RuleFor(x => x.CodigosSolicitud)
            .NotEmpty().WithMessage("El código de solicitud no puede estar vacío.")
            .Must(codigos => codigos != null && codigos.All(codigo => codigo != 0))
            .WithMessage("Los códigos de solicitud deben ser diferentes de cero.");

        RuleFor(x => x.UsuarioAnulacion)
            .NotEmpty().WithMessage("El usuario de anulación no puede estar vacío.")
            .GreaterThan(0).WithMessage("El usuario de anulación debe ser mayor a cero.");
    }
}