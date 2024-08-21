using Domain.Entities.Request.ConfiguracionPresupuestaria;
using FluentValidation;

namespace Domain.Entities.Server;

public class JustificacionCreditosPresupuestarios
{
 
    public int Ejercicio { get; set; }   
    public string? Usuario { get; set; }
    public int materia { get; set; }
    public int? codigoCR { get; set; }
    public string? descripcionCR { get; set; }
    public int? codigoCircunscripcion { get; set; }
    
}
public class JustificacionCreditosPresupuestariosValidator : AbstractValidator<JustificacionCreditosPresupuestarios>
{
    public JustificacionCreditosPresupuestariosValidator()
    {
        

        RuleFor(x => x.Ejercicio).NotEmpty().WithMessage("El campo Ejercicio no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Ejercicio debe ser mayor que cero");       

        RuleFor(x => x.Usuario).NotEmpty().WithMessage("El campo Usuario no puede estar vacío");            

    }
}