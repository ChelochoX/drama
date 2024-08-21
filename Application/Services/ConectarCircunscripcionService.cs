using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using Domain.Entities.ConectarCircunscripcion;

namespace Application.Services;

public class ConectarCircunscripcionService : IConectarCircunscripcionService
{
    private readonly IConectarCircunscripcionRepository _repository;
    private readonly IMapper _mapper;
    public ConectarCircunscripcionService(IMapper mapper, IConectarCircunscripcionRepository repository)
    {
        _mapper = mapper;
        _repository = repository;        
    }

    public async Task<string> ObtenerDatosparaConectarCircunscripcion(int codigoCircunscripcion)
    {    
        var datosCircunscripcion = await _repository.ObtenerDatosparaConectarCircunscripcion(codigoCircunscripcion);
        
        return datosCircunscripcion.BasedeDatos;
    }
}
