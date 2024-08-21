using System.Text.Json.Serialization;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion;

public class DetalleObjetoGastoporNroSolicitudDTO
{
    public int CodigoSolicitudObjeto { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public string ObjetoDeGasto { get; set; }
    public decimal CostoUnitario { get; set; }
    public int CantidadTotal { get; set; }
    public int Estado { get; set; }
    public string DescripcionEstado { get; set; }
    [JsonIgnore]
    public int TotalRegistros { get; set; }
}
