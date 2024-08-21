using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Anteproyecto
{
    public class CatalogoCentrosResponsabilidad
    {
        public int CodigoCentroResponsabilidad { get; set; }
        public string? Descripcion { get; set; }
        public string? DescripcionAdicional { get; set; }
        public int? CodigoCircunscripcion { get; set; }
        public string? CodigoInterno { get; set; }
    }
}
