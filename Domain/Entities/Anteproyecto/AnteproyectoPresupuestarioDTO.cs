using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Anteproyecto;

public class AnteproyectoPresupuestarioDTO
{
    public int? numeroOf { get; set; }
    public int? numeroOg { get; set; }
    public int? numeroFf { get; set; }
    public int? numeroDpto { get; set; }
    public string? fundamentacion { get; set; }
    public Int64? presupuestoInicial { get; set; }
    public Int64? modificaciones { get; set; }
    public Int64? presupuestoVigente { get; set; }
    public Int64? proyectoPresupuesto { get; set; }
    public Int64? presupuestoAprobado { get; set; }
    public Int64? diferencia { get; set; }
    public int? porcentaje { get; set; }
    public Int64? totalContrato { get; set; }
    public int pagina { get; set; }
    public int cantidadRegistros { get; set; }
    public int? codigoVersion { get; set; }
    public int? codigoAnteproyectoObjeto { get; set; }
    public int? ejercicio { get; set; }
    public string descripcionMateria { get; set; }
    public string? estado { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public string? circunscripcion { get; set; }
    public string? descripcionCentroResponsabilidad { get; set; }
    public int codigoObjetoGasto { get; set; }
    public string? descripcionObjeto { get; set; }
    public int codigoOrganismoF { get; set; }
    public int codigoFuenteF { get; set; }
    public int codigoConfiguracionPresupuestaria { get; set; }


}
