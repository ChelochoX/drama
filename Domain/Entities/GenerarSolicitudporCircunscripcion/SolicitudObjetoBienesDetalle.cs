using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion;

public class SolicitudObjetoBienesDetalle
{
    [Key]
    public int CodigoBienDetalle { get; set; }
    public string NumeroBien { get; set; }
    public string Descripcion { get; set; }
    public double CostoUnitario { get; set; }
    public int Cantidad { get; set; }
    public int UsuarioInserto { get; set; }
    public DateTime FechaInserto { get; set; }
    public int CodigoSolicitudObjeto { get; set; }
    public int CodigoSolicitud { get; set; }
    public decimal MontoTotal { get; set; }
    public int UsuarioUltimaModificacion { get; set; }
    public DateTime FechaUltimaModificacion { get; set; }
    public string Fundamentacion { get; set; }



}
