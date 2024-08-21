using FluentValidation;

namespace Domain.Entities.Request;

public class AnteproyectoPresupuestarioRequest
{

    public int? numeroOf { get; set; }
    public int? numeroOg { get; set; }
    public int? numeroFf { get; set; }
    public int? numeroDpto { get; set; }
    public string? fundamentacion { get; set; }
    public string? presupuestoInicial { get; set; }
    public string? modificaciones { get; set; }
    public string? presupuestoVigente { get; set; }
    public string? proyectoPresupuesto { get; set; }
    public string? presupuestoAprobado { get; set; }
    public string? diferencia { get; set; }
    public string? porcentaje { get; set; }
    public string? totalContrato { get; set; }
    public string? terminoDeBusqueda { get; set; }
    public int pagina { get; set; }
    public int cantidadRegistros { get; set; }
    public int? codigoVersion { get; set; }
    public int? codigoAnteproyectoObjeto { get; set; }
    public int? ejercicio { get; set; }
    public string? descripcionMateria { get; set; }
    public string? estado { get; set; }
    public string? circunscripcion { get; set; }
    public int codigoCircunscripcion { get; set; }
    public string? descripcionCentroResponsabilidad { get; set; }
    public string? descripcionObjeto { get; set; }
    public int codigoObjetoGasto { get; set; }   
    public int codigoOrganismoF { get; set; }
    public int codigoFuenteF { get; set; }
    public int codigoConfiguracionPresupuestaria { get; set; }

}

public class AnteproyectoPresupuestarioRequestValidator : AbstractValidator<AnteproyectoPresupuestarioRequest>
{
    public AnteproyectoPresupuestarioRequestValidator()
    {


        RuleFor(x => x.numeroOf)
              .GreaterThan(0).WithMessage("El numero_of debe ser mayor que cero.");

        RuleFor(x => x.numeroOg)
            .GreaterThan(0).WithMessage("El numero_og de versión debe ser mayor que cero.");

        RuleFor(x => x.numeroFf)
            .GreaterThan(0).WithMessage("El numero_ff debe ser mayor que cero.");

        RuleFor(x => x.terminoDeBusqueda)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El término de búsqueda no puede contener caracteres especiales.");

        RuleFor(x => x.pagina)
                 .GreaterThan(0).WithMessage("La página debe ser mayor que cero.");

        RuleFor(x => x.cantidadRegistros)
            .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero.");

    }
}
