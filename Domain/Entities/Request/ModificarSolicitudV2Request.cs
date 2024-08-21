namespace Domain.Entities.Request;

public class ModificarSolicitudV2Request
{
    public int CodigoMateria { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int Poi { get; set; }
    public int UsuarioModificacion { get; set; }   
    public int CodigoSolicitud { get; set; }  
}
