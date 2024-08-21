namespace Domain.Entities.PlanificacionFinanciera;

public class PlanificacionMensualDTO
{
    public int CodigoAnteproyectoPlanificacion { get; set; }
    public int NumeroMes { get; set; }
    public string Mes { get; set; }
    public decimal Monto { get; set; }
}
