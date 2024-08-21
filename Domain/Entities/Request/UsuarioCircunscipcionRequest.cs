using FluentValidation;

namespace Domain.Entities.Request;

public class UsuarioCircunscipcionRequest
{
    public int? CodigoCentroResponsabilidad { get; set; }
    public string? DescripcionCentroResponsabilidad { get; set; }
    public int? CodigoMateria { get; set; }
    public string? DescripcionMateria { get; set; }
    public int CodigoUsuario { get; set; }
    public int Codigocircunscripcion { get; set; }
    public string? TerminoDeBusqueda { get; set; }  
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}
public class UsuarioCircunscipcionRequestValidator : AbstractValidator<UsuarioCircunscipcionRequest>
{
    public UsuarioCircunscipcionRequestValidator()
    {
        RuleFor(x => x.TerminoDeBusqueda)
                   .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("La expresión ingresada no puede contener caracteres especiales.");

        RuleFor(x => x.DescripcionCentroResponsabilidad)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("La descripción del centro de responsabilidad no puede contener caracteres especiales.");

        RuleFor(x => x.DescripcionMateria)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("La descripción de la materia no puede contener caracteres especiales.");

        RuleFor(x => x.Pagina)
            .NotEmpty().WithMessage("La página es obligatoria.");

        RuleFor(x => x.CantidadRegistros)
            .NotEmpty().WithMessage("La cantidad de registros es obligatoria.");

    }
}