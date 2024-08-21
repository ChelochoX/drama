namespace Domain.Entities.Sincronizacion;

public class VersionesContratosSync
{
    public int CodigoAnteproyectoContrato { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoContrato { get; set; }
    public string UsuarioInserto { get; set; }
    public DateTime? FechaInserto { get; set; }
    public string UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public decimal MontoContrato { get; set; }
    public int CodigoACOrigen { get; set; }
    public int CodigoAOOrigen { get; set; }
    public int CodigoVersionOrigen { get; set; }
}
