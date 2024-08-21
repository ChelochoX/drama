using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using ClosedXML.Excel;
using Domain.Entities.Anteproyecto;
using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.GenerarSolicitudporCircunscripcion;
using Domain.Entities.Request;
using Domain.Entities.VerificacionSolicitudes;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Application.Services;

public class GenerarSolicitudesporCircunscripcionService : IGenerarSolicitudesporCircunscripcionService
{

    private readonly IGenerarSolicitudporCircunscripcionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IConsolidarAnteproyectoModificarObjetoGastoRepository _repositoryModificacionOBG;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GenerarSolicitudesporCircunscripcionService(IGenerarSolicitudporCircunscripcionRepository repository, IMapper mapper,
        IConsolidarAnteproyectoModificarObjetoGastoRepository repositoryModificacionOBG, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _mapper = mapper;
        _repositoryModificacionOBG = repositoryModificacionOBG;
        _httpContextAccessor = httpContextAccessor;       
    }


    public async Task<Datos<IEnumerable<SolicitudCircunscripcionesEstadoAbierto>>> ListarSolicitudesconEstadosAbiertos(SolicitudBienesCircunscripcionRequest request)
    {
        return await _repository.ListarSolicitudesconEstadosAbiertos(request);
    }
    public async Task<CentroResponsabilidadyMateria> ObtenerCentrodeResponsabilidadyMateriaJuridica(string usuarioLogeado)
    {
        return await _repository.ObtenerCentrodeResponsabilidadyMateriaJuridica(usuarioLogeado);
    }
    public async Task<UsuarioCircunscripcion> ObtenerCentroResponsabilidadporUsuario(UsuarioCircunscipcionRequest2 request)
    { 
        var resultado = await _repository.ObtenerCentroResponsabilidadporUsuario(request);
        return resultado;
    }
    public async Task<UsuarioCircunscripcion> ObtenerMateriaporUsuarioyCentroResponsabilidad(UsuarioCircunscipcionRequest2 request)
    {
        return await _repository.ObtenerMateriaporUsuarioyCentroResponsabilidad(request);
    }
    public async Task<Datos<IEnumerable<ObjetodeGastosDTO>>> ObtenerObjetosGasto(ObjetosdeGastosRequest request)
    {
        return await _repository.ObtenerObjetosGasto(request);
    }
    public async Task<Datos<IEnumerable<DetalleObjetoGastoporNroSolicitudDTO>>> ListarObjetodeGastosporCodigoSolicitud(ListaObjetoGastosdeSolicitudRequest request)
    {
        return await _repository.ListarObjetodeGastosporCodigoSolicitud(request);
    }
    public async Task<Datos<IEnumerable<SolicitudesCabeceraInsertResponse>>> InsertarCabeceraSolicitudCircunscripcion(SolicitudCabeceraporCircunscripcionRequest request)
    {
        SolicitudBienesCircunscripcion solicitud = new SolicitudBienesCircunscripcion();
        solicitud.CodigoMateria = request.CodigoMateria;
        solicitud.CodigoCentroResponsabilidad = request.CodigoCentroResponsabilidad;
        solicitud.CodigoCircunscripcion = request.CodigoCircunscripcion;
        solicitud.UsuarioInserto = await _repository.ObtenerCodigoUsuario(request.CedulaUsuario);
        solicitud.Poi = request.Poi;

        return await _repository.InsertarSolicitudBienesCircunscripcion(solicitud);
    }
    public async Task<Datos<int>> InsertarObjetosdeGastosPorNrodeSolicitud(ObjetosdeGastospornroSolicitudRequest request)
    {

        SolicitudObjetoDetalle solicitudDetalle = new SolicitudObjetoDetalle();
        solicitudDetalle.CodigoObjetoGasto = request.CodigoObjetoGasto;
        solicitudDetalle.UsuarioInserto = await _repository.ObtenerCodigoUsuario(request.CedulaUsuario);
        solicitudDetalle.CodigoSolicitud = request.CodigoSolicitud;

        return await _repository.InsertarSolicitudObjetosDetalle(solicitudDetalle);
    }
    public async Task<int> ModificarSolicitudBienCircunscripcion(ModificarSolicitudRequest request)
    {
        var parametros = _mapper.Map<ModificarSolicitudV2Request>(request);
        parametros.UsuarioModificacion = await _repository.ObtenerCodigoUsuario(request.CedulaUsuario);

        return await _repository.ModificarSolicitudBienesCircunscripcion(parametros);
    }
    public async Task<int> ActualizarEstadoSolicitudesysusObjetosGastos(ActualizarEstadoSolicitudesRequest request)
    {
        var anulacion = new ActualizarEstadoSolicitudes2Request
        {
            CodigosSolicitud = request.CodigosSolicitud,
            UsuarioAnulacion = await _repository.ObtenerCodigoUsuario(request.CedulaUsuario)
    };

        return await _repository.ActualizarEstadoSolicitudesysusObjetosGastos(anulacion);
    }
    public async Task<int> ActualizarEstadoObjetodeGasto(int codigoSolicitud, int codigoObjetoGasto, int estado)
    {
        return await _repository.ActualizarEstadoObjetodeGasto(codigoSolicitud, codigoObjetoGasto, estado);
    }
    public async Task<Datos<int>> EliminarObjetodeGasto(int codigoSolicitudObjeto, int codigoSolicitud)
    {
        return await _repository.EliminarObjetodeGasto(codigoSolicitudObjeto, codigoSolicitud);
    }
    public async Task<Datos<ObjetosdeGastosyBienesporNroSolicitud>> VisualizarObjetosdeGastosyBienesporNroSolicitud(int codigoSolicitud, int pagina, int cantidadRegistros, int codigoSolicitudObjeto, int? estado)
    {
        return await _repository.VisualizarObjetosdeGastosyBienesporNroSolicitud(codigoSolicitud, pagina, cantidadRegistros, codigoSolicitudObjeto, estado);
    }
    public async Task<(List<RegistrodesdeExcel> registros, List<string> errores, List<string> erroresValidacion)> ImportarDocumento(IFormFile archivo, string ciUsuarioSesion)
    {
        var registros = new List<RegistrodesdeExcel>();
        var errores = new List<string>();  // Listado de Errores del tipo de datos
        var erroresValidacion = new List<string>(); //Listado de Errores del tipo consistencia en la bbdd
        var erroresPermiso = new List<string>(); //Listado de Errores del tipo permiso
        string stringCodCircunscripcion = _httpContextAccessor.HttpContext.Request.Headers["codCircunscripcion"].ToString().Trim(); // Obtiene el codigo de circunscripcion del header
        int codCircunscripcion = 0;
        if (!string.IsNullOrEmpty(stringCodCircunscripcion))
        {
            codCircunscripcion = int.Parse(stringCodCircunscripcion);
        }
        else
        {
            codCircunscripcion = 1;
        }

        try
        {
            using (var workbook = new XLWorkbook(archivo.OpenReadStream()))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed();

                if (rows.Count() <= 1)
                {
                    errores.Add("El archivo está vacío y no puede ser procesado.");
                    return (registros, errores,erroresValidacion);
                }

                var headerRow = rows.First();
                var headers = new List<string>();
                foreach (var cell in headerRow.Cells())
                {
                    headers.Add(cell.GetString());
                }

                var registrosMapeados = new List<(RegistrodesdeExcel registro, int fila, IXLRow row)>();

                foreach (var row in rows.Skip(1))
                {
                    var erroresFila = new List<string>();

                    try
                    {
                        var poi = row.Cell(1).GetString();
                        var codigoCircunscripcion = row.Cell(2).GetString();
                        var codigoCentroResponsabilidad = row.Cell(3).GetString();
                        var codigoMateriaJuridica = row.Cell(4).GetString();
                        var numeroObjetoGasto = row.Cell(5).GetString();
                        var codigoCatalogoBienServicio = row.Cell(6).GetString();
                        var cantidadStr = row.Cell(7).GetString();
                        var valorUnitarioStr = row.Cell(8).GetString();
                        var fundamentacion = row.Cell(9).GetString();

                        double cantidad = 0;
                        double valorUnitario = 0;

                        // Validar que ningún campo esté vacío                       
                        if (string.IsNullOrWhiteSpace(poi)) erroresFila.Add(headers[0]);
                        if (string.IsNullOrWhiteSpace(codigoCircunscripcion)) erroresFila.Add(headers[1]);
                        if (string.IsNullOrWhiteSpace(codigoCentroResponsabilidad)) erroresFila.Add(headers[2]);
                        if (string.IsNullOrWhiteSpace(codigoMateriaJuridica)) erroresFila.Add(headers[3]);
                        if (string.IsNullOrWhiteSpace(numeroObjetoGasto)) erroresFila.Add(headers[4]);
                        if (string.IsNullOrWhiteSpace(codigoCatalogoBienServicio)) erroresFila.Add(headers[5]);
                        if (string.IsNullOrWhiteSpace(cantidadStr) || !double.TryParse(cantidadStr, out cantidad) || cantidad <= 0)
                            erroresFila.Add(headers[6] + " no es válido.");
                        if (string.IsNullOrWhiteSpace(valorUnitarioStr) || !double.TryParse(valorUnitarioStr, out valorUnitario) || valorUnitario <= 0)
                            erroresFila.Add(headers[7] + " no es válido.");
                        if (string.IsNullOrWhiteSpace(fundamentacion)) erroresFila.Add(headers[8]);

                        if (!Regex.IsMatch(poi, @"^[0-9]+$")) { }
                        if (!Regex.IsMatch(codigoCircunscripcion, @"^[0-9]+$")) { }
                        if (!Regex.IsMatch(codigoCentroResponsabilidad, @"^[0-9]+$")) { }
                        if (!Regex.IsMatch(codigoMateriaJuridica, @"^[0-9]+$")) { }
                        if (!Regex.IsMatch(numeroObjetoGasto, @"^[0-9]+$")) { }
                        if (!Regex.IsMatch(codigoCatalogoBienServicio, @"^[0-9]+$")) { }
                        if (!Regex.IsMatch(cantidadStr, @"^[0-9]+$")) { }
                        if (!Regex.IsMatch(valorUnitarioStr, @"^[0-9]+$")) { }
                        if (!Regex.IsMatch(fundamentacion, @"^[a-zA-Z0-9\s\.,]+$")) { }


                        if (erroresFila.Count > 0)
                        {
                            errores.Add($"Error en la fila {row.RowNumber()}: uno o más campos obligatorios están vacíos o contiene caracteres no permitidos ({string.Join("-", erroresFila)}).");
                            continue;
                        }

                        var registro = new RegistrodesdeExcel
                        {
                            Poi = poi,
                            CodigoCircunscripcion = codigoCircunscripcion,
                            CodigoCentroResponsabilidad = codigoCentroResponsabilidad,
                            CodigoMateriaJuridica = codigoMateriaJuridica,
                            NumeroObjetoGasto = numeroObjetoGasto,
                            CodigoCatalogoBienServicio = codigoCatalogoBienServicio,
                            Cantidad = cantidad,
                            ValorUnitario = valorUnitario,
                            Fundamentacion = fundamentacion
                        };

                        //registros.Add(registro);
                        registrosMapeados.Add((registro, row.RowNumber(), row));
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Error en la fila {row.RowNumber()}: uno o más campos obligatorios están vacíos ({string.Join(", ", erroresFila)}).");
                    }
                }               

                //Si todo Ok con los tipos de Datos, validamos si estan en base los datos
                var ejercicioActivo = await _repositoryModificacionOBG.ObtenerEjercicioActivo();

                if (errores.Count == 0)
                {
                    foreach (var (registro, fila, row) in registrosMapeados)
                    {
                        //Validar que el codigo de circunscripcion corresponde al usuario actual
                        if (Int32.Parse(registro.CodigoCircunscripcion.Trim()) != codCircunscripcion)
                        {
                            erroresValidacion.Add($"El Código de Circunscripción {registro.CodigoCircunscripcion} no corresponde al usuario actual (fila {fila}, columna {headers[5]})");
                        }

                        //Validar que poi sea del mismo ejercicio del presupuesto
                        if (registro.Poi != ejercicioActivo.ToString())
                        {
                            erroresValidacion.Add($"POI {registro.Poi} no coincide con el ejercicio activo {ejercicioActivo} (fila {fila}, columna {headers[0]})");
                        }

                        // Validar que el NumeroObjetoGasto existe en la base de datos
                        var objetoGastoExiste = await _repository.ObtenerCodigoObjetoGastoapartirdeNumeroObjetoGasto(int.Parse(registro.NumeroObjetoGasto));
                        if (objetoGastoExiste == 0)
                        {
                            erroresValidacion.Add($"Número de Objeto de Gasto {registro.NumeroObjetoGasto} no existe en la base de datos (fila {fila}, columna {headers[4]})");
                        }

                        // Validar que el CodigoCatalogoBienServicio existe en la base de datos
                        var bienServicioExiste = await _repository.ObtenerDescripcionBien(registro.CodigoCatalogoBienServicio);
                        if (bienServicioExiste == null)
                        {
                            erroresValidacion.Add($"Código de Catálogo de Bien/Servicio {registro.CodigoCatalogoBienServicio} no existe en la base de datos (fila {fila}, columna {headers[5]})");
                        }

                        // Validar que

                        var validacionUsuario = await _repository.ObtenerDatosValidacionUsuario(ciUsuarioSesion);
                        bool errorValidacionUsuario = true;

                        foreach(RegistrodesdeExcel valid in validacionUsuario)
                        {
                            if (registro.CodigoMateriaJuridica == valid.CodigoMateriaJuridica && registro.CodigoCentroResponsabilidad == valid.CodigoCentroResponsabilidad)
                            {
                                errorValidacionUsuario = false;
                                break;
                            }
                        }
                        if (errorValidacionUsuario)
                        {
                            erroresValidacion.Add($"El Código de Materia Jurídica {registro.CodigoMateriaJuridica} y el Código de Centro de Responsabilidad {registro.CodigoCentroResponsabilidad} no corresponden al usuario actual (fila {fila}, columna {headers[5]})");
                        }
                    }
                }


                if (errores.Count == 0 && erroresValidacion.Count == 0)
                {
                    bool banderaSolicitud = false;
                    var valorCodigoSolicitud = 0;
                    int valorCodigoSolicitudObjetodeGasto = 0;
                    string ultimoNumeroObjetoGasto = null;
                    var usuario_Inserto = await _repository.ObtenerCodigoUsuario(ciUsuarioSesion);

                    foreach (var (registro, fila, row) in registrosMapeados)
                    {

                        if (!banderaSolicitud)
                        {
                            banderaSolicitud = true;

                            //insertado de solicitudes bienes por circunscripcion
                            var requestSolicitud = new SolicitudBienesCircunscripcion
                            {
                                Poi = int.Parse(registro.Poi),
                                UsuarioInserto = usuario_Inserto,
                                CodigoCircunscripcion = int.Parse(registro.CodigoCircunscripcion),
                                CodigoMateria = int.Parse(registro.CodigoMateriaJuridica),
                                CodigoCentroResponsabilidad = int.Parse(registro.CodigoCentroResponsabilidad)
                            };

                            var resultado1 = await _repository.InsertarSolicitudBienesCircunscripcion(requestSolicitud);
                            valorCodigoSolicitud = resultado1.Items.First().CodigoSolicitud;
                        }

                        //INSERTAMOS LOS OBJETOS DE GASTOS
                        var comparar = registro.NumeroObjetoGasto.Trim();
                        if (ultimoNumeroObjetoGasto != registro.NumeroObjetoGasto.Trim())
                        {

                            var requestObjetoGasto = new SolicitudObjetoDetalle
                            {
                                CodigoSolicitud = valorCodigoSolicitud,
                                CodigoObjetoGasto = await _repository.ObtenerCodigoObjetoGastoapartirdeNumeroObjetoGasto(int.Parse(registro.NumeroObjetoGasto)),
                                UsuarioInserto = usuario_Inserto,
                                Estado = 1
                            };

                            var resultado2 = await _repository.InsertarSolicitudObjetosDetalle(requestObjetoGasto);


                            valorCodigoSolicitudObjetodeGasto = resultado2.Items;

                            ultimoNumeroObjetoGasto = registro.NumeroObjetoGasto.Trim();
                        }


                        //INSERTAMOS LOS BIENES POR OBJETOS
                        var requestBienes = new SolicitudObjetoBienesDetalle
                        {
                            CodigoSolicitud = valorCodigoSolicitud,
                            CodigoSolicitudObjeto = valorCodigoSolicitudObjetodeGasto,
                            NumeroBien = registro.CodigoCatalogoBienServicio,
                            Descripcion = await _repository.ObtenerDescripcionBien(registro.CodigoCatalogoBienServicio),
                            Cantidad = (int)registro.Cantidad,
                            CostoUnitario = (double)registro.ValorUnitario,
                            Fundamentacion = registro.Fundamentacion,
                            UsuarioInserto = usuario_Inserto
                        };

                        await _repository.InsertarSolicitudObjetosBienesDetalle(requestBienes);

                    }
                }
            }
        }
        catch (Exception ex)
        {
            errores.Add($"Error al procesar el archivo: {ex.Message}");
        }

