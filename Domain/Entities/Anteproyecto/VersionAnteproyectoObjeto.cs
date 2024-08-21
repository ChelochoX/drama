using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Domain.Entities.Anteproyecto;

public class VersionAnteproyectoObjeto
{
    public int CodigoVersion { get; set; }    
    public int CodigoAnteproyectoObjeto { get; set; } 
    public int CodigoCircunscripcionOrigen { get; set; }

    public int CodigoConfiguracionPresupuestaria { get; set; }
    public int CodigoFuenteFinanciamiento	{ get; set; }
    public int CodigoOrganismoFinanciador { get; set; }
    public int Evento { get; set; }
    public Int64 PresupuestoInicial { get; set; }
	public Int64 Modificaciones { get; set; }
	public Int64 MontoPlanificado { get; set; }












}
