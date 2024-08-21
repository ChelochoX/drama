namespace Domain.Entities.ConfiguracionPresyupuestaria;

public class ObjetodeGastos
{
    public ObjetodeGastos()
    {
        Bienes = new List<Bien>();
    }

    public int Id { get; set; }
    public string Descripcion { get; set; }
    public string CostoTotal { get; set; }
    public string CantidadTotal { get; set; }
    public string MontoTotal { get; set; }
    public List<Bien> Bienes { get; set; }
}
public class Bien
{
    public int Id { get; set; }
    public string Descripcion { get; set; }
    public string CostoUnitario { get; set; }
    public string Cantidad { get; set; }
    public string Monto { get; set; }
}