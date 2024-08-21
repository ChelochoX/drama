using FluentValidation;

namespace Domain.Entities.ConfiguracionPresyupuestaria;

public class FuenteFinanciamientoRequest
{
    public int? CodigoFuenteFinanciamiento { get; set; }
    public string? NumeroFuenteFinanciamiento { get; set; }
    public string? DescripcionFuenteFinanciamiento { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class FuenteFinanciamientorValidator : AbstractValidator<FuenteFinanciamientoRequest>
{
    public FuenteFinanciamientorValidator()
    {
        RuleFor(x => x.CodigoFuenteFinanciamiento)
            .GreaterThan(0).WithMessage("El campo CodigoOrganismoFinanciador debe ser mayor que cero");

        RuleFor(x => x.NumeroFuenteFinanciamiento)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El objeto de gasto no puede contener caracteres especiales.");

        RuleFor(x => x.DescripcionFuenteFinanciamiento)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El objeto de gasto no puede contener caracteres especiales.");

        RuleFor(x => x.TerminoDeBusqueda)
         .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El objeto de gasto no puede contener caracteres especiales.");

        RuleFor(x => x.Pagina).NotEmpty().WithMessage("La página es obligatoria.");
        RuleFor(x => x.CantidadRegistros).NotEmpty().WithMessage("La cantidad de registros es obligatoria.");
    }
}