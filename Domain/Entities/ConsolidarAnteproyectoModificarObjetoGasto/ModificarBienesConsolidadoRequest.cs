using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class ModificarBienesConsolidadoRequest
{
    public int NumeroBien { get; set; }
    public string DescripcionBien { get; set; }
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public string? CedulaUsuario { get; set; }
    public int? CodigoUsuarioModificacion { get; set; }
    public string? Fundamentacion { get; set; }
    public bool Seleccionado { get; set; }

    public int CodigoAnteproyectoBien { get; set; }
    public int CodigoVersion { get; set; }
    public int CodigoAnteProyectoObjeto { get; set; }
    public int CodigoConfiguracionPresupuestaria { get; set; }
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }
}
public class ModificarBienesConsolidadoRequestValidator : AbstractValidator<ModificarBienesConsolidadoRequest>
{
    public ModificarBienesConsolidadoRequestValidator()
    {
        RuleFor(x => x.NumeroBien)
            .NotNull().WithMessage("El número de bien no puede ser nulo")
            .GreaterThan(0).WithMessage("El número de bien debe ser mayor que cero");

        RuleFor(x => x.DescripcionBien)
            .NotNull().WithMessage("La descripción del bien no puede ser nula")
            .NotEmpty().WithMessage("La descripción del bien no puede estar vacía");

        RuleFor(x => x.Cantidad)
            .NotNull().WithMessage("La cantidad no puede ser nula")
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero");

        RuleFor(x => x.ValorUnitario)
            .NotNull().WithMessage("El valor unitario no puede ser nulo")
            .GreaterThanOrEqualTo(0).WithMessage("El valor unitario debe ser mayor o igual a cero");        

        RuleFor(x => x.CodigoAnteproyectoBien)
            .NotNull().WithMessage("El código de anteproyecto bien no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de anteproyecto bien debe ser mayor que cero");

        RuleFor(x => x.CodigoVersion)
            .NotNull().WithMessage("El código de versión no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de versión debe ser mayor que cero");

        RuleFor(x => x.CodigoAnteProyectoObjeto)
            .NotNull().WithMessage("El código de anteproyecto objeto no puede ser nulo")
            .GreaterThan(0).WithMessage("El código de anteproyecto objeto debe ser mayor que cero");

        RuleFor(x => x.CodigoConfiguracionPresupuestaria)
           .NotNull().WithMessage("El CodigoConfiguracionPresupuestaria no puede ser nulo")
           .GreaterThan(0).WithMessage("El CodigoConfiguracionPresupuestaria debe ser mayor que cero");

        RuleFor(x => x.CodigoMateria)
          .NotNull().WithMessage("El CodigoMateria no puede ser nulo")
          .GreaterThan(0).WithMessage("El CodigoMateria debe ser mayor que cero");

        RuleFor(x => x.CodigoCentroResponsabilidad)
          .NotNull().WithMessage("El CodigoCentroResponsabilidad no puede ser nulo")
          .GreaterThan(0).WithMessage("El CodigoCentroResponsabilidad debe ser mayor que cero");
    }
}
