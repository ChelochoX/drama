using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Domain.Entities.Anteproyecto;

public class AnteproyectoPresupuestarioCABDTO
{
    public int? numeroVersion { get; set; }
    public string? estado { get; set; }
    public int ejercicio { get; set; }
    public int codigoVersion { get; set; }
    public int EsLey { get; set; }

}
