using System.Text.Json.Serialization;

namespace Domain.Entities.VerificacionSolicitudes;

public class VerificacionSolicitudes
{   
    public int CodigoSolicitud { get; set; }
    public int NumeroSolicitud { get; set; }
    public string POI { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public string Circunscripcion { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public string CentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
    public string MateriaJuridica { get; set; }
    public DateTime FechaEmision { get; set; }
    public string CodigoUsuario { get; set; }
    public string UsuarioSolicitante { get; set; }
    public string UsuarioNombreCompleto { get; set; }
    public string DescripcionEstado { get; set; }
    public int Ejercicio { get; set; }
    [JsonIgnore]
    public int TotalRegistro { get; set; }
}
