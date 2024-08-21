namespace Domain.Entities.Sincronizacion;

public class VersionesAnteproyectosConfBienesSync
{
    public int CodigoABOrigen { get; set; }
    public int CodigoAOOrigen { get; set; }
    public int CodigoVersionOrigen { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoAnteproyectoBien { get; set; }
    public int CodigoVersion { get; set; }
    public int NumeroBien { get; set; }
    public string? DescripcionBien { get; set; }
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public int? UsuarioInserto { get; set; }
    public DateTime? FechaInserto { get; set; }
    public int? UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UnidadMedida { get; set; }
    public string? Fundamentacion { get; set; }
    public int Seleccionado { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
}
