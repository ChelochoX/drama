namespace Domain.Entities.Sincronizacion;

public class VersionesAnteproyectosBienesSync
{
    public int CodigoAnteproyectoBien { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int NumeroBien { get; set; }
    public string DescripcionBien { get; set; }
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public string UsuarioInserto { get; set; }
    public DateTime? FechaInserto { get; set; }
    public string UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string UnidadMedida { get; set; }
    public string Fundamentacion { get; set; }
    public Boolean Seleccionado { get; set; }
    public int CodigoABOrigen { get; set; }
    public int CodigoAOOrigen { get; set; }
    public int CodigoVersionOrigen { get; set; }
}
