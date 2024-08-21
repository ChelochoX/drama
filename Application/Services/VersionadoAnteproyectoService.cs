using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using Domain.Entities.Request;
using Domain.Entities.VersionadoAnteproyecto;

namespace Application.Services;

public class VersionadoAnteproyectoService : IVersionadoAnteproyectoService
{
    private readonly IVersionadoAnteproyectoRepository _repository;
    private readonly IMapper _mapper;

    public VersionadoAnteproyectoService(IMapper mapper, IVersionadoAnteproyectoRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Datos<IEnumerable<VersionadoAnteproyectoDTO>>> ListarVersionadodeAnteproyecto(VersionadoAnteproyectoRequest request)
    {
        return await _repository.ListarVersionadodeAnteproyecto(request);
    }

    public async Task<IEnumerable<VersionesAnteproyectoDTO>> ObtenerVersionesporEjercicio(int ejercicio)
    {
        return await _repository.ObtenerVersionesporEjercicio(ejercicio);   
    }
}
