using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using Domain.Entities.PlanificacionFinanciera;
using Domain.Entities.Request;

namespace Application.Services;

public class PlanificacionFinancieraService: IPlanificacionFinancieraService
{
    private readonly IPlanificacionFinancieraRepository _repository;
    private readonly IMapper _mapper;
    private readonly IGenerarSolicitudporCircunscripcionRepository _repositorySolicitud;
    public PlanificacionFinancieraService(IMapper mapper, IPlanificacionFinancieraRepository repository, IGenerarSolicitudporCircunscripcionRepository repositorySolicitud)
    {
        _mapper = mapper;
        _repository = repository;
        _repositorySolicitud = repositorySolicitud;
    }

    public async Task<Datos<IEnumerable<VersionesAnteproyectoDTO>>> ListadoVersionesanteproyectos(VersionesAnteproyectoRequest request)
    {
        return await _repository.ListadoVersionesanteproyectos(request);
    }
    public async Task<int> ObtenerDiasRestantesCierre()
    {
        return await _repository.ObtenerDiasRestantesCierre();
    }
    public async Task<Datos<IEnumerable<PlanificacionFinancieraDTO>>> ListadoPlanificacionFinanciera(PlanificacionFinancieraRequest request)
    {
        return await _repository.ListadoPlanificacionFinanciera(request);
    }
    public async Task<IEnumerable<PlanificacionFinancieraporObjetoGastoDTO>> ListadoPlanificacionFinancieraporObjetoGasto(PlanificacionFinancieraporObjetoGastoRequest request)
    {
        return await _repository.ListadoPlanificacionFinancieraporObjetoGasto(request);
    }
    public async Task<IEnumerable<PlanificacionMensualDTO>> ListadoPlanificacionMensual(PlanificacionMensualRequest request)
    {
        return await _repository.ListadoPlanificacionMensual(request);
    }
    public async Task<CodigoAnteproyectoOBJparaPlanificacionFinancieraDTO> ObtenerCodigoAnteproyectoObjeto(CodigoAnteproyectoOBJparaPlanificacionFinancieraRequest request)
    {
        return await _repository.ObtenerCodigoAnteproyectoObjeto(request);
    }
    public async Task<IEnumerable<MesesparaPlanificacionFinancieraDTO>> ObtenerMesesParalaPlanificacionFinanciera(MesesparaPlanificacionFinancieraRequest request)
    {
        return await _repository.ObtenerMesesParalaPlanificacionFinanciera(request);
    }
    public async Task<int> GestionarInsertarPlanificacionFinanciera(GestionarInsertarPlanificacionFinancieraRequest request)
    {
        var retorno = 0;
        //Primeramente realizamos las validacion del monto
        var validar = new MesesparaPlanificacionFinancieraRequest
        {
            CodigoAnteproyectoObjeto = request.CodigoAnteproyectoObjeto,
            CodigoVersion = request.CodigoVersion
        };

        var monto = await _repository.ValidarMontodePlanificacionFinanciera(validar);
        var montoCalculado = monto + request.Monto;

        if (montoCalculado > request.MontoPlanificacion)
        {
             return -1; //referecnia a exeso de monto
        }           

        //Primeramente realizamos las validacion de cantidad
        var cantidadEvento = await _repository.ValidarCantidadMesesporEvento(validar);     
              
        if (cantidadEvento >= request.CantidadEvento)
            return -2; //referencia a exeso de cantidad
       
        //Si esta todo Ok, insertamos
        if (retorno != -1 || retorno != -2)
        {
            var insertar = new InsertarPlanificacionFinancieraRequest
            {
                CodigoAnteproyectoObjeto = request.CodigoAnteproyectoObjeto,
                CodigoVersion = request.CodigoVersion,
                Mes = request.Mes,
                Monto = request.Monto,
                UsuarioInserto = await _repositorySolicitud.ObtenerCodigoUsuario(request.UsuarioInserto)
            };
            var respuesta = await _repository.InsertarPlanificacionFinanciera(insertar);

            retorno = respuesta;           
        }
        return retorno;
    }
    public async Task<int> GestionarEditarPlanificacionFinanciera(GestionarEditartarPlanificacionFinancieraRequest request)
    {
        var retorno = 0;
        //Primeramente realizamos las validacion del monto
        var validar = new MesesparaPlanificacionFinancieraRequest
        {
            CodigoAnteproyectoObjeto = request.CodigoAnteproyectoObjeto,  
            CodigoVersion = request.CodigoVersion
        };

        var monto = await _repository.ValidarMontodePlanificacionFinanciera(validar);
        var montoCalculado = (monto + request.Monto) - request.MontoAnterior;

        if (montoCalculado > request.MontoPlanificacion)
        {
            return -1; //referecnia a exeso de monto
        }       

        //Si esta todo Ok, insertamos
        if (retorno != -1 || retorno != -2)
        {
            var editar = new InsertarPlanificacionFinancieraRequest
            {
                CodigoAnteproyectoPlanificacion = request.CodigoAnteproyectoPlanificacion,
                CodigoAnteproyectoObjeto = request.CodigoAnteproyectoObjeto,
                CodigoVersion = request.CodigoVersion,
                Mes = request.Mes,
                Monto = request.Monto,
                UsuarioInserto = await _repositorySolicitud.ObtenerCodigoUsuario(request.CedulaUsuario)
            };
            var respuesta = await _repository.EditarPlanificacionFinanciera(editar);

            retorno = respuesta;
        }
        return retorno;
    } 
    public async Task<IEnumerable<PlanificacionMensualDTO>> ListadoPlanificacionMensualporItem(PlanificacionMensualporItemRequest request)
    {
        return await _repository.ListadoPlanificacionMensualporItem(request);
    }
    public async Task<int> EliminarPlanificacionFinanciera(EliminarPlanificacionFinancieraRequest request)
    {
        return await _repository.EliminarPlanificacionFinanciera(request);
    }
}
