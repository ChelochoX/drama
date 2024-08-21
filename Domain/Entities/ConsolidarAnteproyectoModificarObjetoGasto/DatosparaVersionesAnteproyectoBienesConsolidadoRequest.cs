using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class DatosparaVersionesAnteproyectoBienesConsolidadoRequest
{
    public int CodigoAnteproyectoObjetoGasto { get; set; }
    public int CodigoAnteproyecto { get; set; }
    public int CodigoConfiguracionPresupuestaria { get; set; }
    public string NumeroBien { get; set; }
    public string DescripcionBien { get; set; }
    public int Cantidad { get; set; }
    public int ValorUnitario { get; set; }
    public string? UsuarioInserto { get; set; }
    public int? CodigoUsuarioInserto { get; set; }
    public string UnidadMedida { get; set; }
    public string? Fundamentacion { get; set; }
    public bool Seleccionado { get; set; } = false;
    public int CodigoCentroResponsabilidad { get; set; }
    public int CodigoMateria { get; set; }

}
public class DatosparaVersionesAnteproyectoBienesConsolidadoRequestValidator : AbstractValidator<DatosparaVersionesAnteproyectoBienesConsolidadoRequest>
{
    public DatosparaVersionesAnteproyectoBienesConsolidadoRequestValidator()
    {
        RuleFor(x => x.CodigoAnteproyectoObjetoGasto)
           .NotNull().WithMessage("El CodigoAnteproyectoObjetoGasto es Obligatoria");

        RuleFor(x => x.CodigoCentroResponsabilidad)
          .NotNull().WithMessage("El CodigoCentroResponsabilidad es Obligatoria");

        RuleFor(x => x.CodigoMateria)
          .NotNull().WithMessage("El CodigoMateria es Obligatoria");

        RuleFor(x => x.CodigoAnteproyecto)
           .NotNull().WithMessage("El CodigoAnteproyecto es Obligatoria");

        RuleFor(x => x.CodigoConfiguracionPresupuestaria)
         .NotNull().WithMessage("El CodigoConfiguracionPresupuestaria es Obligatoria");

        RuleFor(x => x.NumeroBien)
            .NotNull().WithMessage("El número de bien no puede ser nulo")
            .NotEmpty().WithMessage("El número de bien no puede estar vacío");

        RuleFor(x => x.DescripcionBien)
            .NotNull().WithMessage("La descripción del bien no puede ser nula")
            .NotEmpty().WithMessage("La descripción del bien no puede estar vacía");

        RuleFor(x => x.Cantidad)
            .NotNull().WithMessage("La cantidad no puede ser nula");

        RuleFor(x => x.ValorUnitario)
            .NotNull().WithMessage("El valor unitario no puede ser nulo");

        RuleFor(x => x.UsuarioInserto)
            .NotNull().WithMessage("El usuario que insertó no puede ser nulo")
            .NotEmpty().WithMessage("El usuario que insertó no puede estar vacío");

        RuleFor(x => x.UnidadMedida)
            .NotNull().WithMessage("La unidad de medida no puede ser nula")
            .NotEmpty().WithMessage("La unidad de medida no puede estar vacía");   

        RuleFor(x => x.Seleccionado)
            .NotNull().WithMessage("La selección no puede ser nula");

    }
}
