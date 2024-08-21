using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Anteproyecto;

public class VersionAnteproyectoDTO
{
    public int CodigoVersion { get; set; }
    public int NumeroVersion { get; set; }
    public int Ejercicio { get; set; }
    public int Estado { get; set; }
    public string EstadoDescripcion { get; set; }
    public DateTime Fecha { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public int UsuarioInserto { get; set; }

    public int EsLey { get; set; }

}
