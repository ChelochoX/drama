using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Referencias
{
    public class ReferenciasDTO
    {
        public int CodigoReferencia { get; set; }
        public string DominioReferencia { get; set; }
        public int CodigoTipoReferencia { get; set; }
        public string DescripcionReferencia { get; set; }
        public int Estado { get; set; }
        public DateTime? ValorFecha { get; set; }
        public string ValorAlfanumerico { get; set; }
        public string DescripcionLarga { get; set; }
        public decimal? ValorDecimal { get; set; }
        public string DescripcionTipoReferencia { get; set; }
    }
}
