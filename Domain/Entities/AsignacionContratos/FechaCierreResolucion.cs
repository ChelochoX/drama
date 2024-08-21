namespace Domain.Entities.AsignacionContratos;

public class FechaCierreResolucion
{
    public int? CodigoFechaCierre { get; set; }
    public DateTime FechaCierre { get; set; }
    public int? Estado { get; set; }
    public string? EstadoDescripcion { get; set; }
    public int Ejercicio { get; set; }
    public string? Descripcion { get; set; }
    public int? UsuarioInserto { get; set; }
    public DateTime FechaInserto { get; set; }
    public int? UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
