using Domain.Entities.ConectarCircunscripcion;
using Domain.Entities.Request;

namespace Application.Services.Interfaces.IRepository;

public interface IConectarCircunscripcionRepository
{
    Task<ConectarCircunscripcion> ObtenerDatosparaConectarCircunscripcion(int codigoCircunscripcion);
    Task<string> ObtenerBaseDeDatosCircunscripcion(int codigoCircunscripcion);
}
