using Domain.Entities.Request;
using Domain.Entities.SincronizacionDatosCircunscripcion;

namespace Application.Services.Interfaces.IRepository;

public interface ISincronizacionDatosCircunscripcionRepository
{
    Task<Datos<IEnumerable<SincronizacionDatosDTO>>> ObtenerDatosOrganismoFinanciador(SincronizacionDatosRequest request);
}
