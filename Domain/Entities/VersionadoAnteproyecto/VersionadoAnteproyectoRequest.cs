using FluentValidation;

namespace Domain.Entities.VersionadoAnteproyecto;

public class VersionadoAnteproyectoRequest
{
    public int Ejercicio { get; set; }
    public int NumeroVersion1 { get; set; }
    public int NumeroVersion2 { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
    public string? TerminoDeBusqueda { get; set; }

    public string? NumeroOBG { get; set; }
    public string? NumeroFF { get; set; }
    public string? NumeroOF { get; set; }
    public string? Fundamentacion { get; set; }
    public string? PresupuestoVigente { get; set; }
    public string? ProyectoPresupuesto { get; set; }
    public string? Diferencia { get; set; }
    public string? Porcentaje { get; set; }

}
public class VersionadoAnteproyectoRequestValidator : AbstractValidator<VersionadoAnteproyectoRequest>
{
    public VersionadoAnteproyectoRequestValidator()
    {
        RuleFor(x => x.Ejercicio)
            .NotEmpty().WithMessage("El campo Ejercicio no debe estar vacío.")
            .GreaterThan(0).WithMessage("El campo Ejercicio debe ser mayor que cero.");

        RuleFor(x => x.NumeroVersion1)
            .NotEmpty().WithMessage("El campo NumeroVersion1 no debe estar vacío.")
            .GreaterThan(0).WithMessage("El campo NumeroVersion1 debe ser mayor que cero.");

        RuleFor(x => x.NumeroVersion2)
            .NotEmpty().WithMessage("El campo NumeroVersion2 no debe estar vacío.")
            .GreaterThan(0).WithMessage("El campo NumeroVersion2 debe ser mayor que cero.");
    }
}