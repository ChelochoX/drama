using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion;

public class SolicitudObjetoDetalle
{
    [Key]
    public int CodigoSolicitudObjeto { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public DateTime FechaInserto { get; set; }
    public int UsuarioInserto { get; set; }
    public int CodigoSolicitud { get; set; }
    public int Estado { get; set; }
}
