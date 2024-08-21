using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ClaseLlavePrimaria
    {
        public int CodigoConfiguracionPresupuestaria { get; set; }
        public int CodigoVersion { get; set; }
        public int CodigoAnteproyectoObjeto { get; set; }
        public ClaseLlavePrimaria()
        { }

    }
}