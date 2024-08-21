using Domain.Entities.ConectarCircunscripcion;
using Domain.Entities.Request;

namespace Application.Services.Interfaces;

public interface IConectarCircunscripcionService
{
    Task<string> ObtenerDatosparaConectarCircunscripcion(int codigoCircunscripcion);
}
