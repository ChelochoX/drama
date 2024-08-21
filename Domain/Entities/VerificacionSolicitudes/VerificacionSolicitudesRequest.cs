using FluentValidation;

namespace Domain.Entities.VerificacionSolicitudes;

public class VerificacionSolicitudesRequest
{
    public string? TerminoDeBusqueda { get; set; }
    public int? NumeroSolicitud { get; set; }
    public string? Poi { get; set; }
    public string? Circunscripcion { get; set; }
    public string? CentroResponsabilidad { get; set; }
    public string? MateriaJuridica { get; set; }
    public string? UsuarioSolicitante { get; set; }
    public DateTime? FechaEmision { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
    public int Ejercicio { get; set; }
}
public class VerificacionSolicitudesRequestValidator : AbstractValidator<VerificacionSolicitudesRequest>
{
    public VerificacionSolicitudesRequestValidator()
    {
        RuleFor(x => x.Pagina)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor que cero.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor que cero.");

     
    }
}
