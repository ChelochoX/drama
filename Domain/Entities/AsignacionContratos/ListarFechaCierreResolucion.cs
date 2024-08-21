namespace Domain.Entities.AsignacionContratos;

public class ListarFechaCierreResolucion
{
    public string Descripcion { get; set; }
    public DateTime FechaCierre { get; set; }
    public int Ejercicio { get; set; }
    public string EstadoDescripcion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }   
}
