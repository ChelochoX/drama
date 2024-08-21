using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using Domain.Entities.Request;
using Domain.Entities.SincronizacionDatosCircunscripcion;

namespace Application.Services;

public class SincronizacionDatosCircunscripcionService : ISincronizacionDatosCircunscripcionService
{
    private readonly ISincronizacionDatosCircunscripcionRepository _repository;
    private readonly IMapper _mapper;

    public SincronizacionDatosCircunscripcionService(IMapper mapper, ISincronizacionDatosCircunscripcionRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Datos<IEnumerable<SincronizacionDatosDTO>>> ObtenerDatosOrganismoFinanciador(SincronizacionDatosRequest request)
    {
        return await _repository.ObtenerDatosOrganismoFinanciador(request);
    }
}
