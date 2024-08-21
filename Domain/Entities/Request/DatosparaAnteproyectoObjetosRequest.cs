using FluentValidation;

namespace Domain.Entities.Request;

public class DatosparaAnteproyectoObjetosRequest
{
    public int CodigoVersionAnteproyecto { get; set; }
    public int CodigoConfiguracionPresupuestaria { get; set; }
    public int CodigoFuenteFinanciamiento { get; set; }
    public int CodigoOrganismoFinanciador { get; set; }
    public int Evento { get; set; }   
    public int CodigoObjetoGasto { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
    public int? CodigoCircunscripcionOrigen { get; set; }
}
public class DatosparaAnteproyectoObjetosRequestValidator : AbstractValidator<DatosparaAnteproyectoObjetosRequest>
{
    public DatosparaAnteproyectoObjetosRequestValidator()
    {
        RuleFor(x => x.CodigoVersionAnteproyecto).NotEmpty().WithMessage("El código de versión anteproyecto no puede estar vacío.");
        RuleFor(x => x.CodigoConfiguracionPresupuestaria).NotEmpty().WithMessage("El código de configuración presupuestaria no puede estar vacío.");
        RuleFor(x => x.CodigoFuenteFinanciamiento).NotEmpty().WithMessage("El código de fuente de financiamiento no puede estar vacío.");
        RuleFor(x => x.CodigoOrganismoFinanciador).NotEmpty().WithMessage("El código de organismo financiador no puede estar vacío.");
        RuleFor(x => x.Evento).NotEmpty().WithMessage("El evento no puede estar vacío.");
        RuleFor(x => x.CodigoObjetoGasto).NotEmpty().WithMessage("El código de objeto de gasto no puede estar vacío.");
        RuleFor(x => x.CodigoCentroResponsabilidad).NotEmpty().WithMessage("El código de centro de responsabilidad no puede estar vacío.");
        RuleFor(x => x.CodigoMateria).NotEmpty().WithMessage("El código de materia no puede estar vacío.");
        RuleFor(x => x.CodigoCircunscripcionOrigen).NotEmpty().WithMessage("El código de circunscripción de origen no puede estar vacío.");
    }
}