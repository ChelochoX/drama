namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class BienesdeAnteproyectoBienesConsolidadoDTO
{
    public int CodigoAnteproyectoBien { get; set; }
    public int NumeroBien { get; set; }
    public string DescripcionBien { get; set; }
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public string UnidadMedida { get; set; }
    public string? Fundamentacion { get; set; }
    public bool Seleccionado { get; set; }
    public Totales Totales { get; set; } = new Totales();
}
public class Totales
{
    public long CantidadTotal { get; set; }
    public decimal MontoTotal { get; set; }
}