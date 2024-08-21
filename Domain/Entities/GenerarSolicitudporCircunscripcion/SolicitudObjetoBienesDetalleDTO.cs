using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion
{
    public class SolicitudObjetoBienesDetalleDTO
    {
        [Key]
        public int CodigoBienDetalle { get; set; }
        public string NumeroCatalogo { get; set; }
        public string DescripcionBienesNacional { get; set; }
        public int CostoUnitario { get; set; }
        public int Cantidad { get; set; }
        public int? UsuarioInserto { get; set; }
        public int? UsuarioUltimaModificacion { get; set; }
        public DateTime FechaInserto { get; set; }
        public DateTime FechaUltimaModificacion { get; set; }
        public int CodigoSolicitudObjeto { get; set; }
        public int CodigoSolicitud { get; set; }
        public decimal MontoTotal { get; set; }
        public string? Fundamentacion { get; set; }
        public int ejercicio { get; set; }
        public int CodigoObjetoGasto { get; set; }

    }

    public class SolicitudObjetoBienesDetalleDTOValidator : AbstractValidator<SolicitudObjetoBienesDetalleDTO>
    {
        public SolicitudObjetoBienesDetalleDTOValidator()//Actualizar Validaciones (ECU)
        {
            RuleFor(x => x.CodigoBienDetalle)
            .NotEmpty().WithMessage("El código de bien es obligatorio.")
            .GreaterThan(0).WithMessage("El código de bien debe ser mayor que cero.");

            RuleFor(x => x.NumeroCatalogo)
                .NotEmpty().WithMessage("El número de bien es obligatorio.")
                .MaximumLength(50).WithMessage("El número de bien debe tener como máximo 50 caracteres.");

            RuleFor(x => x.DescripcionBienesNacional)
                .NotEmpty().WithMessage("La descripción es obligatoria.")
                .MaximumLength(50).WithMessage("La descripción debe tener como máximo 50 caracteres.");



        }
    }

}
