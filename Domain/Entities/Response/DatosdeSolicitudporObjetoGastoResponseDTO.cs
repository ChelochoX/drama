namespace Domain.Entities.Response;

public class DatosdeSolicitudporObjetoGastoResponseDTO
{
    public string ObjetoGasto { get; set; }
    public int CodigoSolicitud { get; set; }
    public int NroSolicitud { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public string DescripcionCircunscripcion { get; set; }
    public DateTime FechaEmision { get; set; }
    public string UsuarioSolicitante { get; set; }
    public int Año { get; set; }    
    public string CentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
    public string MateriaJuridica { get; set; }
    public decimal Montototal { get; set; }
    public int CantidadTotal { get; set; }
}
