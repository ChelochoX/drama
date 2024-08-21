using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Referencias
{
    public class TipoReferenciaDTO
    {
        public int? CodigoTipoReferencia { get; set; }
        public string? DescripcionTipoReferencia { get; set; }
        public string? DominioTipoReferencia { get; set; }
        public int? Estado { get; set; }
    }
}
