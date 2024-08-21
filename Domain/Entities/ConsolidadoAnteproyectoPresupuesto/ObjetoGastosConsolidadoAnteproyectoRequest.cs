using Domain.Entities.ConfiguracionPresyupuestaria;
using FluentValidation;

namespace Domain.Entities.ConsolidadoAnteproyectoPresupuesto;

public class ObjetoGastosConsolidadoAnteproyectoRequest
{  
    public int CodigoObjetoGasto { get; set; }
    public int CodigoFF { get; set; }
    public int CodigoOG { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoCircunscripcion { get; set; } 
}
public class ObjetoGastosConsolidadoAnteproyectoRequestValidator : AbstractValidator<ObjetoGastosConsolidadoAnteproyectoRequest>
{
    public ObjetoGastosConsolidadoAnteproyectoRequestValidator()
    {   
        RuleFor(x => x.CodigoObjetoGasto).NotNull()
           .WithMessage("El campo 'Código Objeto Gasto' no puede ser nulo.");

        RuleFor(x => x.CodigoFF).NotNull()
            .WithMessage("El campo 'Código Fuente de Financiamiento' no puede ser nulo.");

        RuleFor(x => x.CodigoOG).NotNull()
            .WithMessage("El campo 'Código Organismo' no puede ser nulo.");

        RuleFor(x => x.CodigoVersion).NotNull()
            .WithMessage("El campo 'Código Versión' no puede ser nulo.");

        RuleFor(x => x.CodigoCircunscripcion).NotNull()
            .WithMessage("El campo 'Código Circunscripción' no puede ser nulo.");               
    }
}