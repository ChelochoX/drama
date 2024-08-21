using Domain.Entities.VersionadoAnteproyecto;
using Domain.Entities.Request;
namespace Application.Services.Interfaces;

public interface IVersionadoAnteproyectoService
{
    Task<Datos<IEnumerable<VersionadoAnteproyectoDTO>>> ListarVersionadodeAnteproyecto(VersionadoAnteproyectoRequest request);
    Task<IEnumerable<VersionesAnteproyectoDTO>> ObtenerVersionesporEjercicio(int ejercicio);
}
