using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Entities.ConsolidadoAnteproyectoPresupuesto;
using Domain.Entities.Request;

namespace Application.Services;

public class ConsolidadoAnteproyectoPresupuestoService : IConsolidadoAnteproyectoPresupuestoService
{

    private readonly IConsolidadoAnteproyectoPresupuestoRepository _repository;
    private readonly IGenerarSolicitudporCircunscripcionRepository _repositorySolicitud;
    private readonly IMapper _mapper;
    private readonly IConfiguracionPresupuestariaRepository _repositoryVersiones;

    public ConsolidadoAnteproyectoPresupuestoService(IMapper mapper, IConsolidadoAnteproyectoPresupuestoRepository repository, IGenerarSolicitudporCircunscripcionRepository repositorySolicitud, IConfiguracionPresupuestariaRepository repositoryVersiones)
    {
        _mapper = mapper;
        _repository = repository;
        _repositorySolicitud = repositorySolicitud;
        _repositoryVersiones = repositoryVersiones;
    }

    public async Task<IEnumerable<ObjetoGastosConsolidadoAnteproyectoDTO>> ListarConsolidadoAnteproyectoporObjetoGasto(ObjetoGastosConsolidadoAnteproyectoRequest request)
    {
        return await _repository.ListarConsolidadoAnteproyectoporObjetoGasto(request);
    }
    public async Task<Datos<IEnumerable<BienesdelConsolidadoAnteproyectoDTO>>> ListarBienesdelConsolidadoAnteproyectoPresupuestario(BienesdelConsolidadoAnteproyectoRequest request)
    {
        return await _repository.ListarBienesdelConsolidadoAnteproyectoPresupuestario(request);
    }
    public async Task<int> ModificarBiendeVersionAnteproyecto(ModificarBienesVersionAnteproyectoRequest request)
    {
        var user = await _repositorySolicitud.ObtenerCodigoUsuario(request.CedulaUsuario);

        // Primeramente actualizamos la tabla de Bienes
        var actualizarBien = await _repository.ModificarBiendeVersionAnteproyecto(request, user);

        //Actualizamos los Valores de Cantidad y Monto de la Tabla Configuracion en base a la Fuente de Financiamiento realizada
        var actualizarMontoPlanificado = await _repositoryVersiones.ModificarMontoPlanificadoAnteproyectoObjetoModif(request.CodigoVersion, request.CodigoConfiguracionPresupuestaria, request.CodigoAnteproyectoObjeto, 0, 0);

        return actualizarBien;
    }
    public async Task<int> EliminarBiendeVersionAnteproyecto(EliminarBienVersionAnteproyectoRequest request)
    {
        return await _repository.EliminarBiendeVersionAnteproyecto(request);
    }
    public async Task<IEnumerable<ObjetodeGastoVersionAnteproyectoDTO>> ListarObjetosdeGastosdeVersionesAnteproyectos(ObjetodeGastoVersionAnteproyectoRequest request)
    {
        return await _repository.ListarObjetosdeGastosdeVersionesAnteproyectos(request);   
    }
    public async Task<int> GestionarEliminarObjetoGastoVersionesAnteproyecto(List<ObjetodeGastoVersionAnteproyectoDTO> items)
    {
        var resultadoGral = 0;

        foreach (var item in items)
        {
            var datos = new ObjetodeGastoVersionAnteproyectoDTO
            {
                CodigoAnteproyectoObjeto = item.CodigoAnteproyectoObjeto,
                CodigoVersion = item.CodigoVersion
            };

            //Eliminacion en este orden
            var eliminarBien = await _repository.EliminarBiendeVersionAnteproyectoporOBGVersiones(datos);

            var eliminarContratos = await _repository.EliminardeVersionesContratoporOBGVersiones(datos);

            var eliminarAnteproyectoPlanificacion = await _repository.EliminardeVersionesPlanificacionporOBGVersiones(datos);

            var eliminarObjetosGastos = await _repository.EliminarObjetodeVersionesAnteproyectoporOBGVersiones(datos);

            resultadoGral++;
        }

        return resultadoGral;
    }
}
