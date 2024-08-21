using Domain.Entities.Request.ConfiguracionPresupuestaria;
using FluentValidation;

namespace Domain.Entities.Server;

public class FG03Consolidado
{
    public int? CodigoFF { get; set; } = 0;
    public int Ejercicio { get; set; }
    public int CodigoVersion { get; set; }
    public string? Usuario { get; set; }
}
public class FG03ConsolidadoValidator : AbstractValidator<FG03Consolidado>
{
    public FG03ConsolidadoValidator()
    {     
        RuleFor(x => x.Ejercicio).NotEmpty().WithMessage("El campo Ejercicio no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Ejercicio debe ser mayor que cero");

        RuleFor(x => x.CodigoVersion)
            .NotEmpty().WithMessage("El campo Codigo Version no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Codigo Version debe ser mayor que cero");

        RuleFor(x => x.Usuario).NotEmpty().WithMessage("El campo Usuario no puede estar vacío");            

    }
}