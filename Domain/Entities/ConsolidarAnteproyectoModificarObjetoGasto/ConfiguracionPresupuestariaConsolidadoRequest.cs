using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class ConfiguracionPresupuestariaConsolidadoRequest
{
    public int CodigoObjetoGasto { get; set; }
    public int CodigoPrograma { get; set; }
    public int CodigoActividad { get; set; }
    public int CodigoTipoPresupuesto { get; set; }
    public int CodigoDepartamento { get; set; }
    public int Grupo { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
    public int SubGrupo { get; set; }
    public int Ejercicio { get; set; }
}
public class ConfiguracionPresupuestariaConsolidadoRequestValidator : AbstractValidator<ConfiguracionPresupuestariaConsolidadoRequest>
{
    public ConfiguracionPresupuestariaConsolidadoRequestValidator()
    {
        RuleFor(x => x.CodigoObjetoGasto)
            .NotEmpty().WithMessage("El Código Objeto Gasto no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Objeto Gasto debe ser mayor que cero.");

        RuleFor(x => x.CodigoPrograma)
            .NotEmpty().WithMessage("El Código Programa no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Programa debe ser mayor que cero.");

        RuleFor(x => x.CodigoActividad)
            .NotEmpty().WithMessage("El Código Actividad no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Actividad debe ser mayor que cero.");

        RuleFor(x => x.CodigoTipoPresupuesto)
            .NotEmpty().WithMessage("El Código Tipo Presupuesto no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Tipo Presupuesto debe ser mayor que cero.");

        RuleFor(x => x.CodigoDepartamento)
            .NotEmpty().WithMessage("El Código Departamento no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Departamento debe ser mayor que cero.");

        RuleFor(x => x.Grupo)
            .NotEmpty().WithMessage("El Grupo no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Grupo debe ser mayor que cero.");

        RuleFor(x => x.CodigoCentroResponsabilidad)
            .NotEmpty().WithMessage("El Código Centro Responsabilidad no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Centro Responsabilidad debe ser mayor que cero.");

        RuleFor(x => x.CodigoMateria)
            .NotEmpty().WithMessage("El Código Materia no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Código Materia debe ser mayor que cero.");

        RuleFor(x => x.SubGrupo)
            .NotEmpty().WithMessage("El Sub Grupo no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Sub Grupo debe ser mayor que cero.");

        RuleFor(x => x.Ejercicio)
            .NotEmpty().WithMessage("El Ejercicio no debe estar vacío.")
            .GreaterThan(0).WithMessage("El Ejercicio debe ser mayor que cero.");

    }
}
