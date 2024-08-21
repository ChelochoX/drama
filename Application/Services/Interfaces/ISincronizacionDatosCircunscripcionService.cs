using Domain.Entities.Request;
using Domain.Entities.SincronizacionDatosCircunscripcion;

namespace Application.Services.Interfaces
{
    public interface ISincronizacionDatosCircunscripcionService
    {
        Task<Datos<IEnumerable<SincronizacionDatosDTO>>> ObtenerDatosOrganismoFinanciador(SincronizacionDatosRequest request);
    }
}
