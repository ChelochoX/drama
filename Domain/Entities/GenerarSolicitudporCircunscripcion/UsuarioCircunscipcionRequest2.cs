using Domain.Entities.GenerarSolicitudporCircunscripcion;
using FluentValidation;

namespace Domain.Entities.GenerarSolicitudporCircunscripcion
{
    public class UsuarioCircunscipcionRequest2
    {
        public string CedulaUsuario { get; set; }
        public int CodigoCentroResponsabilidad { get; set; }
    }
}
public class UsuarioCircunscipcionRequest2Validator : AbstractValidator<UsuarioCircunscipcionRequest2>
{
    public UsuarioCircunscipcionRequest2Validator()
    {
        RuleFor(x => x.CedulaUsuario)
            .NotNull().WithMessage("La CedulaUsuario es obligatoria.");

    }
}