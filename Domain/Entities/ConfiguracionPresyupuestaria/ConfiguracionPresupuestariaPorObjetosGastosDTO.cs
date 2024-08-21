using System.Text.Json.Serialization;

namespace Domain.Entities.ConfiguracionPresyupuestaria;

public class ConfiguracionPresupuestariaPorObjetosGastosDTO
{
    public string Poi { get; set; }
    public string Departamento { get; set; }
    public int CodigoCentroResposabilidad { get; set; }
    public int CodigoMateria { get; set; }
    public string DescripcionMateria { get; set; }
    public string CentroResponsabilidad { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public string ObjetoGasto { get; set; }
    public List<Financiamiento> Financiamientos { get; set; }
    public long? CantidadSolicitud { get; set; }
    public decimal? MontoSolicitud { get; set; }
    public Configuracion Configuracion { get; set; }

}

public class Financiamiento
{
    public int? CodigoAnteproyectoObjeto { get; set; }
    public string? OF { get; set; }
    public int? CodigoOrganismoFinanciador { get; set; }
    public string? FF { get; set; }
    public int? CodigoFuenteFinanciamiento { get; set; }
    public long? CantidadConfiguracion { get; set; }
    public decimal? MontoConfiguracion { get; set; }
}
public class Configuracion
{
    [JsonIgnore]
    public int? CodigoAnteproyectoObjeto { get; set; }
    public int? CodigoVersion { get; set; }
    public int? CodigoConfigPresupuestaria { get; set; }   
}