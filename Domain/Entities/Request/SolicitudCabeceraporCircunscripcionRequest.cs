using Domain.Entities.GenerarSolicitudporCircunscripcion;
using FluentValidation;

namespace Domain.Entities.Request;

public class SolicitudCabeceraporCircunscripcionRequest
{
    public DateTime FechaSolicitud { get; set; }
    public int CodigoMateria { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public string? CedulaUsuario { get; set; }
    public int Poi { get; set; }
}
public class SolicitudBienesCircunscripcionValidator : AbstractValidator<SolicitudCabeceraporCircunscripcionRequest>
{
    public SolicitudBienesCircunscripcionValidator()
    {
        RuleFor(x => x.FechaSolicitud).NotEmpty().WithMessage("La fecha de solicitud es obligatoria.");
        RuleFor(x => x.CodigoMateria).NotEmpty().WithMessage("El código de materia es obligatorio.");
        RuleFor(x => x.CodigoCentroResponsabilidad).NotEmpty().WithMessage("El código de centro de responsabilidad es obligatorio.");
        RuleFor(x => x.CodigoCircunscripcion).NotEmpty().WithMessage("El código de circunscripción es obligatorio.");
        //RuleFor(x => x.CedulaUsuario).NotEmpty().WithMessage("El usuario que insertó es obligatorio.");
        RuleFor(x => x.Poi).NotEmpty().WithMessage("El POI es obligatorio.");

        RuleFor(x => x.FechaSolicitud).NotEmpty().WithMessage("La fecha de solicitud es obligatoria.");       

    }
}