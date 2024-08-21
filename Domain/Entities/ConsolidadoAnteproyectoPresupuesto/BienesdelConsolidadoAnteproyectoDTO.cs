using Domain.Entities.ConfiguracionPresyupuestaria;
using FluentValidation;

namespace Domain.Entities.ConsolidadoAnteproyectoPresupuesto;

public class BienesdelConsolidadoAnteproyectoDTO
{ 

    public int CodigoVersion { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int CodigoAnteproyectoBien { get; set; }
    public string DescripcionCentroResponsabilidad { get; set; }
    public string DescripcionMateria { get; set; }
    public string DescripcionBien { get; set; }
    public long ValorUnitario { get; set; }
    public long Cantidad { get; set; }
    public long MontoTotal { get; set; }
}
