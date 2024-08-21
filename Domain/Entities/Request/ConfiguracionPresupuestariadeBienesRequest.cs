namespace Domain.Entities.Request;

public class ConfiguracionPresupuestariadeBienesRequest
{
    public int NumeroBien { get; set; }
    public string DescripcionBien { get; set; }
    public int Cantidad { get; set; }
    public int ValorUnitario { get; set; }
    public int UsuarioInserto { get; set; }
    public string UnidadMedida { get; set; }
    public string Fundamentacion { get; set; }
    public bool Seleccionado { get; set; } = false;
    public int CodigoCentroResponsabilidad { get; set; } 
    public long CodigoMateria { get; set; } 
}
