using Domain.Entities.GenerarSolicitudporCircunscripcion;
using FluentValidation;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion
{
    public class UsuarioCircunscipcion_Request
    {
        public int CodigoUsuario { get; set; }
        public int Codigocircunscripcion { get; set; }
    }
}
public class UsuarioCircunscripcionRequestValidator : AbstractValidator<UsuarioCircunscipcion_Request>
{
    public UsuarioCircunscripcionRequestValidator()
    {
        RuleFor(x => x.CodigoUsuario)
            .NotEmpty().WithMessage("La página es obligatoria.");

        RuleFor(x => x.Codigocircunscripcion)
            .NotEmpty().WithMessage("La cantidad de registros es obligatoria.");

    }
}