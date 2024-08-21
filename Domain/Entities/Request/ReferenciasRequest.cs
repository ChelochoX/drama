using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request;

public class ReferenciasRequest
{
    public int CodigoReferencia { get; set; }
    public string? DominioReferencia { get; set; }
    public int? CodigoTipoReferencia { get; set; }
    public string? DescripcionReferencia { get; set; }
    public DateTime? ValorFecha  { get; set; }
    public string? ValorAlfanumerico { get; set; }
    public string? DescripcionLarga { get; set; }
    public decimal? ValorDecimal { get; set; }
    public string? Estado { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
    public string? DescripcionTipoReferencia { get; set; }
}

public class ReferenciasRequestValidator : AbstractValidator<ReferenciasRequest>
{
    public ReferenciasRequestValidator()
    {

        RuleFor(x => x.CodigoTipoReferencia)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("El código de tipo de referencia debe ser mayor que cero.");

        //RuleFor(x => x.Estado)
        //    .Cascade(CascadeMode.Stop)
        //    .GreaterThan(0).WithMessage("El Estado debe ser mayor que cero.");

        RuleFor(x => x.Pagina)
            .GreaterThan(0).WithMessage("La página debe ser mayor que cero.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero.");
    }
}
