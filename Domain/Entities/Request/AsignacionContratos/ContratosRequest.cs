using FluentValidation;

namespace Domain.Entities.Request.AsignacionContratos;

public class ContratosRequest
{
    public string? NumeroContrato { get; set; }
    public string? NumeroContratacion { get; set; }
    public string? CodigoContrato { get; set; }
    public string? PacPrepac { get; set; }
    public string? DescripcionContrato { get; set; }
    public string? AdministradorContrato { get; set; }
    public string? TipoContrato { get; set; }
    public string? MontoContrato { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class ContratosRequestValidator : AbstractValidator<ContratosRequest>
{
    public ContratosRequestValidator()
    {
        //RuleFor(x => x.NumeroContrato)
        //    .Cascade(CascadeMode.Stop)
        //    .GreaterThan(0).WithMessage("El NumeroContrato debe ser mayor que cero.");

        RuleFor(x => x.Pagina)
            .GreaterThan(0).WithMessage("La página debe ser mayor que cero.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero.");
    }
}
