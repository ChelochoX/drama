using Domain.Entities.ConfiguracionPresyupuestaria;
using FluentValidation;

namespace Domain.Entities.PlanificacionFinanciera;

public class VersionesAnteproyectoRequest
{
    public int? NumeroVersion { get; set; }
    public int? Ejercicio { get; set; }
    public string? DescripcionEstado { get; set; }
    public string? TerminoDeBusqueda { get; set; }

    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
    public DateTime? Fecha { get; set; }
}
public class VersionesAnteproyectoValidator : AbstractValidator<VersionesAnteproyectoRequest>
{
    public VersionesAnteproyectoValidator()
    {
        RuleFor(x => x.Pagina).NotEmpty().WithMessage("La página es obligatoria.");
        RuleFor(x => x.CantidadRegistros).NotEmpty().WithMessage("La cantidad de registros es obligatoria.");
    }
}