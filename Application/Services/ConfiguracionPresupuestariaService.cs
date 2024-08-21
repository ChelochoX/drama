using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using DocumentFormat.OpenXml.Office2016.Excel;
using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;
using Domain.Entities.Request;
using Domain.Entities.Request.ConfiguracionPresupuestaria;
using Domain.Entities.Response;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using DatosparaConfiguracionPresupuestariaDTO = Domain.Entities.ConfiguracionPresyupuestaria.DatosparaConfiguracionPresupuestariaDTO;

namespace Application.Services;

public class ConfiguracionPresupuestariaService : IConfiguracionPresupuestariaService
{
    private readonly IConfiguracionPresupuestariaRepository _repository;
    private readonly IMapper _mapper;
    private readonly IConsolidarAnteproyectoModificarObjetoGastoRepository _repositoryModifOBGConsolidado;
    private readonly IGenerarSolicitudporCircunscripcionRepository _repositorySolicitud;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ConfiguracionPresupuestariaService(IConfiguracionPresupuestariaRepository repository, IMapper mapper, IConsolidarAnteproyectoModificarObjetoGastoRepository repositoryModifOBGConsolidado, IGenerarSolicitudporCircunscripcionRepository repositorySolicitud, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _mapper = mapper;
        _repositoryModifOBGConsolidado = repositoryModifOBGConsolidado;
        _repositorySolicitud = repositorySolicitud;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task<Datos<IEnumerable<ConfiguracionPresupuestariaPorObjetosGastosDTO>>> ObtenerConfiguracionPresupuestariaObjetosGastos(ConfiguracionPresupuestariaPorObjetosGastosRequest request)
    {
        return await _repository.ObtenerObjetosdeGastosparalaConfiguracionPresupuestaria(request);
    }

    public async Task<Datos<DatosdeConfiguracionPresupuestariaDTO>> InsertarCabeceraparaConfiguracionPresupuestaria(DatosparaCabeceraConfiguracionPresupuestaria2Request request)
    {
        var datosparaCabConfig = new DatosparaCabeceraConfiguracionPresupuestariaRequest
        {
            UsuarioInserto = await _repositorySolicitud.ObtenerCodigoUsuario(request.CedulaUsuario),
            CodigoCircunscripcion = request.CodigoCircunscripcion,
            Ejercicio = request.Ejercicio,
            CodigoObjetoGasto = request.CodigoObjetoGasto,
            CodigoPrograma = request.CodigoPrograma,    
            CodigoActividad = request.CodigoActividad,  
            CodigoTipoPresupuesto = request.CodigoTipoPresupuesto,
            CodigoDepartamento = request.CodigoDepartamento,
            Grupo = request.Grupo,
            CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad,
            SubGrupo = request.SubGrupo,
            CodigoMateria = request.CodigoMateria
        };

        return await _repository.InsertarCabeceraparaConfiguracionPresupuestaria(datosparaCabConfig);
    }

    public async Task<Datos<int>> InsertarBienesporConfiguracionPresupuestariaNoConsolidado(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion)
    {
        return await _repository.InsertarBienesporConfiguracionPresupuestariaNoConsolidado(request, CodigoAnteproyectoObj, CodigoVersion);
    }

    public async Task<Datos<int>> InsertarBienesporConfiguracionPresupuestariaConsolidado(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion)
    {
        return await _repository.InsertarBienesporConfiguracionPresupuestariaConsolidado(request, CodigoAnteproyectoObj, CodigoVersion);
    }
    public async Task<DatosparaConfiguracionPresupuestariaDTO> ObtenerDatosparaInsertarConfiguracionPresupuestaria(DatosparaConfiguracionPresupuestariaRequest request)
    {
        return await _repository.ObtenerDatosparaInsertarConfiguracionPresupuestaria(request);
    }

    public async Task<Datos<IEnumerable<OrganismoFinanciadorDTO>>> ObtenerDatosOrganismoFinanciador(OrganismoFinanciadorRequest request)
    {
        return await _repository.ObtenerDatosOrganismoFinanciador(request);
    }

    public async Task<Datos<IEnumerable<FuenteFinanciamientoDTO>>> ObtenerDatosFuenteFinanciamiento(FuenteFinanciamientoRequest request)
    {
        return await _repository.ObtenerDatosFuenteFinanciamiento(request);
    }

    public async Task<int> EliminarObjetodeAnteproyectoObjetoyBienes(VerionesAnteproyectoObjetosEliminarRequest request)
    {
        return await _repository.EliminarObjetodeAnteproyectoObjetoyBienes(request);
    }

    public async Task<int> ModificarFuenteFinanciamientoyBienes(ModificarFuenteFinanciamientoyBienesRequest request)
    {
        var resultadoGral = 0;

        //Gestionamos las modificaciones para las fuentes de financiamiento
        var fuente = new ModificarFinanciamientoAnteproyectoObjetosRequest
        {
            CodigoOrganismoFinanciador = request.CodigoOrganismoFinanciador,
            CodigoFuenteFinanciamiento = request.CodigoFuenteFinanciamiento,
            Evento = request.Evento,
            CodigoAnteProyectoObjeto = request.CodigoAnteProyectoObjeto,
            CodigoVersion = request.CodigoVersion,
            ConfiguracionPresupuestaria = request.ConfiguracionPresupuestaria,
        };
        var resultado1 = await _repository.ModificarFinanciamientoAnteproyectoObjeto(fuente);

        //Gestionamos las actualizaciones para los bienes
        int registrosAfectados = 0;

        foreach (var bien in request.Bienes)
        {
            if (!bien.Seleccionado)
            {
                registrosAfectados += await _repository.EliminarBienesporFuentedeFinanciamiento(request.CodigoVersion, request.CodigoAnteProyectoObjeto, bien.CodigoVersionAnteproyectoBienes,
                    request.CodigoMateria, request.CodigoCentroResponsabilidad);

                //Actualizar el monto de la tabla Veersiones Bienes Consolidado
                var datosActualizacion = new ConfiguracionPresupuestariadeBienesRequest
                {
                    NumeroBien = bien.NumeroBien,
                    Cantidad = request.Bienes.Select(a => a.CantidadBien).FirstOrDefault()
                };

                var actualizarCantidadBienConsolidado = await _repository.ActualizarCantidadBiendelConsolidadoBienes(datosActualizacion, request.CodigoAnteProyectoObjeto, request.CodigoVersion);
            }
            else
            {
                var datos = new BienesparaConfiguracionPresupuestariaRequest
                {
                    CodigoVersion = request.CodigoVersion,
                    CodigoVersionOBG = request.CodigoAnteProyectoObjeto,
                    CodigoObjetoGasto = request.CodigoObjetoGasto,
                    Ejercicio = request.Ejercicio,
                    NumeroBien = bien.NumeroBien,
                    CodigoMateria = request.CodigoMateria,
                    CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad
                };

                var ListaBienes = await _repository.ObtenerBienesparaAgregarFinanciamiento(datos);

                foreach (var bien2 in ListaBienes)
                {
                    var bienRequest = request.Bienes.FirstOrDefault(x => x.NumeroBien == int.Parse(bien2.NumeroBien));

                    if (bienRequest != null)
                    {
                        var datosConfiguracionBien = new ConfiguracionPresupuestariadeBienesRequest
                        {
                            NumeroBien = int.Parse(bien2.NumeroBien),
                            DescripcionBien = bien2.Descripcion,
                            Cantidad = bien2.Cantidad,
                            ValorUnitario = bien2.ValorUnitario,
                            UsuarioInserto = await _repositorySolicitud.ObtenerCodigoUsuario(request.CedulaUsuario),
                            UnidadMedida = bien2.UnidadMedida,
                            Fundamentacion = bien2.Fundamentacion,
                            Seleccionado = true,
                            CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad,
                            CodigoMateria = request.CodigoMateria
                        };



                        //Version No Consolidada
                        var resultadoNoConsolidada = await _repository.InsertarBienesporConfiguracionPresupuestariaNoConsolidado(datosConfiguracionBien,
                            request.CodigoAnteProyectoObjeto, request.CodigoVersion);

                        //Version Consolidada
                        var resultadoConsolidada = await _repository.InsertarBienesporConfiguracionPresupuestariaConsolidado(datosConfiguracionBien,
                            request.CodigoAnteProyectoObjeto, request.CodigoVersion);


                        registrosAfectados += resultadoNoConsolidada.Items;
                    }
                }
            }
        }

        if (resultado1 > 0 || registrosAfectados > 0)
            resultadoGral = 1;
        else
            resultadoGral = 0;

        return resultadoGral;

    }

    public async Task<Datos<IEnumerable<BienesparaConfiguracionPresupuestariaDTO>>> ObtenerBienesparaFuenteFinanciamiento(BienesparaConfiguracionPresupuestariaRequest request)
    {
        return await _repository.ObtenerBienesparaFuenteFinanciamiento(request);
    }

    public async Task<int> GestionarFuentesdeFinanciamientoConsusBienes(DatosparaAnteproyectoObjetosyBienesRequest request)
    {
        int resultadoGral = 0;

        //Insertamos la tabla de Anteproyecto Objetos
        var configuracion = new DatosparaAnteproyectoObjetosRequest
        {
            CodigoVersionAnteproyecto = request.CodigoVersionAnteproyecto,
            CodigoConfiguracionPresupuestaria = request.CodigoConfiguracionPresupuestaria,
            CodigoFuenteFinanciamiento = request.CodigoFuenteFinanciamiento,
            CodigoOrganismoFinanciador = request.CodigoOrganismoFinanciador,
            Evento = request.Evento == 0 ? 1 : request.Evento,
            CodigoObjetoGasto = request.CodigoObjetoGasto,
            CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad,
            CodigoMateria = request.CodigoMateria

        };

        ///Obtenemos los montos de Planificacion Inicial y Modoficacion
        var datosParaMontos = new PresupuestoInicialyModificacionesConsolidadoRequest
        {
            CodigoObjetoGasto = request.CodigoObjetoGasto,
            CodigoFF = request.CodigoFuenteFinanciamiento,
            CodigoOF = request.CodigoOrganismoFinanciador,
            CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad,
            CodigoMateria = request.CodigoMateria,
            Ejercicio = request.Ejercicio
        };     

        var valorMontos_ = await _repositoryModifOBGConsolidado.ObtenerPresupuestoInicialyModificaciones(datosParaMontos);

        //pasamos los Montos necesarios para la planificacion y modificacion
        var ValoresMontos = new PresupuestoInicialyModificacionesConsolidadoDTO
        {
            PresupuestoInicial = valorMontos_.PresupuestoInicial,
            Modificaciones = valorMontos_.Modificaciones
        };


         var codigoAnteproyectoObjeto = await _repository.InsertarAnteproyectoObjetos(configuracion,ValoresMontos);
            

        //Obtenemos los bienes relacionados a ese Objeto de Gastos
        var datos = new BienesparaConfiguracionPresupuestariaRequest
        {
            CodigoVersion = request.CodigoVersionAnteproyecto,
            CodigoVersionOBG = codigoAnteproyectoObjeto.Items,
            CodigoObjetoGasto = request.CodigoObjetoGasto,
            Ejercicio = request.Ejercicio,
            CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad,
            CodigoMateria = request.CodigoMateria            
        };

        var ListaBienes = await _repository.ObtenerBienesparaAgregarFinanciamiento(datos);

        //Tratamiento de los bienes por cada fuente de financiamiento
        var resultadoBienesNoConsolidado = new Datos<int> { };
        var resultadoBienesConsolidado = new Datos<int> { };
        foreach (var bien in ListaBienes)
        {
            // Encontrar el bien en la lista de bienes de la base de datos
            var bienRequest = request.Bienes.FirstOrDefault(x => x.NumeroBien == bien.NumeroBien);

            if (bienRequest != null)
            {
                // Si se encuentra el bien, cargar los datos para el insertado
                var datosConfiguracion = new ConfiguracionPresupuestariadeBienesRequest
                {
                    NumeroBien = int.Parse(bien.NumeroBien),
                    DescripcionBien = bien.Descripcion,
                    Cantidad = bien.Cantidad,
                    ValorUnitario = bien.ValorUnitario,
                    UsuarioInserto = await _repositorySolicitud.ObtenerCodigoUsuario(request.CedulaUsuario),
                    UnidadMedida = bien.UnidadMedida,
                    Fundamentacion = bien.Fundamentacion,
                    Seleccionado = true,
                    CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad,
                    CodigoMateria = request.CodigoMateria
                };

                //Primeramente Insertamos en la Version no Consolidada
                resultadoBienesNoConsolidado = await _repository.InsertarBienesporConfiguracionPresupuestariaNoConsolidado(datosConfiguracion,
                   codigoAnteproyectoObjeto.Items, request.CodigoVersionAnteproyecto);
                //Segundo Insertamos en la version consolidada
                resultadoBienesConsolidado = await _repository.InsertarBienesporConfiguracionPresupuestariaConsolidado(datosConfiguracion,
                    codigoAnteproyectoObjeto.Items, request.CodigoVersionAnteproyecto);
            }
        }

        // Actualizamos los Valores de Cantidad y Monto de la Tabla Configuracion en base a la Fuente de Financiamiento realizada
        var resultado2 = await _repository.ModificarMontoPlanificadoAnteproyectoObjeto(request.CodigoVersionAnteproyecto, request.CodigoConfiguracionPresupuestaria, codigoAnteproyectoObjeto.Items
            ,request.CodigoMateria,request.CodigoCentroResponsabilidad);

        if (codigoAnteproyectoObjeto.Items > 0 || resultadoBienesNoConsolidado.Items > 0 || resultado2 > 0)
            resultadoGral = 1;
        else
            resultadoGral = 0;

        return resultadoGral;
    }

    public async Task<IEnumerable<FinanciamientoVersionAnteproyectoObjetosResponseDTO>> ObtenerDatosVersionesAnteproyectosObjetos(FinanciamientoVersionAnteproyectoObjetosRequest request)
    {
        return await _repository.ObtenerDatosVersionesAnteproyectosObjetos(request);
    }

    public async Task<IEnumerable<DatosdeSolicitudporObjetoGastoResponseDTO>> ObtenerDatosdelaSolicitud(DatosdeSolicitudporObjetoGastoRequest request)
    {
        return await _repository.ObtenerDatosdelaSolicitud(request);
    }

    public async Task<int> VerificarExisteVersiondeAnteproyectoCerrado(int ejercicio)
    {
        string stringCir = _httpContextAccessor.HttpContext.Request.Headers["codCircunscripcion"].ToString().Trim();
        int codCircunscripcion = 1;
        if (!string.IsNullOrEmpty(stringCir))
        {
            codCircunscripcion = int.Parse(stringCir);
        }

        return await _repository.VerificarExisteVersiondeAnteproyectoCerrado(ejercicio, codCircunscripcion);
    }

}
