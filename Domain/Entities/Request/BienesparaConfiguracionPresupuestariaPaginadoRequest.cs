using FluentValidation;

namespace Domain.Entities.Request;

public class BienesparaConfiguracionPresupuestariaPaginadoRequest
{
    public int? NumeroBien { get; set; }
    public string? DescripcionObjetoGasto { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoVersionOBG { get; set; }
    public int ObjetoGasto { get; set; }
    public int Ejercicio { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class BienesparaConfiguracionPresupuestariaPaginadoValidator : AbstractValidator<BienesparaConfiguracionPresupuestariaPaginadoRequest>
{
    public BienesparaConfiguracionPresupuestariaPaginadoValidator()
    {
        RuleFor(x => x.ObjetoGasto)
            .NotEmpty().WithMessage("El parámetro objeto de gasto no puede estar vacío.")
            .GreaterThan(0).WithMessage("El parámetro objeto de gasto debe ser mayor que cero.");

        RuleFor(x => x.Ejercicio)
            .NotEmpty().WithMessage("El parámetro ejercicio no puede estar vacío.")
            .GreaterThan(0).WithMessage("El parámetro ejercicio debe ser mayor que cero.");

        RuleFor(x => x.Pagina).NotEmpty().WithMessage("La página es obligatoria.");
        RuleFor(x => x.CantidadRegistros).NotEmpty().WithMessage("La cantidad de registros es obligatoria.");
    }
}
