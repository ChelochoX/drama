namespace Domain.Entities.Response;

public class FinanciamientoVersionAnteproyectoObjetosResponseDTO
{
    public int? CodigoAnteproyectoObjeto { get; set; }
    public int? CodigoOrganismoFinanciador { get; set; }
    public string? DescripcionOrganismoFinanciador { get; set; }
    public int? CodigoFuenteFinanciacion { get; set; }
    public string? DescripcionFuenteFinanciacion { get; set; }
    public long? Cantidad { get; set; }
    public decimal? Monto { get; set; }
    public int Evento { get; set; }
    public string NumeroFF { get; set; }
    public string NumeroORGF { get; set; }
}
