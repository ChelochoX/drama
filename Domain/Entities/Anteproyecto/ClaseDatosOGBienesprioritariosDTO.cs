using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Anteproyecto;

public class ClaseDatosOGBienesprioritariosDTO
{
    [Key]
    public int Codigo { get; set; }
    public string Descripcion { get; set; }
    public int MontoUnitario { get; set; }
    public string Fundamentacion { get; set; }
    public int NumeroBien { get; set; }
    public int UnidadMedida { get; set; }
    public int Cantidad { get; set; }
    public int MontoTotal { get; set; }
}
