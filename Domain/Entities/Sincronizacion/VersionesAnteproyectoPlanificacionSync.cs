namespace Domain.Entities.Sincronizacion;

public class VersionesAnteproyectoPlanificacionSync
{
    public int CodigoAnteproyectoPlanificacion { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int Mes { get; set; }
    public decimal Monto { get; set; }
    public int UsuarioInserto { get; set; }
    public DateTime? FechaInserto { get; set; }
    public int UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public int CodigoAOOrigen { get; set; }
    public int CodigoAPOrigen { get; set; }
    public int CodigoVersionOrigen { get; set; }
}
