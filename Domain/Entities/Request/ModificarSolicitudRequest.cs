namespace Domain.Entities.Request;

public class ModificarSolicitudRequest
{
    public int CodigoMateria { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int Poi { get; set; }      
    public int CodigoSolicitud { get; set; }
    public string? CedulaUsuario { get; set; }
}
