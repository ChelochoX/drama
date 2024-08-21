using Application.Services.Interfaces;
using Domain.Entities.ConectarCircunscripcion;

namespace Application.Services;

public class DatosUsuarioService : IDatosUsuarioService
{
    public UsuarioInfo DatosUsuario { get; set; } = new UsuarioInfo();
}
