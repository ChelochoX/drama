using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boolean = System.Boolean;
namespace Domain.Entities.Anteproyecto
{

    public class ActualizarEstadoVersionAnteproyectoRequest
    {
        public Boolean Esley { get; set; }
        public int CodigoVersion { get; set; }
        public int Ejercicio { get; set; }
        public string? CodigoUsuarioLoggeado { get; set; }


    }
}

