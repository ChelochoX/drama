namespace Domain.Entities.ConsolidadoAnteproyectoPresupuesto;

public class ObjetoGastosConsolidadoAnteproyectoDTO
{
    public int Ejercicio { get; set; }
    public string NombreCircunscripcion { get; set; }
    public int NumeroOBG { get; set; }
    public int NumeroFF { get; set; }
    public int NumeroOG { get; set; }
    public string Fundamentacion { get; set; }
    public int CodigoConfigPresupuestaria { get; set; }
    public decimal PresupuestoInicial { get; set; }
    public decimal Modificaciones { get; set; }
    public decimal PersupuestoVigente { get; set; }
    public decimal ProyectoPresupuesto { get; set; }
    public decimal Diferencia { get; set; }

}