        return (registros, errores,erroresValidacion);
    }
    public async Task<Datos<IEnumerable<VerificacionSolicitudes>>> ListadoVerificacionSolicitudes(VerificacionSolicitudesRequest request)
    {
        return await _repository.ListadoVerificacionSolicitudes(request);
    }
    public async Task<int> ValidarVersionAbierta(int ejercicio)
    {
        return await _repository.ValidarVersionAbierta(ejercicio);
    }

    #region DESARROLLO DE LA INCIDENCIA CSJ-138
   public async Task<string> Bienprocesado(string CodigoBienDetalle , int CodigoSolicitud, int CodigoSolicitudObjeto, bool eliminar)
    {
        return await _repository.Bienprocesado(CodigoBienDetalle,  CodigoSolicitud,  CodigoSolicitudObjeto , eliminar);
    }
    public async Task<int> ActualizarSolicitudObjetoBienesDetalle(SolicitudObjetoBienesDetalleDTO bienDetalle)
    {
        return await _repository.ActualizarSolicitudObjetoBienesDetalle(bienDetalle);
    }

    public async Task InsertarSolicitudObjetosBienesDetalle(SolicitudObjetoBienesDetalle bienDetalle)
    {
        await _repository.InsertarSolicitudObjetosBienesDetalle(bienDetalle);

    }
    public async Task<int> CrearSolicitudObjetosBienesDetalle(SolicitudObjetoBienesDetalleDTO bienDetalle)
    {
        var cedula = bienDetalle.UsuarioInserto.ToString();
        var codigoUsuario = await _repository.ObtenerCodigoUsuario(cedula);
        bienDetalle.UsuarioInserto = codigoUsuario;
        return await _repository.CrearSolicitudObjetosBienesDetalle(bienDetalle);

    }
    public async Task<int> EliminarSolicitudObjetosBienesDetalle(int codigoBienDetalle, int codigoSolicitud, int codigoObjetoGasto, string descripcionBienesNacional, int ejercicio)
    {
        return await _repository.EliminarSolicitudObjetosBienesDetalle(codigoBienDetalle, codigoSolicitud, codigoObjetoGasto, descripcionBienesNacional, ejercicio);
    }


    public async Task<List<SolicitudObjetoBienesDetalleDTO>> ObtenerDatosSolicitudObjetoBienesDetallePorCodigoSolicitud(int codigo, int pagina, int cantidadRegistros)
    {
        return await _repository.ObtenerDatosSolicitudObjetoBienesDetallePorCodigoSolicitud(codigo, pagina, cantidadRegistros);

    }

    public async Task<List<SolicitudObjetoBienesDetalleDTO>> ListadoBienesPorCriterioBusqueda(int ejercicio, int codigoSolicitud, int codigoSolicitudObjeto, int codigo, string descripcion, int pagina, int cantidadRegistros)
    {
        return await _repository.ListadoBienesPorCriterioBusqueda(ejercicio, codigoSolicitud, codigoSolicitudObjeto, codigo, descripcion, pagina, cantidadRegistros);

    }
    //public async Task<List<SolicitudObjetoCodigoDescripcionDTO>> ListadoBienesPorCodigoDescripcion(double costoUnitario, int ejercicio, int codigo, int codigoObjetoGasto, int codigoSolicitudGasto, string descripcion, int pagina, int cantidadRegistros)
    //{
    //    return await _repository.ListadoBienesPorCodigoDescripcion(costoUnitario, ejercicio, codigo, codigoObjetoGasto, codigoSolicitudGasto, descripcion, pagina, cantidadRegistros);

    //}
    public async Task<Datos<IEnumerable<SolicitudObjetoCodigoDescripcionDTO>>> ListadoBienesPorCodigoDescripcion(SolicitudObjetoBienesDetalleRequest request)

    {
        return await _repository.ListadoBienesPorCodigoDescripcion(request);

    }


    //filtro
    public async Task<Datos<IEnumerable<SolicitudObjetoBienesDetalleDTO>>> ListadoBienesPorCriterioBusquedaPorFiltros(SolicitudObjetoBienesDetalleRequest request)

    {
        return await _repository.ListadoBienesPorCriterioBusquedaPorFiltros(request);

    }


    public async Task<int> CantidadRegistrosBienes(double costoUnitario, int ejercicio, int codigoSolicitud, int codigoSolicitudObjeto, int codigo, string descripcionSolicitudObjetoBienesDetalle)
    {
        return await _repository.CantidadRegistrosBienes(costoUnitario, ejercicio, codigoSolicitud, codigoSolicitudObjeto, codigo, descripcionSolicitudObjetoBienesDetalle);
    }

    public async Task<int> EjercicioSolicitud()
    {
        return await _repository.EjercicioSolicitud();
    }
    public async Task<CantidadTotalGenericaDTO> TotalesCatidadBienesSolicitud(int codigoSolicitud, int codigoSolicitudObjeto)
    {
        return await _repository.TotalesCatidadBienesSolicitud(codigoSolicitud, codigoSolicitudObjeto);
    }

    #endregion


}

