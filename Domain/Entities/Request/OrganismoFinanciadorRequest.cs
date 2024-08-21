using FluentValidation;
using System.Text.RegularExpressions;

namespace Domain.Entities.Request;

public class OrganismoFinanciadorRequest
{
    public int? CodigoOrganismoFinanciador { get; set; }
    public string? NumeroOrganismoFinanciador { get; set; }
    public string? DescripcionOrganismoFinanciador { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class OrganismoFinanciadorValidator : AbstractValidator<OrganismoFinanciadorRequest>
{
    public OrganismoFinanciadorValidator()
    {
        RuleFor(x => x.CodigoOrganismoFinanciador)
            .GreaterThan(0).WithMessage("El campo CodigoOrganismoFinanciador debe ser mayor que cero");

        RuleFor(x => x.NumeroOrganismoFinanciador)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El objeto de gasto no puede contener caracteres especiales.");

        RuleFor(x => x.DescripcionOrganismoFinanciador)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El objeto de gasto no puede contener caracteres especiales.");

        RuleFor(x => x.TerminoDeBusqueda)
         .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El objeto de gasto no puede contener caracteres especiales.");

        RuleFor(x => x.Pagina).NotEmpty().WithMessage("La página es obligatoria.");
        RuleFor(x => x.CantidadRegistros).NotEmpty().WithMessage("La cantidad de registros es obligatoria.");
    }
}