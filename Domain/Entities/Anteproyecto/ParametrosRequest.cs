using FluentValidation;
using System.Text.RegularExpressions;

namespace Domain.Entities.Anteproyecto;

public class ParametrosRequest
{
    public string TerminoDeBusqueda { get; set; }
    public int? CodigoSolicitud { get; set; }
    public int Estado { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class ParametrosRequestValidator : AbstractValidator<ParametrosRequest>
{
    public ParametrosRequestValidator()
    {
        RuleFor(x => x.TerminoDeBusqueda)
            .NotEmpty().WithMessage("El término de búsqueda es obligatorio.")
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El término de búsqueda no puede contener caracteres especiales.");

        RuleFor(x => x.CodigoSolicitud)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("El código de solicitud debe ser mayor que cero.");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("El estado es obligatorio.");

        RuleFor(x => x.Pagina)
            .GreaterThan(0).WithMessage("La página debe ser mayor que cero.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero.");
    }
}