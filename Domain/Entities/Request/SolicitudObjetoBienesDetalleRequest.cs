using Domain.Entities.Request;
using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Request;

public class SolicitudObjetoBienesDetalleRequest
{
    [Key]


    public string? CodigoBienDetalle { get; set; }
    public string? NumeroBien { get; set; }
    public string? DescripcionBienesNacional { get; set; }
    public string? CostoUnitario { get; set; }
    public string? Cantidad { get; set; }
    public int? UsuarioInserto { get; set; }
    public string? TerminoDeBusqueda { get; set; }
    public int? UsuarioUltimaModificacion { get; set; }
    public DateTime FechaInserto { get; set; }
    public DateTime FechaUltimaModificacion { get; set; }
    public int? CodigoSolicitudObjeto { get; set; }
    public int? CodigoObjetoGasto { get; set; }    
    public int? CodigoSolicitud { get; set; }
    public string? MontoTotal { get; set; }
    public decimal? Monto { get; set; }
    public int Pagina { get; set; }
    public int CantidadRegistros { get; set; }
    public int Ejercicio { get; set; }
    public string? Fundamentacion { get; set; }
    public string? NumeroCatalogo { get; set; }
    public string? ValorUnitario { get; set; }
    


    public class ParametrosRequestValidator : AbstractValidator<SolicitudObjetoBienesDetalleRequest>
    {
        public ParametrosRequestValidator()
        {

            RuleFor(x => x.NumeroBien)
                .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("El NumeroBien no puede contener caracteres especiales.");

            RuleFor(x => x.DescripcionBienesNacional)
                .Matches("^[A-Za-z0-9\\s.,_-]*$").WithMessage("La circunscripción no puede contener caracteres especiales.");


            RuleFor(x => x.Pagina)
                .GreaterThan(0).WithMessage("La página debe ser mayor que cero.");

            RuleFor(x => x.UsuarioInserto)
                .GreaterThan(0).WithMessage("El usuario debe ser mayor que cero.");
            RuleFor(x => x.CantidadRegistros)
                .GreaterThan(0).WithMessage("La cantidad de registros debe ser mayor que cero.");
        }
    }
}
