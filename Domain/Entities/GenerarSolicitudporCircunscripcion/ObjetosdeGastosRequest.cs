using FluentValidation;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion;

public class ObjetosdeGastosRequest
{   
    public string? NumeroObjetoGasto { get; set; }
    public string? DescripcionObjetoGasto { get; set; }
    public int Ejercicio { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
}

public class ListarObjetosdeGastosRequestValidator : AbstractValidator<ObjetosdeGastosRequest>
{
    public ListarObjetosdeGastosRequestValidator()
    {     

        RuleFor(x => x.TerminoDeBusqueda)
            .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("lA expresion ingresada no puede contener caracteres especiales.");

        RuleFor(x => x.NumeroObjetoGasto)
           .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El NumeroObjetoGasto no puede contener caracteres especiales.");

        RuleFor(x => x.DescripcionObjetoGasto)
           .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El DescripcionObjetoGasto no puede contener caracteres especiales.");    

        RuleFor(x => x.Pagina)
            .NotEmpty().WithMessage("La página es obligatoria.");

        RuleFor(x => x.CantidadRegistros)
            .NotEmpty().WithMessage("La cantidad de registros es obligatoria.");        
    }
}