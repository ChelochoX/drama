using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Anteproyecto
{



    public class ActualizarVersionAnteproyectoObjetoRequest
    {
        public Int64? PresupuestoInicial { get; set; }
        public Int64? Modificaciones { get; set; }
        public string? UsuarioUltimaModificacion { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public int CodigoVersion { get; set; }
        public int CodigoAnteproyectoObjeto { get; set; }
        public string? DescripcionMateria { get; set; }
        public string? DescripcionCentroResponsabilidad { get; set; }
        public int? numero_of { get; set; }
        public int? numero_og { get; set; }
        public int? numero_ff { get; set; }
        public int? numero_dpto { get; set; }
        public string? fundamentacion { get; set; }
        public Int64? presupuesto_vigente { get; set; }
        public Int64? proyecto_presupuesto { get; set; }
        public Int64? presupuesto_aprobado { get; set; }
        public Int64? diferencia { get; set; }
        public int? porcentaje { get; set; }
        public Int64? total_contrato { get; set; }
        public int? Ejercicio { get; set; }


    }
}
