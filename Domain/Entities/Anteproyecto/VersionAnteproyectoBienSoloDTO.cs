namespace Domain.Entities.Anteproyecto;

public class VersionAnteproyectoBienSoloDTO
{
    public string Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? MontoUnitario { get; set; }
    public int Cantidad { get; set; }
    public string? MontoTotal { get; set; }
    public int CodigoUnidadMedida { get; set; }
    public string? UnidadMedida { get; set; }
    public int NumeroBien { get; set; }             
   
}
