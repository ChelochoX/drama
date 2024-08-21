using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion
{
    public class SolicitudObjetoCodigoDescripcionDTO
    {



        [Key]

        public string NumeroCatalogo { get; set; }
        public string DescripcionBienesNacional { get; set; }
        public int CostoUnitario { get; set; }



    }

    public class SolicitudObjetoCodigoDescripcionDTOValidator : AbstractValidator<SolicitudObjetoCodigoDescripcionDTO>
    {
        public SolicitudObjetoCodigoDescripcionDTOValidator()//Actualizar Validaciones (ECU)
        {


            RuleFor(x => x.NumeroCatalogo)
                .NotEmpty().WithMessage("El número de bien es obligatorio.")
                .MaximumLength(50).WithMessage("El número de bien debe tener como máximo 50 caracteres.");

            RuleFor(x => x.DescripcionBienesNacional)
                .NotEmpty().WithMessage("La descripción es obligatoria.")
                .MaximumLength(50).WithMessage("La descripción debe tener como máximo 50 caracteres.");



        }
    }

}
