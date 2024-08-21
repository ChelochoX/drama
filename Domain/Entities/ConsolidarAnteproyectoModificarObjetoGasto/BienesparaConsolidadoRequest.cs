using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class BienesparaConsolidadoRequest
{
   
    public int? CodigoCatalogo { get; set; }
    public string? DescripcionCatalogo { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int Ejercicio { get; set; }
    public int CodigoObjetoGasto { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class BienesparaConsolidadoRequestValidator : AbstractValidator<BienesparaConsolidadoRequest>
{
    public BienesparaConsolidadoRequestValidator()
    {
        RuleFor(x => x.Ejercicio)
            .GreaterThan(0).WithMessage("El Ejercicio debe ser mayor a 0.");

        RuleFor(x => x.CodigoObjetoGasto)
            .GreaterThan(0).WithMessage("El CodigoObjetoGasto debe ser mayor a 0.");

        RuleFor(x => x.Pagina)
           .GreaterThan(0).WithMessage("El Pagina debe ser mayor a 0.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0).WithMessage("El CantidadRegistros debe ser mayor a 0.");
    }
}
