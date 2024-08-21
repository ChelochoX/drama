using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion;

public class SolicitudBienesCircunscripcion
{
    [Key]
    public int CodigoSolicitud { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public int Ejercicio { get; set; }
    public int CodigoMateria { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public int UsuarioInserto { get; set; }
    public DateTime FechaInserto { get; set; }
    public int UsuarioUltimaModificacion { get; set; }
    public DateTime? FechaUltimaModificacion { get; set; }
    public int UsuarioAnulacion { get; set; }
    public DateTime? FechaAnulacion { get; set; } // Puede ser nulo
    public int NumeroSolicitud { get; set; }
    public int Estado { get; set; }
    public int Poi { get; set; }
}
