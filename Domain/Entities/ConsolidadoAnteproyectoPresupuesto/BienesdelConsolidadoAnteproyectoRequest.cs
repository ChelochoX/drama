using Domain.Entities.ConfiguracionPresyupuestaria;
using FluentValidation;

namespace Domain.Entities.ConsolidadoAnteproyectoPresupuesto;

public class BienesdelConsolidadoAnteproyectoRequest
{
    public string? DescripcionCentroResponsabilidad { get; set; }
    public string? DescripcionMateria { get; set; }
    public string? DescripcionBien { get; set; }
    public long? ValorUnitario { get; set; }
    public long? Cantidad { get; set; }
    public long? MontoTotal { get; set; }
    public string? TerminoDeBusqueda { get; set; }


    public int CodigoObjetoGasto { get; set; }
    public int CodigoFF { get; set; }
    public int CodigoOG { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoCircunscripcion { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class BienesConsolidadoAnteproyectoRequestValidator : AbstractValidator<BienesdelConsolidadoAnteproyectoRequest>
{
    public BienesConsolidadoAnteproyectoRequestValidator()
    {       
        RuleFor(x => x.CodigoObjetoGasto).NotEmpty()
           .WithMessage("El campo 'Código Objeto Gasto' no puede ser nulo.");

        RuleFor(x => x.CodigoFF).NotEmpty()
            .WithMessage("El campo 'Código Fuente de Financiamiento' no puede ser nulo.");

        RuleFor(x => x.CodigoOG).NotEmpty()
            .WithMessage("El campo 'Código Organismo' no puede ser nulo.");

        RuleFor(x => x.CodigoVersion).NotEmpty()
            .WithMessage("El campo 'Código Versión' no puede ser nulo.");

        RuleFor(x => x.CodigoCircunscripcion).NotEmpty()
            .WithMessage("El campo 'Código Circunscripción' no puede ser nulo.");

        RuleFor(x => x.Pagina).NotEmpty().WithMessage("La página es obligatoria.");
        RuleFor(x => x.CantidadRegistros).NotEmpty().WithMessage("La cantidad de registros es obligatoria.");
    }
}