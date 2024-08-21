using FluentValidation;

namespace Domain.Entities.Request;

public class ListaObjetoGastosdeSolicitudRequest
{   
    public string? ObjetoDeGasto { get; set; }
    public string? CostoUnitario { get; set; }
    public string? CantidadTotal { get; set; }
    public string? Estado { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int? CodigoObjetoGasto { get; set; }
    public int CodigoSolicitud { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }      
}

public class ObjetoGastodeSolicitudRequestValidator : AbstractValidator<ListaObjetoGastosdeSolicitudRequest>
{
    public ObjetoGastodeSolicitudRequestValidator()
    {
        RuleFor(x => x.TerminoDeBusqueda)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El objeto de gasto no puede contener caracteres especiales.");

        RuleFor(x => x.CodigoSolicitud)
           .NotEmpty().WithMessage("El código de solicitud es obligatorio.");

        RuleFor(x => x.Pagina)
            .NotEmpty().WithMessage("La página es obligatoria.");

        RuleFor(x => x.CantidadRegistros)
            .NotEmpty().WithMessage("La cantidad de registros es obligatoria.");

        RuleFor(x => x.Estado)
          .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("La Descripcion del Estado no puede contener caracteres especiales.");

    }
}
