namespace Domain.Entities.SincronizacionDatosCircunscripcion;

public class SincronizacionDatosDTO
{
    public int CodigoTarea { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public string DescripcionCircunscripcion { get; set; }
    public string DescripcionTarea { get; set; }
    public string Estado { get; set; }
    public DateTime FechaInicio { get; set; }
    public string UsuarioEjecucion { get; set; }
    public DateTime FechaFinalizacion { get; set; }
}
