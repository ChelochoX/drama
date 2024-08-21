using Domain.Entities.Request;
using Domain.Entities.VersionadoAnteproyecto;

namespace Application.Services.Interfaces.IRepository;

public interface IVersionadoAnteproyectoRepository
{
    Task<Datos<IEnumerable<VersionadoAnteproyectoDTO>>> ListarVersionadodeAnteproyecto(VersionadoAnteproyectoRequest request);
    Task<IEnumerable<VersionesAnteproyectoDTO>> ObtenerVersionesporEjercicio(int ejercicio);
}
