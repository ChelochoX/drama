using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Anteproyecto
{
    public class UbicacionPatrimonio
    {
        public int CodigoUbicacionPatrimonio { get; set; }
        public int? CodigoTipoPresupuesto { get; set; }
        public int? CodigoPrograma { get; set; }
        public int? CodigoSubprograma { get; set; }
        public int? CodigoDepartamentoPais { get; set; }
        public string? Reparticion { get; set; }
        public string? Dependencia { get; set; }
        public string? Area { get; set; }
        public string? Descripcion { get; set; }
        public string? Responsable { get; set; }
        public int? CodigoLegajoResponsable { get; set; }
        public string? Observacion { get; set; }
        public int? Ejercicio { get; set; }
        public int? Codigo2004 { get; set; }
        public string? CirAnterior { get; set; }
        public string? DptAnterior { get; set; }
        public string? DisAnterior { get; set; }
        public string? RepAnterior { get; set; }
        public string? DepAnterior { get; set; }
        public string? CarAnterior { get; set; }
        public int? CodigoMateria { get; set; }
        public int? CodigoNivel { get; set; }
        public int? NumeroFuncionarios { get; set; }
        public string? Lugar { get; set; }
        public int? CodigoSubprogramaAnterior { get; set; }
        public int? CodigoCentroResponsabilidad { get; set; }
    }
}
