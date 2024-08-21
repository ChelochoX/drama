namespace Domain.Entities.Sincronizacion;

public class VersionAnteproyectoSync
{
    public int CodigoVersion { get; set; }
    public int NumeroVersion { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public int Ejercicio { get; set; }
    public int Estado { get; set; }
    public int UsuarioInserto { get; set; }
    public DateTime? FechaInserto { get; set; }
    public int UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public int VersionConsolidado { get; set; }
    public int EsLey { get; set; }
    public int CodigoVersionOrigen { get; set; }
}
