using FluentValidation;

namespace Domain.Entities.Server;

public class ConsultaVersionadoAnteproyecto
{
    public string? Usuario { get; set; }
    public int NumeroVersion1 { get; set; }
    public int NumeroVersion2 { get; set; }
    public int Ejercicio { get; set; }
}
public class ConsultaVersionadoAnteproyectoValidator : AbstractValidator<ConsultaVersionadoAnteproyecto>
{
    public ConsultaVersionadoAnteproyectoValidator()
    {
        RuleFor(x => x.NumeroVersion1).NotEmpty().WithMessage("El campo Numero Version 1  no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Numero Version 1  debe ser mayor que cero");

        //RuleFor(x => x.Ejercicio).NotEmpty().WithMessage("El campo Ejercicio no puede estar vacío")
        //    .GreaterThan(0).WithMessage("El campo Ejercicio debe ser mayor que cero");

        RuleFor(x => x.NumeroVersion2)
            .NotEmpty().WithMessage("El campo Numero Version 2 no puede estar vacío")
            .GreaterThan(0).WithMessage("El campo Numero Version 2 debe ser mayor que cero");

        RuleFor(x => x.Usuario).NotEmpty().WithMessage("El campo Usuario no puede estar vacío");

    }
}
