using FluentValidation;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion;

public class SolicitudBienesCircunscripcionRequest
{
    public int? NumeroSolicitud { get; set; }
    public string? POI { get; set; }
    public string? Circunscripcion { get; set; }
    public string? CentroResponsabilidad { get; set; }
    public string? MateriaJuridica { get; set; }
    public DateTime? FechaEmision { get; set; }
    public string? UsuarioSolicitante { get; set; }
    public string? UsuarioNombreCompleto { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int? CodigoSolicitud { get; set; }
    public  string CedulaUsuario { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
    public string? descripcionEstado { get; set; }
}

public class ParametrosRequestValidator : AbstractValidator<SolicitudBienesCircunscripcionRequest>
{
    public ParametrosRequestValidator()
    {

        RuleFor(x => x.POI)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El POI no puede contener caracteres especiales.");

        RuleFor(x => x.Circunscripcion)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("La circunscripción no puede contener caracteres especiales.");

        RuleFor(x => x.CentroResponsabilidad)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El centro de responsabilidad no puede contener caracteres especiales.");

        RuleFor(x => x.MateriaJuridica)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("La materia jurídica no puede contener caracteres especiales.");

        RuleFor(x => x.UsuarioSolicitante)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El usuario solicitante no puede contener caracteres especiales.");

        RuleFor(x => x.UsuarioNombreCompleto)
           .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El usuario UsuarioNombreCompleto no puede contener caracteres especiales.");

        //RuleFor(x => x.TerminoDeBusqueda)
        //    .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El término de búsqueda no puede contener caracteres especiales.");

        RuleFor(x => x.Pagina)
            .GreaterThan(0).WithMessage("La página debe ser mayor que cero.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero.");

    }
}