using FluentValidation;

namespace Domain.Entities.AsignacionContratos;

public class ListadoNotificacionesUsuarioDTO
{
    public string? TipoNotificacion { get; set; }
    public string? Mensaje { get; set; }
    public string? FechaNotificacion { get; set; }
    public string? Usuario { get; set; }
    
    public string? CircunscripcionOrigen { get; set; }

}

  
