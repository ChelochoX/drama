using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion
{
    public class SolicitudesObjetosBienesDTO
    {
        public int CodigoSolicitud { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int Ejercicio { get; set; }
        public int CodigoMateria { get; set; }
        public int CodigoCentroResponsabilidad { get; set; }
        public int CodigoCircunscripcion { get; set; }
        public string UsuarioInserto { get; set; }
        public DateTime FechaInserto { get; set; }
        public string? UsuarioUltimaModificacion { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public string? UsuarioAnulacion { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public int NumeroSolicitud { get; set; }
        public int Estado { get; set; }
        public int Poi { get; set; }
        public int CodigoObjetoGasto { get; set; }
    }
}
