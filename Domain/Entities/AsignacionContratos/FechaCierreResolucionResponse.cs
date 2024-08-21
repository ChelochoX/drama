namespace Domain.Entities.AsignacionContratos;

public class FechaCierreResolucionResponse
{
    public DateTime FechaCierre { get; set; }
    public string? EstadoDescripcion { get; set; }
    public int Ejercicio { get; set; }
    public string? Descripcion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
