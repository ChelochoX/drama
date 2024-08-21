using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class DatosparaConfiguracionPresupuestariaConsolidadoDTO
{
    //query 1
    public string DescripcionCentroResponsabilidad { get; set; }
    public string Programa { get; set; }
    public string Actividad { get; set; }
    public int Departamento { get; set; }
    public int NumeroPresupuesto { get; set; }
    public string TipoPresupuesto { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoPrograma { get; set; }
    public int CodigoActividad { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public int CodigoDpto { get; set; }
    public int CodigoTipoPresupuesto { get; set; }

    //query 2
    public string Grupo { get; set; }
    public string SubGrupo { get; set; }
    public int CodigoGrupo { get; set; }
    public int CodigoSubgrupo { get; set; }
}

