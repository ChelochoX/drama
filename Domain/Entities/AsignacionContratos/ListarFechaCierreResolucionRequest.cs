using FluentValidation;

namespace Domain.Entities.AsignacionContratos;

public class ListarFechaCierreResolucionRequest
{
    public string? Descripcion { get; set; }
    public DateTime? FechaCierre { get; set; }
    public int? Ejercicio { get; set; }
    public string? EstadoDescripcion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public String? TerminodeBusqueda { get; set; }

    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}

public class ListarFechaCierreResolucionRequestValidator : AbstractValidator<ListarFechaCierreResolucionRequest>
{
    public ListarFechaCierreResolucionRequestValidator()
    {
        RuleFor(x => x.Pagina)
            .GreaterThan(0).WithMessage("La página debe ser mayor que 0.");

        RuleFor(x => x.CantidadRegistros)
            .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que 0.");
    }
}


