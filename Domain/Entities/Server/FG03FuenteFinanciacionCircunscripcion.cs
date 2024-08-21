using Domain.Entities.Request.ConfiguracionPresupuestaria;
using FluentValidation;

namespace Domain.Entities.Server;

public class FG03FuenteFinanciacionCircunscripcion
{
 
    public int Ejercicio { get; set; }   
    public string? Usuario { get; set; }
    public int? codigoFFC { get; set; }
    public string? descripcionFFC { get; set; }
}
public class FG03FuenteFinanciacionCircunscripcionValidator : AbstractValidator<FG03FuenteFinanciacionCircunscripcion>
{
    public FG03FuenteFinanciacionCircunscripcionValidator()
    {
        

        RuleFor(x => x.Ejercicio).NotEmpty().WithMessage("El campo Ejercicio no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Ejercicio debe ser mayor que cero");       

        RuleFor(x => x.Usuario).NotEmpty().WithMessage("El campo Usuario no puede estar vacío");            

    }
}