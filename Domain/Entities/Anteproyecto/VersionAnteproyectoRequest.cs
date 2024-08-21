using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Anteproyecto;

public class VersionAnteproyectoRequest
{
    public int? CodigoVersion { get; set; }
    public int? Ejercicio { get; set; }
    public string? Estado { get; set; }
    public DateTime? Fecha { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
    public int? NumeroVersion { get; set; } 

}

public class VersionAnteproyectoRequestValidator : AbstractValidator<VersionAnteproyectoRequest>
{
    public VersionAnteproyectoRequestValidator()
    {


        RuleFor(x => x.Ejercicio)
              .Cascade(CascadeMode.Stop)
              .GreaterThan(0).WithMessage("El Ejercicio debe ser mayor que cero.");

        RuleFor(x => x.CodigoVersion)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("El Número de versión debe ser mayor que cero.");
              RuleFor(x => x.TerminoDeBusqueda)
            .Matches("^[A-Za-z0-9\\s.,_\\\\/\\-]*$").WithMessage("El término de búsqueda no puede contener caracteres especiales.");

        RuleFor(x => x.Pagina)
                 .GreaterThan(0).WithMessage("La página debe ser mayor que cero.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero.");

    }
}
