using Domain.Entities.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request;

public class TipoReferenciasRequest
{
    public string? DescripcionTipoReferencia { get; set; }
    public string? DominioTipoReferencia { get; set; }
    public string? Estado { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}


public class TipoReferenciasRequestValidator : AbstractValidator<TipoReferenciasRequest>
{
    public TipoReferenciasRequestValidator()
    {

        //RuleFor(x => x.Estado)
        //    .Cascade(CascadeMode.Stop)
        //    .GreaterThan(0).WithMessage("El Estado debe ser mayor que cero.");

        RuleFor(x => x.Pagina)
            .GreaterThan(0).WithMessage("La página debe ser mayor que cero.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero.");
    }
}
