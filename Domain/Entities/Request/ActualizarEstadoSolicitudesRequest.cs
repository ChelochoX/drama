using FluentValidation;

namespace Domain.Entities.Request;

public class ActualizarEstadoSolicitudesRequest
{
    public int[] CodigosSolicitud { get; set; }
    public string? CedulaUsuario { get; set; }
}
public class ActualizarEstadoSolicitudesRequestValidator : AbstractValidator<ActualizarEstadoSolicitudesRequest>
{
    public ActualizarEstadoSolicitudesRequestValidator()
    {
        RuleFor(x => x.CodigosSolicitud)
            .NotEmpty().WithMessage("El código de solicitud no puede estar vacío.")
            .Must(codigos => codigos != null && codigos.All(codigo => codigo != 0))
            .WithMessage("Los códigos de solicitud deben ser diferentes de cero.");
    }
}