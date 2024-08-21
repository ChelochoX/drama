namespace Domain.DTOs;

public class ContratoPorObjetosdeGastoDTO
{
    public int CodigoContrato { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public string NumeroContrato { get; set; }
    public string PacPrepac { get; set; }
    public string DescripcionContrato { get; set; }
    public string AdministradorContrato { get; set; } 
    public string TipoContrato { get; set; }
    public Int64? MontoContrato { get; set; }
    public Int64? MontoContratoInicial { get; set; }
    public int? CodigoAnteproyectoContrato { get; set; }
    
}
