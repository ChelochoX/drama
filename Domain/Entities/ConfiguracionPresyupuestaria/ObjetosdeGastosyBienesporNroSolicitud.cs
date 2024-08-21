namespace Domain.Entities.ConfiguracionPresyupuestaria;

public class ObjetosdeGastosyBienesporNroSolicitud
{
    public string NumeroSolicitud { get; set; }
    public string POI { get; set; }
    public string Circunscripcion { get; set; }
    public string CentroResponsabilidad { get; set; }
    public string MateriaJuridica { get; set; }
    public DateTime FechaEmision { get; set; }
    public string EstadoOG { get; set; }
    public string NroOG { get; set; }
    public string ObjetoGasto { get; set; }
    public List<Bienes> Bienes { get; set; }
}

public class Bienes
{
    public string CodigoBienDetalle { get; set; }
    public string NumeroBien { get; set; }
    public string Descripcion { get; set; }
    public decimal CostoUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal MontoTotal { get; set; }
    public string? Fundamentacion { get; set; }
}
