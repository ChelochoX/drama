namespace Domain.Entities.VersionadoAnteproyecto;

public class VersionadoAnteproyectoDTO
{
    public int NumeroOBG { get; set; }
    public int NumeroFF { get; set; }
    public int NumeroOF { get; set; }
    public string Fundamentacion { get; set; }
    public decimal PresupuestoVigente { get; set; }
    public decimal ProyectoPresupuesto1 { get; set; }
    public decimal Diferencia1 { get; set; }
    public decimal Porcentaje1 { get; set; }
    public decimal ProyectoPresupuesto2 { get; set; }
    public decimal DiferenciaVersiones { get; set; }
    public decimal Porcentaje2 { get; set; }
}
