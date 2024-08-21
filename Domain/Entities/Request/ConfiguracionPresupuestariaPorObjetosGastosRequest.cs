using FluentValidation;

namespace Domain.Entities.Request;

public class ConfiguracionPresupuestariaPorObjetosGastosRequest
{
    public string? Poi { get; set; }
    public string? CentroResponsabilidad { get; set; }
    public string? ObjetoGasto { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int ParametroPoi { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }

    public string? DescripcionMateria { get; set; }
    public string? Departamento { get; set; }
    public string? Cantidad { get; set; }
    public string? MontoTotal { get; set; }
}
public class ConfiguracionPresupuestariaValidator : AbstractValidator<ConfiguracionPresupuestariaPorObjetosGastosRequest>
{
    public ConfiguracionPresupuestariaValidator()
    {  
        RuleFor(x => x.Pagina).NotEmpty().WithMessage("La página es obligatoria.");
        RuleFor(x => x.CantidadRegistros).NotEmpty().WithMessage("La cantidad de registros es obligatoria.");
        RuleFor(x => x.ParametroPoi).NotEmpty().WithMessage("La ParametroPoi es obligatoria.");
        RuleFor(x => x.TerminoDeBusqueda)
          .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El objeto de gasto no puede contener caracteres especiales.");
    }
}