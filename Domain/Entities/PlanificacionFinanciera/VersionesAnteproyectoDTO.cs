namespace Domain.Entities.PlanificacionFinanciera;

public class VersionesAnteproyectoDTO
{
    public int CodigoVersion { get; set; }
    public int NumeroVersion { get; set; }
    public int Ejercicio { get; set; }
    public int CodigoEstado { get; set; }
    public string DescripcionEstado { get; set; }
    public DateTime Fecha { get; set; }
}
