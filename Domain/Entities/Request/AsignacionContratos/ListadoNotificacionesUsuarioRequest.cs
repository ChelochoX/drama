using FluentValidation;

namespace Domain.Entities.Request.AsignacionContratos;

public class ListadoNotificacionesUsuarioRequest
{
    public string? TipoNotificacion { get; set; }
    public string? Mensaje { get; set; }
    public string? FechaNotificacion { get; set; }
    public string? Usuario { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public string? CircunscripcionOrigen { get; set; }
    public int CantidadRegistros { get; set; }
    public int Pagina { get; set; }
    public string? UserName { get; set; } 
}

public class ListadoNotificacionesUsuarioRequestValidator : AbstractValidator<ListadoNotificacionesUsuarioRequest>
{
    public ListadoNotificacionesUsuarioRequestValidator()
    {
        RuleFor(x => x.Pagina)
            .GreaterThan(0).WithMessage("La página debe ser mayor que cero.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero.");
    }
}

