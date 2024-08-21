namespace Domain.Entities.AsignacionContratos;

public class ContratoPorObjetosdeGasto
{
    public int CodigoAnteproyectoContrato { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoContrato { get; set; }
    public Int64? MontoContrato { get; set; }
    public int UsuarioInserto { get; set; }
    public DateTime FechaInserto { get; set; }
    public int UsuarioModificacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}
