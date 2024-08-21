using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using Boolean = System.Boolean;
namespace Domain.Entities.Anteproyecto;

public class VersionAnteproyectoBien
{
    public int CodigoVersion { get; set; }
    public int CodigoAnteproyectoBien { get; set; }
    public int CodigoAnteproyectoObjeto { get; set; }
    public int Ejercicio { get; set; }
    public int Estado { get; set; }
    public DateTime Fecha { get; set; }
    public int CodigoCircunscripcion { get; set; }

    public string NumeroBien { get; set; }
    public string Descripcion { get; set; }
    public int CostoUnitario { get; set; }
    public int Cantidad { get; set; }
    public string? UnidadMedida { get; set; }

    public string? fundamentacion { get; set; }

    public Boolean Seleccionado { get; set; }
    public int UsuarioInserto { get; set; }
    public int UsuarioModificacion { get; set; }
    public DateTime FechaInserto { get; set; }
    public DateTime FechaModificacion { get; set; }
    public int codigoMateria { get; set; }
    public int codigoCentroResponsabilidad { get; set; }

}
