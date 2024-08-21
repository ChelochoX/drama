namespace Domain.Entities.PlanificacionFinanciera;

public class PlanificacionFinancieraDTO
{
    public int CodigoVersion { get; set; }
    public int Ejercicio { get; set; }
    public int CodigoOBG { get; set; }
    public int NumeroObjetoGasto { get; set; }
    public int NumeroFuenteFinanciador { get; set; }
    public int CodigoFF { get; set; }
    public int NumeroOrgFinanciador { get; set; }
    public int CodigoOF { get; set; }
    public int NumeroDpto { get; set; }
    public string Fundamentacion { get; set; }
    public decimal PresupuestoAprobado { get; set; }
    public long Enero { get; set; }
    public long Febrero { get; set; }
    public long Marzo { get; set; }
    public long Abril { get; set; }
    public long Mayo { get; set; }
    public long Junio { get; set; }
    public long Julio { get; set; }
    public long Agosto { get; set; }
    public long Septiembre { get; set; }
    public long Octubre { get; set; }
    public long Noviembre { get; set; }
    public long Diciembre { get; set; }
    public decimal TotalMensual { get; set; }
    public decimal Diferencia { get; set; }

}
