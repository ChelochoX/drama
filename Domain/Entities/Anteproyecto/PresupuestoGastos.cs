using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Boolean = System.Boolean;

namespace Domain.Entities.Anteproyecto;

public class PresupuestoGastos
{
    public int CodigoPresupuesto { get; set; }    
    public int CodigoObjetoGasto { get; set; } 
    public int CodigoTipoPresupuesto { get; set; }
    public int CodigoPrograma { get; set; }
    public int CodigoFuenteFinanciamiento	{ get; set; }
    public int CodigoOrganismoFinanciador { get; set; }
    public Boolean Activo { get; set; }
    public double MontoPresupuesto { get; set; }	
    public int Ejercicio { get; set; }
    public int CodigoActividad { get; set; }
    public int CodigoDepartamento { get; set; }


}
