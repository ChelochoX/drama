using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;
using Domain.Entities.Request;

namespace Application.Services;

public class ConsolidarAnteproyectoModificarObjetoGastoService : IConsolidarAnteproyectoModificarObjetoGastoService
{
    private readonly IConsolidarAnteproyectoModificarObjetoGastoRepository _repository;
    private readonly IMapper _mapper;
    private readonly IConfiguracionPresupuestariaRepository _repositoryVersiones;
    private readonly IGenerarSolicitudporCircunscripcionRepository _repositorySolicitud;

    public ConsolidarAnteproyectoModificarObjetoGastoService(IMapper mapper, IConsolidarAnteproyectoModificarObjetoGastoRepository repository, 
        IConfiguracionPresupuestariaRepository repositoryVersiones, IGenerarSolicitudporCircunscripcionRepository repositorySolicitud)
    {
        _mapper = mapper;
        _repository = repository;
        _repositoryVersiones = repositoryVersiones;
        _repositorySolicitud = repositorySolicitud;
    }

    public async Task<IEnumerable<CentroResponsabilidadyMateriaDTO>> ObtenerCentroResponsabilidadyMateriaporUsuario(string cedulaUsuario)
    {
        return await _repository.ObtenerCentroResponsabilidadyMateriaporUsuario(cedulaUsuario);
    }
    public async Task<IEnumerable<CircunscripcionesparaConsolidadoAnteproyectoDTO>> ObtenerCircunscripciones()
    {
        return await _repository.ObtenerCircunscripciones();
    }
    public async Task<IEnumerable<ObjetosGastosConsolidacionAnteproyectoDTO>> ObtenerObjetosGastos(int ejercicio)
    {
        return await _repository.ObtenerObjetosGastos(ejercicio);
    }
    public async Task<IEnumerable<DatosparaConfiguracionPresupuestariaConsolidadoDTO>> ObtenerDatosparalaConfiguracionPresupuestaria(DatosparaConfiguracionPresupuestariaConsolidadoRequest request)
    {
        return  await _repository.ObtenerDatosparalaConfiguracionPresupuestaria(request);
    }
    public async Task<IEnumerable<BienesdeAnteproyectoBienesConsolidadoDTO>> ObtenerBienesdeAnteproyectoObjeto(BienesdeAnteproyectoBienesConsolidadoRequest request)
    {
        return await _repository.ObtenerBienesdeAnteproyectoObjeto(request);
    }
    public async Task<Datos<IEnumerable<BienesparaConsolidadoDTO>>> ObtenerBienesparaConsolidado(BienesparaConsolidadoRequest request)
    {
        return await _repository.ObtenerBienesparaConsolidado(request);
    }
    public async Task<int> InsertarConfiguracionPresupuestariaDesdeConsolidado(ConfiguracionPresupuestariaConsolidadoRequest request)
    {
        return await _repository.InsertarConfiguracionPresupuestariaDesdeConsolidado(request);
    }
    public async Task<int> GestionarInsertarVersionadoObjetoGastoConsolidado(DatosparaVersionesAnteproyectoObjetoConsolidadoRequest request)
    {
        int resultadoFinal = 0;        
    
        //pasamos los Montos necesarios para la planificacion y modificacion
        var ValoresMontos = new PresupuestoInicialyModificacionesConsolidadoDTO
        {
            PresupuestoInicial = request.PresupuestoInicial,
            Modificaciones = request.Modificaciones
        };

        //Insertamos la tabla de Anteproyecto Objetos
        var objeto = new DatosparaAnteproyectoObjetosRequest
        {
            CodigoVersionAnteproyecto = request.CodigoVersionAnteproyecto,
            CodigoConfiguracionPresupuestaria = request.CodigoConfiguracionPresupuestaria,
            CodigoFuenteFinanciamiento = request.CodigoFuenteFinanciamiento,
            CodigoOrganismoFinanciador = request.CodigoOrganismoFinanciador,
            Evento = request.Evento,
            CodigoObjetoGasto = request.CodigoObjetoGasto,
            CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad,
            CodigoMateria = request.CodigoMateria,
            CodigoCircunscripcionOrigen = request.CodigoCircunscripcion          
        };     

        var codigoAnteproyectoObjeto = await _repositoryVersiones.InsertarAnteproyectoObjetos(objeto, ValoresMontos);

        resultadoFinal = codigoAnteproyectoObjeto.Items;   

        return resultadoFinal;
    }
    public async Task<int> GestionarInsertarVersionadoBienesConsolidado(DatosparaVersionesAnteproyectoBienesConsolidadoRequest request)
    {
        var resultadoFinal = 0;
        var actualizacion = 0;

        var cedulaUsuario = request.UsuarioInserto;

        var codigoUsuario = await _repositorySolicitud.ObtenerCodigoUsuario(cedulaUsuario);

        var bienes = new ConfiguracionPresupuestariadeBienesRequest
        {
            NumeroBien = int.Parse(request.NumeroBien),
            DescripcionBien = request.DescripcionBien,
            Cantidad = request.Cantidad,
            ValorUnitario = request.ValorUnitario,
            UnidadMedida = request.UnidadMedida,
            UsuarioInserto  = codigoUsuario,
            Fundamentacion = request.Fundamentacion,
            Seleccionado = true,
            CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad,
            CodigoMateria = request.CodigoMateria
        };

        //Primeramente Insertamos en la Version no Consolidada
        var resultadoBienesNoConsolidado = await _repositoryVersiones.InsertarBienesporConfiguracionPresupuestariaNoConsolidado(bienes,
                                           request.CodigoAnteproyectoObjetoGasto, request.CodigoAnteproyecto);
        //Segundo Insertamos en la version consolidada
        var resultadoBienesConsolidado = await _repositoryVersiones.InsertarBienesporConfiguracionPresupuestariaConsolidado(bienes,
                                     request.CodigoAnteproyectoObjetoGasto, request.CodigoAnteproyecto);

        //Actualizamos los Valores de Cantidad y Monto de la Tabla Configuracion en base a la Fuente de Financiamiento realizada
        if (resultadoBienesConsolidado.Items != -1 && resultadoBienesConsolidado.Items > 0)
        {
            actualizacion = await _repositoryVersiones.ModificarMontoPlanificadoAnteproyectoObjetoModif(request.CodigoAnteproyecto, request.CodigoConfiguracionPresupuestaria, 
                request.CodigoAnteproyectoObjetoGasto,request.CodigoMateria,request.CodigoCentroResponsabilidad);
        }

        if (resultadoBienesConsolidado.Items > 0 || resultadoBienesNoConsolidado.Items > 0 || actualizacion > 0 )
            resultadoFinal = 1;
        else if (resultadoBienesConsolidado.Items == -1)
            resultadoFinal = -1;
        else
            resultadoFinal = 0; 

        return resultadoFinal;
    }
    public async Task<int> ModificarAnteproyectoObjetoConsolidado(ModificarOBGConsolidadoRequest request)
    {
        return await _repository.ModificarAnteproyectoObjetoConsolidado(request);
    }
    public async Task<int> ModificarAnteproyectoBienesConsolidado(ModificarBienesConsolidadoRequest request)
    {
        var resultadoFinal = 0;
        var actualizacion = 0;

        var codigoUsuario = await _repositorySolicitud.ObtenerCodigoUsuario(request.CedulaUsuario);

        var datosModificacion = new ModificarBienesConsolidadoRequest
        {
            NumeroBien = request.NumeroBien,
            DescripcionBien = request.DescripcionBien,
            Cantidad = request.Cantidad,
            ValorUnitario = request.ValorUnitario,
            CodigoUsuarioModificacion = codigoUsuario,
            Fundamentacion = request.Fundamentacion,
            Seleccionado = request.Seleccionado,
            CodigoAnteproyectoBien = request.CodigoAnteproyectoBien,
            CodigoVersion = request.CodigoVersion,
            CodigoAnteProyectoObjeto = request.CodigoAnteProyectoObjeto
        };

        var resultado = await _repository.ModificarAnteproyectoBienesConsolidado(datosModificacion);

        //Actualizamos los Valores de Cantidad y Monto de la Tabla Configuracion en base a la Fuente de Financiamiento realizada
        actualizacion = await _repositoryVersiones.ModificarMontoPlanificadoAnteproyectoObjetoModif(request.CodigoVersion, request.CodigoConfiguracionPresupuestaria, request.CodigoAnteProyectoObjeto,
            request.CodigoMateria,request.CodigoCentroResponsabilidad);

        if (resultado > 0 || actualizacion > 0)
            resultadoFinal = 1;       
        else
            resultadoFinal = 0;

        return resultadoFinal;

    }
    public async Task<PresupuestoInicialyModificacionesConsolidadoDTO> ObtenerPresupuestoInicialyModificaciones(PresupuestoInicialyModificacionesConsolidadoRequest request)
    {
        return await _repository.ObtenerPresupuestoInicialyModificaciones(request);
    }
    public async Task<int> EliminarBiendesdeConsolidadoModif(int CodigoVersionAnteproyecto, int CodigoVersionAnteproyectoObjeto, int CodigoVersionAnteproyectoBien, int CodigoConfiguracionPresupuestaria,
        int codigoMateria,int codigoCentroResponsabilidad)
    {
        var resultadoFinal = 0;
        var actualizacion = 0;

        var resultado = await _repositoryVersiones.EliminarBienesporFuentedeFinanciamientoModif(CodigoVersionAnteproyecto, CodigoVersionAnteproyectoObjeto, CodigoVersionAnteproyectoBien,
            codigoMateria, codigoCentroResponsabilidad);

        //Actualizamos los Valores de Cantidad y Monto de la Tabla Configuracion en base a la Fuente de Financiamiento realizada
        actualizacion = await _repositoryVersiones.ModificarMontoPlanificadoAnteproyectoObjetoModif(CodigoVersionAnteproyecto, CodigoConfiguracionPresupuestaria, CodigoVersionAnteproyectoObjeto,
            codigoMateria,codigoCentroResponsabilidad);

        if (resultado > 0 || actualizacion > 0)
            resultadoFinal = 1;
        else
            resultadoFinal = 0;

        return resultadoFinal;

    }
    public async Task<int> ValidarExistenciaenAnteproyectoObjetoConsolidado(ValidarExistenciaenAnteproyectoObjetoConsolidadoRequest request)
    {
        return await _repository.ValidarExistenciaenAnteproyectoObjetoConsolidado(request);
    }
    public async Task<int> ObtenerEjercicioActivo()
    {
        return await _repository.ObtenerEjercicioActivo();
    }
    public async Task<int> ObtenerMontoPlanificado(MontoPlanificadoRequest request)
    {
        return await _repository.ObtenerMontoPlanificado(request);
    }
    public async Task<Datos<IEnumerable<ObjetosGastosPendientesaConfigurarDTO>>> ValidarOBGPerteneceaConfiguracion(int codigoVersion)
    {
        return await _repository.ValidarOBGPerteneceaConfiguracion(codigoVersion);
    }

}
