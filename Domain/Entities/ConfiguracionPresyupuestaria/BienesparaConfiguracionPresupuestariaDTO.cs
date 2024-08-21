namespace Domain.Entities.ConfiguracionPresyupuestaria;

public class BienesparaConfiguracionPresupuestariaDTO
{
    public int CodigoAnteproyectoBien { get; set; }
    public string NumeroBien { get; set; }
    public string Descripcion { get; set; }
    public int Cantidad { get; set; }
    public int ValorUnitario { get; set; }
    public decimal Total { get; set; }
    public string Fundamentacion { get; set; }
    public string UnidadMedida { get; set; }
    public bool Seleccionado { get; set; }
}
