using Application.Services.Interfaces.IRepository;
using Dapper;
using Domain.Entities.Anteproyecto;
using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.GenerarSolicitudporCircunscripcion;
using Domain.Entities.Request;
using Domain.Entities.VerificacionSolicitudes;
using Domain.Exceptions.ImportarArchivoSIPOIExcepcions;
using Microsoft.Extensions.Logging;
using System.Data;
using static Dapper.SqlMapper;
using Boolean = System.Boolean;

namespace Persistence.Repositories
{
    public class GenerarSolicitudporCircunscripcionRepository : IGenerarSolicitudporCircunscripcionRepository
    {
        private readonly DbConnections _conexion;
        private readonly ILogger<GenerarSolicitudCircunscripcionRepository> _logger;
        public GenerarSolicitudporCircunscripcionRepository(DbConnections conexion, ILogger<GenerarSolicitudCircunscripcionRepository> logger)
        {
            _conexion = conexion;
            _logger = logger;
        }


        public async Task<Datos<IEnumerable<SolicitudCircunscripcionesEstadoAbierto>>> ListarSolicitudesconEstadosAbiertos(SolicitudBienesCircunscripcionRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de obtener datos de solicitudes abiertas");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
            var query = string.Empty;
            var filtros = string.Empty;
            int? valorEstado = null;

            if(!string.IsNullOrEmpty(request.descripcionEstado))
            {
                switch (request.descripcionEstado.ToUpper())
                {
                    case "ABIERTO":
                        valorEstado = 1;
                        break;
                    case "CERRADO":
                        valorEstado = 2;
                        break;
                    case "ANULADO":
                        valorEstado = 3;
                        break;
                    case "":
                        valorEstado = null;
                        break;
                    default:
                        return null;
                }
            }

            try
            {              
                //Tratamos de identificar si el dato que viene es una fecha
                DateTime fechaBusqueda;
               
                bool esFechaBusqueda = DateTime.TryParseExact(request.TerminoDeBusqueda, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out fechaBusqueda);
                string terminoDeBusquedaSQL = esFechaBusqueda ? fechaBusqueda.ToString("yyyy-MM-dd") : request.TerminoDeBusqueda;              

                var parametros = new DynamicParameters();
                parametros.Add("@terminoDeBusqueda", terminoDeBusquedaSQL);
                parametros.Add("@saltarRegistros", (request.Pagina - 1) * request.CantidadRegistros);
                parametros.Add("@cantidadRegistros", request.CantidadRegistros);
                parametros.Add("@codigoSolicitud", request.CodigoSolicitud);
                parametros.Add("@cedulaUsuario", request.CedulaUsuario);
                parametros.Add("@parametroNroSolicitud", request.NumeroSolicitud);
                parametros.Add("@parametroPOI", request.POI);
                parametros.Add("@parametroCircunscripcion", request.Circunscripcion);
                parametros.Add("@parametroCentroResponsabilidad", request.CentroResponsabilidad);
                parametros.Add("@parametroMateria", request.MateriaJuridica);
                parametros.Add("@parametroFechaSolicitud", request.FechaEmision.HasValue ? request.FechaEmision.Value.ToString("yyyy-MM-dd") : null);
                parametros.Add("@parametroUsername", request.UsuarioSolicitante);
                parametros.Add("@parametroNombreUsuario", request.UsuarioNombreCompleto);
                parametros.Add("@estado", valorEstado);

                using (var connection = _conexion.CreateSqlConnection())
                {
                    using (var multi = await connection.QueryMultipleAsync(
                     "ListarSolicitudesConEstadosAbiertos",
                     parametros,
                     commandType: CommandType.StoredProcedure))
                    {
                        var resultado = await multi.ReadAsync<SolicitudCircunscripcionesEstadoAbierto>();
                        var totalRegistros = await multi.ReadSingleAsync<int>();

                        var listado = new Datos<IEnumerable<SolicitudCircunscripcionesEstadoAbierto>>
                        {
                            Items = resultado,
                            TotalRegistros = totalRegistros
                        };

                        _logger.LogInformation("Fin de Proceso de obtener datos de solicitudes abiertas");
                        return listado;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al buscar datos de solicitudes abiertas" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<CentroResponsabilidadyMateria> ObtenerCentrodeResponsabilidadyMateriaJuridica(string usuarioLogeado)
        {
            _logger.LogInformation("Inicio de Proceso de obtener codigo centro de responsabilidad y Codigo Materia");


            string query = @" SELECT  
                                vu.nombre_circunscripcion AS Circunscripción,
	                            vu.codigo_centro_responsabilidad as CodigoCentroResponsabilidad,
	                            vu.descripcion_centro_responsabilidad AS CentrodeResponsabilidad, 
	                            vu.codigo_materia as CodigoMateria,
	                            vu.descripcion_materia AS materia,
                                vu.cedula_identidad as CedulaUsuario
                            FROM  
                                vListaUsuariosPorCentrosResponsabilidad vu 
                            WHERE  
                                vu.cedula_identidad = @desc";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        desc = usuarioLogeado.ToUpper()
                    };

                    var resultado = await connection.QuerySingleOrDefaultAsync<CentroResponsabilidadyMateria>(
                        query, parametro);

                    _logger.LogInformation("Fin de Proceso de obtener codigo centro de responsabilidad y Codigo Materia");

                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener el codigo de la tabla catalogo_centros_responsabilidad" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<UsuarioCircunscripcion> ObtenerCentroResponsabilidadporUsuario(UsuarioCircunscipcionRequest2 request)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener Centro de responsabilidad del usuario por circunscripcion");

            try
            {
                string query = @"
                            SELECT	                           
                                codigo_centro_responsabilidad as CodigoCentroResponsabilidad,
                                descripcion_centro_responsabilidad as DescripcionCentroResponsabilidad                               
                            FROM vListaUsuariosPorCentrosResponsabilidad
                            where cedula_identidad = @cedulaUsuario";


                var parametros = new DynamicParameters();

                parametros.Add("@cedulaUsuario", request.CedulaUsuario);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var resultado = await connection.QueryAsync<dynamic>(query, parametros);

                    var usuarioCircunscripcion = new UsuarioCircunscripcion
                    {
                        CentrosResponsabilidad = resultado
                    .Select(r => new CentroResponsabilidad
                    {
                        CodigoCentroResponsabilidad = r.CodigoCentroResponsabilidad,
                        DescripcionCentroResponsabilidad = r.DescripcionCentroResponsabilidad
                    })
                    .Distinct()
                    .ToList(),
                    };

                    _logger.LogInformation("Fin de Proceso de Obtener Centro de responsabilidad del usuario por circunscripcion");

                    return usuarioCircunscripcion;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener el listado de centro de responsabilidad" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<UsuarioCircunscripcion> ObtenerMateriaporUsuarioyCentroResponsabilidad(UsuarioCircunscipcionRequest2 request)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener Materia del usuario por circunscripcion y centro de responsabilidad");

            try
            {
                string query = @"
                            SELECT	                           
                                codigo_materia as CodigoMateria,
                                descripcion_materia as DescripcionMateria                             

                            FROM vListaUsuariosPorCentrosResponsabilidad
                            where cedula_identidad = @cedulaUsuario
                            and codigo_centro_responsabilidad = @codigoCentro";


                var parametros = new DynamicParameters();

                parametros.Add("@cedulaUsuario", request.CedulaUsuario);
                parametros.Add("@codigoCentro", request.CodigoCentroResponsabilidad);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var resultado = await connection.QueryAsync<dynamic>(query, parametros);

                    var usuarioCircunscripcion = new UsuarioCircunscripcion
                    {
                        Materias = resultado
                    .Select(r => new Materia
                    {
                        CodigoMateria = r.CodigoMateria,
                        DescripcionMateria = r.DescripcionMateria
                    })
                    .Distinct()
                    .ToList(),
                    };

                    _logger.LogInformation("Fin de Proceso de Obtener Materia del usuario por circunscripcion y centro de responsabilidad");

                    return usuarioCircunscripcion;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener el listado de materia por usuario y centro de responsabilidad" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<Datos<IEnumerable<ObjetodeGastosDTO>>> ObtenerObjetosGasto(ObjetosdeGastosRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener objetos de gastos de la tabla matriz");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;

            try
            {
                string query = string.Empty;
                string filtro = string.Empty;

                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||                    
                     !string.IsNullOrEmpty(request.NumeroObjetoGasto) ||
                     !string.IsNullOrEmpty(request.DescripcionObjetoGasto)            
                     )
                {                    
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        filtro += @"
                                     AND (@terminoDeBusqueda IS NULL OR @terminoDeBusqueda = '' OR 
                                         bp.numero_objeto_gasto LIKE '%' + @terminoDeBusqueda + '%' OR 
                                         bp.descripcion_objeto_gasto LIKE '%' + @terminoDeBusqueda + '%')";
                    }
                    else
                    {
                        filtro += @"
                                 	AND (@parametroNroGasto IS NULL OR bp.numero_objeto_gasto LIKE '%' + @parametroNroGasto + '%')
                                    AND (@parametroDescripOBG IS NULL OR bp.descripcion_objeto_gasto LIKE '%' + @parametroDescripOBG + '%')
                               ";
                    }
                }

                query = string.Format(@"
                                SELECT DISTINCT
                                    bp.codigo_objeto_gasto AS CodigoObjetoGasto,
                                    bp.numero_objeto_gasto AS Numero_Objeto_Gasto,
                                    bp.descripcion_objeto_gasto AS Descrip_Objeto_Gasto
                                FROM
                                    vListaBienesPrioritarios bp
                                WHERE
                                    bp.ejercicio IN (@ejercicio, 0)
                                    {0}
                                ORDER BY bp.numero_objeto_gasto
                                OFFSET @saltarRegistros ROWS
                                FETCH NEXT @cantidadRegistros ROWS ONLY",filtro);


                    string queryCantidadTotal = string.Format(@"
                            SELECT COUNT(DISTINCT bp.codigo_objeto_gasto)
                            FROM
                                vListaBienesPrioritarios bp
                            WHERE
                                bp.ejercicio IN (@ejercicio, 0)
                                {0}",filtro);

                string queryObtenerEjercicio = "select vfc.EJERCICIO from VERSION_FECHA_CIERRE vfc where vfc.ACTIVO = 1";
                int ejercicio = 0;
                int valorEjercicio = 0;

               

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    ejercicio = await connection.ExecuteScalarAsync<int>(queryObtenerEjercicio);

                    if (request.Ejercicio == 0)
                    {
                        valorEjercicio = ejercicio - 1;
                    }
                    else
                        valorEjercicio =  request.Ejercicio;

                    var parametros = new DynamicParameters();
                    parametros.Add("@terminoDeBusqueda", $"%{request.TerminoDeBusqueda}%");
                    parametros.Add("@saltarRegistros", saltarRegistros);
                    parametros.Add("@cantidadRegistros", request.CantidadRegistros);
                    parametros.Add("@parametroNroGasto", request.NumeroObjetoGasto);
                    parametros.Add("@parametroDescripOBG", request.DescripcionObjetoGasto);
                    parametros.Add("@ejercicio", valorEjercicio);

                    var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotal, parametros);
                        
                    var resultado = await connection.QueryAsync<ObjetodeGastosDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<ObjetodeGastosDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalRegistros
                    };

                    _logger.LogInformation("Fin de Proceso de Obtener objetos de gastos de la tabla matriz");

                    return listado;
                }              

            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener el listado de la tabla de Objeto de gastos" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<Datos<IEnumerable<DetalleObjetoGastoporNroSolicitudDTO>>> ListarObjetodeGastosporCodigoSolicitud(ListaObjetoGastosdeSolicitudRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener objetos de gastos por codigo de solicitud");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
           
            try
            {
                var parametros = new DynamicParameters();
                parametros.Add("@terminoDeBusqueda", request.TerminoDeBusqueda);
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.CantidadRegistros);
                parametros.Add("@codigoSolicitud", request.CodigoSolicitud);

                parametros.Add("@parametroObjetoDeGasto", request.ObjetoDeGasto);
                parametros.Add("@parametroCostoUnitario", request.CostoUnitario);
                parametros.Add("@parametroCantidadTotal", request.CantidadTotal);
                parametros.Add("@valordescripcionEstado", request.Estado);
                parametros.Add("@parametroCodigoOBG", request.CodigoObjetoGasto);                

                using (var connection = _conexion.CreateSqlConnection())
                {
                    var multi = await connection.QueryMultipleAsync(
                        "ListarObjetodeGastosporCodigoSolicitud",
                        parametros,
                        commandType: CommandType.StoredProcedure);

                    var resultado = await multi.ReadAsync<DetalleObjetoGastoporNroSolicitudDTO>();
                    int totalRegistros = 0;
                    if (resultado.Any())
                    {
                        totalRegistros = resultado.First().TotalRegistros;
                    }

                    var listado = new Datos<IEnumerable<DetalleObjetoGastoporNroSolicitudDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalRegistros
                    };

                    _logger.LogInformation("Fin de Proceso de obtener datos de solicitudes");
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener el listado de objetos de Gastos" + "||-->" + ex.Message + "<--||");
            }
        }               
        public async Task<Datos<IEnumerable<SolicitudesCabeceraInsertResponse>>> InsertarSolicitudBienesCircunscripcion(SolicitudBienesCircunscripcion solicitudBienes)
        {
            _logger.LogInformation("Inicio de Proceso de insertar solicitud bienes circunscripcion {@solicitud}", solicitudBienes);

            string queryUltimoValorCodigo = "SELECT ISNULL(MAX(codigo_solicitud),0) FROM solicitudes_bienes_circunscripcion";
            string queryUltimoValorSecuenciaSolicitud = "SELECT ISNULL(MAX(numero_solicitud),0) FROM solicitudes_bienes_circunscripcion";
            string queryObtenerEjercicio = "select vfc.EJERCICIO from VERSION_FECHA_CIERRE vfc where vfc.ACTIVO = 1";
            string queryObtenerCircunscripcion = "select codigo_circunscripcion from circunscripciones where codigo_circunscripcion_jurisdiccional = @CodigoCircunscripcion";

            string query = @"
                INSERT INTO solicitudes_bienes_circunscripcion
                (codigo_solicitud,fecha_solicitud, ejercicio, codigo_materia, codigo_centro_responsabilidad, codigo_circunscripcion, 
                 usuario_inserto, fecha_inserto, numero_solicitud, estado, poi)
                VALUES
                (@CodigoSolicitud,GETDATE(), @Ejercicio, @CodigoMateria, @CodigoCentroResponsabilidad, @CodigoCircunscripcion,
                 @UsuarioInserto, GETDATE(), @NumeroSolicitud, @Estado, @Poi)";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    int ultimoValorCodigo = await connection.ExecuteScalarAsync<int>(queryUltimoValorCodigo);
                    int nuevoCodigoSolicitud = ultimoValorCodigo + 1;

                    int ultimoValorSecuencia = await connection.ExecuteScalarAsync<int>(queryUltimoValorSecuenciaSolicitud);
                    int nuevoCodigoSecuencia = ultimoValorSecuencia + 1;

                    //Obtener Ejercicio
                    int ejercicio = await connection.ExecuteScalarAsync<int>(queryObtenerEjercicio);

                    //Obtener Codigo Circunscripcion a partir del codigo jurisdiccional que envia el front
                    var parametro = new { solicitudBienes.CodigoCircunscripcion };
                    int codigoCircunscripcion = await connection.ExecuteScalarAsync<int>(queryObtenerCircunscripcion, parametro);

                    solicitudBienes.Ejercicio = ejercicio;
                    solicitudBienes.CodigoCircunscripcion = codigoCircunscripcion;
                    solicitudBienes.CodigoSolicitud = nuevoCodigoSolicitud;
                    solicitudBienes.NumeroSolicitud = nuevoCodigoSecuencia;
                    solicitudBienes.Estado = 1; // Estado Abierto

                    var resultado = await connection.ExecuteAsync(query, solicitudBienes);

                    var response = new SolicitudesCabeceraInsertResponse
                    {
                        CodigoSolicitud = nuevoCodigoSolicitud,
                        NumeroSolicitud = nuevoCodigoSecuencia,
                        Ejercicio = ejercicio
                    };

                    var listado = new Datos<IEnumerable<SolicitudesCabeceraInsertResponse>>
                    {
                        Items = new List<SolicitudesCabeceraInsertResponse> { response },
                        TotalRegistros = 1
                    };

                    _logger.LogInformation("Fin de Proceso de insertar solicitud bienes circunscripcion {@solicitud}", solicitudBienes);

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al insertar los datos en la tabla Solicitud Bienes Circunscripcion" + "||-->" + ex.Message + "<--||");
            }
        }               
        public async Task<Datos<int>> InsertarSolicitudObjetosDetalle(SolicitudObjetoDetalle solicitudObjetoDetalle)
        {
            _logger.LogInformation("Inicio de Proceso de insertar solicitud objetos detalle {@solicitud}", solicitudObjetoDetalle);

            string query = @"
                            INSERT INTO solicitudes_objetos_detalle 
                            (codigo_solicitud_objeto, codigo_objeto_gasto, fecha_inserto, usuario_inserto, codigo_solicitud, estado)
                            VALUES
                            (@CodigoSolicitudObjeto, @CodigoObjetoGasto, GETDATE(), @UsuarioInserto, @CodigoSolicitud, @Estado)";

            var queryUltimoValorCodigoSolicitudObj = "SELECT ISNULL(MAX(codigo_solicitud_objeto),0) FROM solicitudes_objetos_detalle WHERE codigo_solicitud = @codigoSolicitud";

            var existe = "select Count(*) from solicitudes_objetos_detalle where codigo_solicitud = @codigoSolicitud and codigo_objeto_gasto = @codigoOBG";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        codigoSolicitud = $"{solicitudObjetoDetalle.CodigoSolicitud}",
                        codigoOBG = $"{solicitudObjetoDetalle.CodigoObjetoGasto}"
                    };

                    int valorExiste = await connection.ExecuteScalarAsync<int>(existe, parametro);
                    int nuevoCodigoSolicitudObjeto = 0;
                    int totalRegistro = 0;

                    if (valorExiste == 0)
                    {
                        int ultimoValorCodigoSolicitudObj = await connection.ExecuteScalarAsync<int>(queryUltimoValorCodigoSolicitudObj, parametro);
                        nuevoCodigoSolicitudObjeto = ultimoValorCodigoSolicitudObj + 1;

                        solicitudObjetoDetalle.CodigoSolicitudObjeto = nuevoCodigoSolicitudObjeto;
                        solicitudObjetoDetalle.Estado = 1;

                        var resultado = await connection.ExecuteAsync(query, solicitudObjetoDetalle);
                        totalRegistro = 1;
                    }
                    else
                    {
                        totalRegistro = -1;
                    }
                   

                    var listado = new Datos<int>
                    {
                        Items = nuevoCodigoSolicitudObjeto,
                        TotalRegistros = totalRegistro
                    };


                    _logger.LogInformation("fin de Proceso de insertar solicitud objetos detalle {@solicitud}", solicitudObjetoDetalle);

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al insertar los datos en la tabla Solicitud Objetos Detalle " + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task InsertarSolicitudObjetosBienesDetalle(SolicitudObjetoBienesDetalle solicitudObjetoBienesDetalle)
        {
            _logger.LogInformation("Inicio de Proceso de insertar solicitud objetos bienes detalle {@solicitud}", solicitudObjetoBienesDetalle);

            string query = @"
                INSERT INTO solicitudes_objetos_bienes_detalle
                (codigo_bien_detalle, numero_bien, descripcion, costo_unitario, cantidad, usuario_inserto, fecha_inserto, codigo_solicitud_objeto,codigo_solicitud,fundamentacion)
                VALUES
                (@CodigoBienDetalle, @NumeroBien, @Descripcion, @CostoUnitario, @Cantidad, @UsuarioInserto, GETDATE(), @CodigoSolicitudObjeto,@CodigoSolicitud,@Fundamentacion)";

            string queryUltimoValorSecuenciaSolicitudBien = "SELECT ISNULL(MAX(codigo_bien_detalle),0) " +
                "FROM solicitudes_objetos_bienes_detalle sb " +
                "where sb.codigo_solicitud =@codigoSolicitud and sb.codigo_solicitud_objeto =@codigoSolicitudOBJ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        codigoSolicitud = $"{solicitudObjetoBienesDetalle.CodigoSolicitud}",
                        codigoSolicitudOBJ = $"{solicitudObjetoBienesDetalle.CodigoSolicitudObjeto}"
                    };

                    int ultimoValorCodigoSolicitudBien = await connection.ExecuteScalarAsync<int>(queryUltimoValorSecuenciaSolicitudBien, parametro);
                    int nuevoCodigoSolicitudBien = ultimoValorCodigoSolicitudBien + 1;

                    solicitudObjetoBienesDetalle.CodigoBienDetalle = nuevoCodigoSolicitudBien;

                    var resultado = await connection.ExecuteAsync(query, solicitudObjetoBienesDetalle);

                    _logger.LogInformation("Fin de Proceso de insertar solicitud objetos bienes detalle {@solicitud}", solicitudObjetoBienesDetalle);
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error al insertar los datos en la tabla Solicitud Objetos Bienes Detalle" + "||-->" + ex.Message + "<--||");
            }

        }
        public async Task<int> ObtenerCodigoUsuario(string cedula)
        {
            _logger.LogInformation("Inicio de Proceso de obtener el nombre del usuario con cedula {@cedula}", cedula);


            string queryUsuarioSesion = "SELECT vu.codigo_usuario FROM vListaUsuariosPorCentrosResponsabilidad vu " +
                                        "WHERE vu.cedula_identidad like '%' + @cedula + '%'";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        cedula = $"%{cedula.Trim()}%"
                    };

                    var username = await connection.QuerySingleOrDefaultAsync<int>(
                      queryUsuarioSesion, parametro);

                    _logger.LogInformation("Fin de Proceso de obtener el nombre del usuario con cedula {@cedula}", cedula);


                    return username;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener el nombre de usuario de la tabla usuarios_poder_judicial" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<int> ObtenerNombreUsuarioCentroResponsabilidad(string cedula, int codigoCentroResp, int codigoMateria)
        {
            _logger.LogInformation("Inicio de Proceso de obtener el nombre del usuario con cedula {@cedula}", cedula);


            string queryUsuarioSesion = "SELECT vu.codigo_usuario FROM vListaUsuariosPorCentrosResponsabilidad vu " +
                                        "WHERE vu.cedula_identidad like '%' + @cedula + '%' AND codigo_centro_responsabilidad = @codigoCentroResp AND codigo_materia = @codigoMateria ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        cedula = $"%{cedula.Trim()}%",
                        codigoCentroResp,
                        codigoMateria
                    };

                    var username = await connection.QuerySingleOrDefaultAsync<int>(
                      queryUsuarioSesion, parametro);

                    _logger.LogInformation("Fin de Proceso de obtener el nombre del usuario con cedula {@cedula}", cedula);


                    return username;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener el nombre de usuario de la tabla usuarios_poder_judicial" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<int> ActualizarEstadoSolicitudesysusObjetosGastos(ActualizarEstadoSolicitudes2Request request)
        {
            _logger.LogInformation("Inicio de Proceso de Actualizar el estado de las solicitudes y sus objetos de gasto");

            if (request.CodigosSolicitud == null || !request.CodigosSolicitud.Any())
            {
                throw new GeneracionSolicitudesException("Debe proporcionar al menos un código de solicitud.");
            }

            try
            {
                var resultadoSolicitud = 0;
                var resultadoObjetoGasto = 0;
                var totalUpdated = 0;
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    // Query para actualizar el estado de las solicitudes
                    string querySolicitud = @"
                        UPDATE solicitudes_bienes_circunscripcion
                        SET estado = @Estado,
                            usuario_anulacion = @UsuarioAnulacion,
                            fecha_anulacion = GETDATE()
                        WHERE codigo_solicitud = @CodigoSolicitud
                        AND NOT EXISTS (
                              SELECT 1
                              FROM solicitudes_objetos_detalle obg
                              WHERE obg.codigo_solicitud = solicitudes_bienes_circunscripcion.codigo_solicitud
                                AND obg.estado = 2
                          )";

                    // Query para actualizar el estado de los objetos de gasto relacionados
                    string queryObjetoDetalle = @"
                        UPDATE solicitudes_objetos_detalle
                        SET estado = @Estado
                        WHERE codigo_solicitud = @CodigoSolicitud
                        AND estado != 2";                    

                    foreach (var codigoSolicitud in request.CodigosSolicitud)
                    {                      
                        resultadoSolicitud = await connection.ExecuteAsync(querySolicitud, 
                            new { 
                                CodigoSolicitud = codigoSolicitud, 
                                Estado = 3,
                                UsuarioAnulacion = request.UsuarioAnulacion
                            });

                        resultadoObjetoGasto = await connection.ExecuteAsync(queryObjetoDetalle, 
                            new {
                                CodigoSolicitud = codigoSolicitud,
                                Estado = 3 
                            });

                        totalUpdated += resultadoSolicitud;                                               
                    }

                    _logger.LogInformation("Fin de Proceso de Actualizar el estado de las solicitudes y sus objetos de gasto");
                    return totalUpdated;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al actualizar el estado de las solicitudes y sus objetos de gasto" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<int> ModificarSolicitudBienesCircunscripcion(ModificarSolicitudV2Request request)
        {
            _logger.LogInformation("Inicio del Proceso de modificar solicitud de bienes de circunscripción {@solicitud}", request);

            string query = @"UPDATE solicitudes_bienes_circunscripcion
                                SET poi = @poi,
                                    codigo_materia = @codigoMateria,
                                    codigo_centro_responsabilidad = @codigoCentroResponsabilidad,
                                    usuario_ultima_modificacion = @usuarioModificacion,
                                    fecha_ultima_modificacion = GETDATE()
                                WHERE codigo_solicitud = @codigoSolicitud";

            var parametros = new DynamicParameters();
            parametros.Add("@codigoCentroResponsabilidad", request.CodigoCentroResponsabilidad);
            parametros.Add("@codigoMateria", request.CodigoMateria);
            parametros.Add("@poi", request.Poi);
            parametros.Add("@usuarioModificacion", request.UsuarioModificacion);
            parametros.Add("@codigoSolicitud", request.CodigoSolicitud);

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var resultado = await connection.ExecuteAsync(query, parametros);

                    _logger.LogInformation("Fin del Proceso de modificar solicitud de bienes de circunscripción {@solicitud}", request);
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new RegistrosParaSolicitudException("Ocurrió un error al modificar los datos en la tabla Solicitud Bienes Circunscripción" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<int> ActualizarEstadoObjetodeGasto(int codigoSolicitud, int codigoObjetoGasto, int estado)
        {
            _logger.LogInformation("Inicio de Proceso de Actualizar el estado del objeto de gasto");

            if (codigoSolicitud < 1 || codigoObjetoGasto < 1)
            {
                throw new GeneracionSolicitudesException("Los valores de Codigo Solicitud y Codigo Objeto de Gasto deben ser números positivos.");
            }
        
            int valor =0;

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    if (estado == 2)
                    {
                        string validarObjeto = " SELECT count( numero_bien) FROM solicitudes_objetos_bienes_detalle " +
                                          " WHERE codigo_solicitud_objeto = @codigoObjetoGasto" +
                                          " AND codigo_solicitud = @codigoSolicitud and (costo_unitario*cantidad=0)";
                        string validarOG = " SELECT count( numero_bien) FROM solicitudes_objetos_bienes_detalle " +
                                       " WHERE codigo_solicitud_objeto = @codigoObjetoGasto" +
                                       " AND codigo_solicitud = @codigoSolicitud";

                        var validaOG = await connection.QuerySingleOrDefaultAsync<int>(validarOG, new { codigoSolicitud, codigoObjetoGasto });
                        var validar = await connection.QuerySingleOrDefaultAsync<int>(validarObjeto, new { codigoSolicitud, codigoObjetoGasto });
                        if (validar > 0)
                        {
                            valor = 3;                           
                        }
                        else
                        

                        if (validaOG == 0)
                        {
                            valor = 2;
                        }
                    }

                    if (valor == 0)
                    {
                        string query = $@"UPDATE solicitudes_objetos_detalle
                                        SET estado = @estado
                                        WHERE codigo_solicitud_objeto = @codigoObjetoGasto
                                        AND codigo_solicitud = @codigoSolicitud";

                        var resultado = await connection.ExecuteAsync(query, new { codigoSolicitud, codigoObjetoGasto, estado });

                        _logger.LogInformation("Fin de Proceso de Actualizar el estado del objeto de gasto");
                        valor=resultado;
                       
                    }
                }
                return valor;
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException(ex.Message);
            }
        }
        public async Task<Datos<int>> EliminarObjetodeGasto(int codigoSolicitudObjeto, int codigoSolicitud)
        {
            _logger.LogInformation("Inicio de Proceso de eliminar el objeto de gasto de la lista");

            try
            {
                string queryOBG = @"
                                    DELETE FROM solicitudes_objetos_detalle 
                                    WHERE codigo_solicitud = @codigoSolicitud
	                                    AND codigo_solicitud_objeto = @codigoSolicitudObjeto";

                string queryBienes = @"
                                    DELETE FROM solicitudes_objetos_bienes_detalle
                                    WHERE codigo_solicitud_objeto = @codigoSolicitudObjeto
                                        AND codigo_solicitud = @codigoSolicitud";

                var parametros = new DynamicParameters();
                parametros.Add("@codigoSolicitudObjeto", codigoSolicitudObjeto);
                parametros.Add("@codigoSolicitud", codigoSolicitud);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    //Primero eliminamos los biens que pertenecen a ese objeto de gasto
                    int filasAfectadasBienes = await connection.ExecuteAsync(queryBienes, parametros);

                    //posteriormente se elimina el objeto de gasto
                    int filasAfectadasOBG = await connection.ExecuteAsync(queryOBG, parametros);

                    var retorno = new Datos<int>
                    {
                        Items = filasAfectadasOBG,
                        TotalRegistros = filasAfectadasOBG
                    };

                    _logger.LogInformation("Fin de Proceso de eliminar el objeto de gasto de la lista");

                    return retorno;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al intentar eliminar el registro de objeto de gasto " + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<Datos<ObjetosdeGastosyBienesporNroSolicitud>> VisualizarObjetosdeGastosyBienesporNroSolicitud(int codigoSolicitud, int pagina, int cantidadRegistros, int codigoSolicitudObjeto, int? estado)
        {
            _logger.LogInformation("Inicio de Proceso de Visualizar La solicitud por Objeto de Gastos y los Bienes relacionados a ese objeto");

            try
            {
                int saltarRegistros = (pagina - 1) * cantidadRegistros;

                string query = $@"SELECT  
                                            s.numero_solicitud AS ""NumeroSolicitud"",  
                                            s.poi AS ""POI"",  
                                            c.nombre_circunscripcion AS ""Circunscripcion"",  
                                            v.descripcion_centro_responsabilidad AS ""CentroResponsabilidad"",  
                                            m.descrip_materia AS ""MateriaJuridica"",   
                                             s.fecha_solicitud AS ""FechaEmision"", 
                                            sod.estado AS ""EstadoOG"", 
                                            og.numero_objeto_gasto AS ""NroOG"",  
                                            og.descrip_objeto_gasto AS ""ObjetoGasto"",
	                                        bd.codigo_bien_detalle AS ""CodigoBienDetalle"",
	                                        bd.numero_bien AS ""NumeroBien"",
	                                        bd.descripcion AS ""Descripcion"",
	                                        bd.costo_unitario AS ""CostoUnitario"",
	                                        bd.cantidad AS ""Cantidad"",
                                            bd.fundamentacion AS ""Fundamentación"",
	                                        bd.costo_unitario * bd.cantidad AS ""MontoTotal""

                                        FROM  
                                            solicitudes_bienes_circunscripcion s 
	                                        JOIN circunscripciones c ON c.codigo_circunscripcion = s.codigo_circunscripcion
	                                        JOIN materias m ON m.codigo_materia = s.codigo_materia 
	                                        JOIN vListaCentrosResponsabilidadPorCircunscripcion v 
	                                        ON v.codigo_centro_responsabilidad = s.codigo_centro_responsabilidad
                                            JOIN solicitudes_objetos_detalle sod ON sod.codigo_solicitud = s.codigo_solicitud 
                                            JOIN objeto_gasto og ON sod.codigo_objeto_gasto = og.codigo_objeto_gasto 
	                                        JOIN solicitudes_objetos_bienes_detalle bd ON bd.codigo_solicitud = s.codigo_solicitud 
	                                        AND bd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto
	   
                                        WHERE  
                                            s.estado = @estado
                                            AND sod.codigo_solicitud_objeto = @codigoSolicitudObjeto
                                            AND s.codigo_solicitud = @codigoSolicitud

                                        ORDER BY s.numero_solicitud
                                        OFFSET @saltarRegistros ROWS
                                        FETCH NEXT @cantidadRegistros ROWS ONLY;";

                string queryCantidadTotalREgistros = $@"SELECT COUNT(*) AS TotalRegistros
                                            FROM solicitudes_bienes_circunscripcion s 
                                            JOIN circunscripciones c ON c.codigo_circunscripcion = s.codigo_circunscripcion
                                            JOIN materias m ON m.codigo_materia = s.codigo_materia 
                                            JOIN vListaCentrosResponsabilidadPorCircunscripcion v ON v.codigo_centro_responsabilidad = s.codigo_centro_responsabilidad
                                            JOIN solicitudes_objetos_detalle sod ON sod.codigo_solicitud = s.codigo_solicitud 
                                            JOIN objeto_gasto og ON sod.codigo_objeto_gasto = og.codigo_objeto_gasto 
                                            JOIN solicitudes_objetos_bienes_detalle bd ON bd.codigo_solicitud = s.codigo_solicitud AND bd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto
                                            WHERE s.estado = @estado
                                            AND sod.codigo_solicitud_objeto = @codigoSolicitudObjeto
                                            AND s.codigo_solicitud = @codigoSolicitud";


                var parametros = new DynamicParameters();              
                parametros.Add("@estado", estado);
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", cantidadRegistros);
                parametros.Add("@codigoSolicitud", codigoSolicitud);
                parametros.Add("@codigoSolicitudObjeto", codigoSolicitudObjeto);


                using (var connection = this._conexion.CreateSqlConnection())
                {

                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);        

                    var resultado = await connection.QueryAsync<ObjetosdeGastosyBienesporNroSolicitud, Bienes, ObjetosdeGastosyBienesporNroSolicitud>(
                    query,
                    (objetoGasto, bien) =>
                    {
                        objetoGasto.Bienes = objetoGasto.Bienes ?? new List<Bienes>();
                        objetoGasto.Bienes.Add(bien);
                        return objetoGasto;
                    },
                        parametros,
                        splitOn: "CodigoBienDetalle"
                    );

                    var agrupado = resultado.GroupBy(r => r.NumeroSolicitud).Select(g =>
                    {
                        var primerResultado = g.First();
                        primerResultado.Bienes = g.SelectMany(r => r.Bienes).ToList();
                        return primerResultado;
                    });

                    var listado = new Datos<ObjetosdeGastosyBienesporNroSolicitud>
                    {
                        Items = agrupado.FirstOrDefault(),
                        TotalRegistros = totalTegistros
                    };

                    _logger.LogInformation("Fin de Proceso de Visualizar La solicitud por Objeto de Gastos y los Bienes relacionados a ese objeto");

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener la consulta de Visualizacion del Objeto y sus Bienes por Solicitud" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<int> ObtenerCodigoObjetoGastoapartirdeNumeroObjetoGasto(int numeroObjetoGasto)
        {
            _logger.LogInformation("Inicio de Proceso de obtener codigo objeto de gasto apartir del numero de objeto de gasto");

            string query = @"select distinct(vl.codigo_objeto_gasto) from vListaBienesPrioritarios vl where vl.numero_objeto_gasto = @desc";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        desc = $"{numeroObjetoGasto}"
                    };

                    int codigoCentroResponsabilidad = await connection.QuerySingleOrDefaultAsync<int>(
                        query, parametro);

                    _logger.LogInformation("Fin de Proceso de obtener codigo objeto de gasto apartir del numero de objeto de gasto");

                    return codigoCentroResponsabilidad;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener el codigo de la tabla Objeto de Gastos" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<string> ObtenerDescripcionBien(string codigoCatalogo)
        {
            _logger.LogInformation("Inicio de Proceso de obtener descripcion del bien a partir del codigo catalogo");

            string query = @"select distinct(vl.descripcion_catalogo) from vListaBienesPrioritarios vl where vl.codigo_catalogo = @desc";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        desc = $"{codigoCatalogo}"
                    };

                    string descripcionBien = await connection.QuerySingleOrDefaultAsync<string>(query, parametro);

                    _logger.LogInformation("Fin de Proceso de obtener descripcion del bien a partir del codigo catalogo");

                    return descripcionBien;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener la descripcion del bien" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<List<RegistrodesdeExcel>> ObtenerDatosValidacionUsuario(string cedulaUsuario)
        {
            _logger.LogInformation("Inicio de Proceso de obtener datos para la validacion");

            string query = @"select c.codigo_circunscripcion_jurisdiccional as CodigoCircunscripcion,
	            v.codigo_materia as CodigoMateriaJuridica,
	            v.codigo_centro_responsabilidad as CodigoCentroResponsabilidad
            from vListaUsuariosPorCentrosResponsabilidad v
            left join circunscripciones c on c.codigo_circunscripcion = v.codigo_circunscripcion
            where v.cedula_identidad = @cedulaUsuario";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        cedulaUsuario = $"{cedulaUsuario.Trim()}"
                    };

                    var result = (await connection.QueryAsync<RegistrodesdeExcel>(query, parametro)).ToList();

                    _logger.LogInformation("Fin de Proceso de obtener datos para la validacion");

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener la descripcion del bien" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<Datos<IEnumerable<VerificacionSolicitudes>>> ListadoVerificacionSolicitudes(VerificacionSolicitudesRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de obtener datos de solicitudes");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;

            try
            {
                // Tratamos de identificar si el dato que viene es una fecha
                DateTime fechaBusqueda;
                bool esFechaBusqueda = DateTime.TryParseExact(request.TerminoDeBusqueda, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out fechaBusqueda);
                string terminoDeBusquedaSQL = esFechaBusqueda ? fechaBusqueda.ToString("yyyy-MM-dd") : request.TerminoDeBusqueda;

                // Parámetros del procedimiento almacenado
                var parametros = new DynamicParameters();
                parametros.Add("@TerminoBusqueda", terminoDeBusquedaSQL);
                parametros.Add("@NumeroSolicitud", request.NumeroSolicitud);
                parametros.Add("@Poi", request.Poi);
                parametros.Add("@Circunscripcion", request.Circunscripcion);
                parametros.Add("@CentroResponsabilidad", request.CentroResponsabilidad);
                parametros.Add("@MateriaJuridica", request.MateriaJuridica);
                parametros.Add("@UsuarioSolicitante", request.UsuarioSolicitante);
                parametros.Add("@FechaEmision", request.FechaEmision.HasValue ? request.FechaEmision.Value.ToString("yyyy-MM-dd") : null);
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.CantidadRegistros);
                parametros.Add("@ejercicio", request.Ejercicio);

                using (var connection = _conexion.CreateSqlConnection())
                {
                    var multi = await connection.QueryMultipleAsync(
                        "VerificacionyAprobaciondeSolicitudes",
                        parametros,
                        commandType: CommandType.StoredProcedure);

                    var resultado = await multi.ReadAsync<VerificacionSolicitudes>();
                    int totalRegistros = 0;
                    if (resultado.Any())
                    {
                        totalRegistros = resultado.First().TotalRegistro;
                    }

                    var listado = new Datos<IEnumerable<VerificacionSolicitudes>>
                    {
                        Items = resultado,
                        TotalRegistros = totalRegistros
                    };

                    _logger.LogInformation("Fin de Proceso de obtener datos de solicitudes");
                    return listado;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error inesperado al buscar datos de solicitudes");
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al buscar datos de solicitudes" + "||-->" + ex.Message + "<--||");
            }
        }
        public async Task<int> ValidarVersionAbierta(int ejercicio)
        {
            _logger.LogInformation("Inicio de Proceso de validar si la version esta abierta");

            string query = @"SELECT count(*)
                            FROM VERSIONES_ANTEPROYECTOS
                            WHERE estado = 1
                            AND EJERCICIO = @ejercicio";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametro = new
                    {
                        ejercicio = $"{ejercicio}"
                    };

                    int valor = await connection.QuerySingleOrDefaultAsync<int>(
                        query, parametro);

                    _logger.LogInformation("Fin de Proceso de validar si la version esta abierta");

                    return valor;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al validar si la version esta abierta" + "||-->" + ex.Message + "<--||");
            }
        }


        #region DESARROLLO DE LA INCIDENCIA CSJ-138

        public async Task<string> Bienprocesado(string codigoBienDetalle, int codigoSolicitud, int CodigoSolicitudObjeto, bool eliminar)
        {
            string completar = "";
            if (eliminar)
            {
                completar = "And  s.codigo_bien_detalle = " + codigoBienDetalle;
            }
            else
            {
                completar = "And  v.codigo_catalogo = " + codigoBienDetalle.ToString();
            }

            string query = @"SELECT CONCAT(v.codigo_catalogo, '-', v.descripcion_catalogo) AS Articulo
            FROM solicitudes_objetos_bienes_detalle s
            JOIN vListaBienesPrioritarios v ON s.numero_bien = v.codigo_catalogo
            WHERE s.codigo_solicitud_objeto = " + CodigoSolicitudObjeto.ToString() + " AND s.codigo_solicitud = " + codigoSolicitud.ToString() + completar;


            string resultado="";
            using (var connection =this._conexion.CreateSqlConnection())

            {
                resultado = await connection.QueryFirstOrDefaultAsync<string>(query);

            }

            return resultado;
        }
        public async Task<int> ActualizarSolicitudObjetoBienesDetalle(SolicitudObjetoBienesDetalleDTO bienDetalle)
        {
            _logger.LogInformation("Inicio de Proceso de Actualizar valores de bienes del objeto de gasto de la solicitud " +
                " Datos enviados {@bienDetalle}", bienDetalle);
            bool eliminar = false;
            int existeDuplicado = await ExisteSolicitudObjetoBienesDetalle(eliminar, bienDetalle.CodigoBienDetalle.ToString(), bienDetalle.CodigoSolicitud, bienDetalle.CodigoSolicitudObjeto, bienDetalle.DescripcionBienesNacional, bienDetalle.NumeroCatalogo, bienDetalle.ejercicio - 1);

            if (existeDuplicado>1 && bienDetalle.CodigoBienDetalle != 0)
            {
                // Si ya existe un registro con el mismo código de bien detalle, no se inserta
                //throw new GeneracionSolicitudesException("Ocurrio un error al de duplicación los datos de Bien para registro solicitudes_objetos_bienes_detalle");
                return -1;
            }
            if (existeDuplicado > 0 && bienDetalle.CodigoBienDetalle == 0)
            {
                // Si ya existe un registro con el mismo código de bien detalle, no se inserta
                //throw new GeneracionSolicitudesException("Ocurrio un error al de duplicación los datos de Bien para registro solicitudes_objetos_bienes_detalle");
                return -2;
            }
            else
            {
                string query = @"
                        UPDATE solicitudes_objetos_bienes_detalle
                            SET 
                            codigo_bien_detalle = @CodigoBienDetalle,
                            numero_bien = @NumeroCatalogo,
                            descripcion = @DescripcionBienesNacional,
                            costo_unitario = @CostoUnitario,
                            cantidad = @Cantidad,
                            usuario_inserto = @UsuarioInserto,
                            fecha_inserto = @FechaInserto,
                            usuario_ultima_modificacion = @UsuarioUltimaModificacion,
                            fecha_ultima_modificacion = GETDATE(),
                            codigo_solicitud_objeto=@CodigoSolicitudObjeto,
                            codigo_solicitud = @CodigoSolicitud,
                            fundamentacion = @Fundamentacion 
                            WHERE codigo_bien_detalle = @CodigoBienDetalle 
                            and codigo_solicitud_objeto = @CodigoSolicitudObjeto 
                            and codigo_solicitud = @CodigoSolicitud";
                try
                {
                    using (var connection = this._conexion.CreateSqlConnection())
                    {                    

                        bienDetalle.UsuarioUltimaModificacion = bienDetalle.UsuarioInserto;

                        var resultado = await connection.ExecuteAsync(query, bienDetalle);

                        _logger.LogInformation("Fin de Proceso de Actualizar valores de bienes en solicitudes_objetos_bienes_detalle" +
                             " Datos enviados {@usuario}", bienDetalle);

                        return resultado;
                    }
                }
                catch (Exception ex)
                {
                    throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de bienes solicitudes_objetos_bienes_detalle");
                }

            }
        }
        public async Task<int> CrearSolicitudObjetosBienesDetalle(SolicitudObjetoBienesDetalleDTO bienDetalle)
        {
            _logger.LogInformation("Inicio de Proceso de insertar solicitud objetos bienes detalle {@solicitud}", bienDetalle);
            Boolean eliminar = false;
            // Validar si ya existe un registro con el mismo código de bien detalle
            int existeDuplicado = await ExisteSolicitudObjetoBienesDetalle(eliminar, bienDetalle.CodigoBienDetalle.ToString(), bienDetalle.CodigoSolicitud, bienDetalle.CodigoSolicitudObjeto, bienDetalle.DescripcionBienesNacional, bienDetalle.NumeroCatalogo, bienDetalle.ejercicio-1);
            var Cantidad = 0;
            if (existeDuplicado> 0 && bienDetalle.CodigoBienDetalle != 0)
            {
                // Si ya existe un registro con el mismo código de bien detalle, no se inserta
                //throw new GeneracionSolicitudesException(" Registro ya existe en la tabla, no se puede crear en solicitudes_objetos_bienes_detalle");
                Cantidad = -1;
            }
            else if
                (existeDuplicado==0 && bienDetalle.NumeroCatalogo != "" && bienDetalle.CodigoBienDetalle == 0)
            {
                string querySolicitudObjeto = @"SELECT COUNT(*) FROM  solicitudes_objetos_detalle where codigo_solicitud_objeto = @CodigoSolicitudObjeto and codigo_solicitud = @CodigoSolicitud";
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametrosCantidad = new
                    {
                        CodigoSolicitud = bienDetalle.CodigoSolicitud,
                        CodigoSolicitudObjeto = bienDetalle.CodigoSolicitudObjeto
                    };

                    Cantidad = await connection.QueryFirstOrDefaultAsync<int>(querySolicitudObjeto, parametrosCantidad);

                    if (Cantidad != 0)
                    {                   
                        string QueryDescripcion = " SELECT SUM(total_count) AS total_sum FROM(SELECT  count(vlb.codigo_catalogo) as total_count" +
                            " from vListaBienesPrioritarios vlb" +
                            " where vlb.origen = 'BIEN'  " +
                            " and vlb.ejercicio = @ejercicio" +
                            " And vlb.activo = 1  And vlb.codigo_objeto_gasto = @NumeroBienesNacional" +
                            " UNION ALL  SELECT COUNT(vlb.codigo_catalogo) AS total_count  " +
                            " from vListaBienesPrioritarios vlb   " +
                            " where vlb.origen = 'SERVICIO' and vlb.activo = 1" +
                            " And vlb.codigo_objeto_gasto = @NumeroBienesNacional) AS counts";
                    
                        var parametrosDescripcion = new
                        {
                            DescripcionBienesNacional = bienDetalle.DescripcionBienesNacional,
                            NumeroBienesNacional = bienDetalle.CodigoObjetoGasto,
                            ejercicio = bienDetalle.ejercicio - 1
                        };

                        Cantidad = await connection.QueryFirstOrDefaultAsync<int>(QueryDescripcion, parametrosDescripcion);
                    }

                    if (Cantidad != 0)
                    {
                        Cantidad = 1;
                        string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad WHERE cedula_identidad =@UsuarioInserto";

                        string queryUltimoValorCodigo = @"SELECT ISNULL(MAX(codigo_bien_detalle),0) FROM solicitudes_objetos_bienes_detalle where codigo_solicitud_objeto = @CodigoSolicitudObjeto and codigo_solicitud = @CodigoSolicitud ";


                        string query = @"
                            INSERT INTO [solicitudes_objetos_bienes_detalle] 
                            (codigo_bien_detalle, numero_bien, descripcion, costo_unitario, cantidad, usuario_inserto,fecha_inserto, codigo_solicitud_objeto,codigo_solicitud, fundamentacion)
                            VALUES
                            (@CodigoBienDetalle, @NumeroCatalogo, @DescripcionBienesNacional, @CostoUnitario, @Cantidad, @UsuarioInserto, GETDATE(), @CodigoSolicitudObjeto, @CodigoSolicitud, @Fundamentacion)";

                        try
                        {

                            var parametrosBien = new
                            {
                                CodigoSolicitud = bienDetalle.CodigoSolicitud,
                                CodigoSolicitudObjeto = bienDetalle.CodigoSolicitudObjeto
                            };
                           
                            int ultimoValorCodigodetalle = await connection.ExecuteScalarAsync<int>(queryUltimoValorCodigo, parametrosBien);
                            int nuevoCodigoCodigodetalle = ultimoValorCodigodetalle + 1;

                            bienDetalle.CodigoBienDetalle = nuevoCodigoCodigodetalle;                         
                            var resultado = await connection.ExecuteAsync(query, bienDetalle);

                            _logger.LogInformation("fin de Proceso de insertar solicitud objetos detalle {@solicitud}", bienDetalle);

                            Cantidad = resultado;


                        }
                        catch (Exception ex)
                        {
                            Cantidad = -2;// throw new GeneracionSolicitudesException("Ocurrio un error al insertar los datos en la tabla Solicitud Objetos Bienes Detalle");
                        }
                    }
                }
            }
            else
            {
                // Si ya existe un registro con el mismo código de bien detalle, no se inserta
                //throw new GeneracionSolicitudesException(" Registro ya existe en la tabla, no se puede crear en solicitudes_objetos_bienes_detalle");

                Cantidad = -1;
            }


            return Cantidad;
        }

        public async Task<int> ExisteSolicitudObjetoBienesDetalle(Boolean eliminar, string codigoBienDetalle, int codigoSolicitud, int codigoObjetoGasto, string descripcionBienesNacional, string NumeroBienesNacional, int ejercicio)
        {
            //ejercicio = ejercicio - 1;
            string QueryBien = "SELECT COUNT(*) FROM solicitudes_objetos_bienes_detalle s join vListaBienesPrioritarios v on (s.numero_bien= v.codigo_catalogo)" +
                               " WHERE s.numero_bien= v.codigo_catalogo ";
            string QueryBienDoble = "SELECT COUNT(*) FROM solicitudes_objetos_bienes_detalle s join vListaBienesPrioritarios v on (s.numero_bien= v.codigo_catalogo)" +
                           " WHERE s.numero_bien= v.codigo_catalogo ";
            

            if (descripcionBienesNacional != "")
            {
                QueryBien += " and (v.descripcion_catalogo = @DescripcionBienesNacional ) ";

            }
            

            if (NumeroBienesNacional != "")
            {
                QueryBien += " and (v.codigo_catalogo = @NumeroBienesNacional)";

            }
            if (codigoObjetoGasto != 0)
            {
                QueryBien += "and codigo_solicitud_objeto = @CodigoSolicitudObjeto ";
            }

            if (codigoSolicitud != 0)
            {
                QueryBien += "and codigo_solicitud = @CodigoSolicitud";
            }

            QueryBienDoble = QueryBien;

                if (codigoBienDetalle != "0")
            {
                QueryBien += " and (s.codigo_bien_detalle = @CodigoBienDetalle) ";
            }
            if (ejercicio > 0)
               
            {
                QueryBienDoble += " and v.ejercicio in (" + ejercicio + ", 0)";
                QueryBien += " and v.ejercicio in (" + ejercicio + ", 0)";

            }
            int validar = 0;

            try
            {
                if (codigoBienDetalle == "0" && !eliminar && descripcionBienesNacional == "")
                {
                    QueryBien = "SELECT COUNT(*) FROM  solicitudes_objetos_bienes_detalle where codigo_solicitud_objeto = @CodigoSolicitudObjeto " +
                        "and codigo_solicitud = @CodigoSolicitud ";

                    if (ejercicio > 0)
                    {
                        QueryBien += " and v.ejercicio in (" + ejercicio+ ", 0)";

                        QueryBienDoble += " and v.ejercicio in (" + ejercicio + ", 0)";
                    }
                    using (var connection = this._conexion.CreateSqlConnection())

                    {
                        var parametrosBien = new
                        {
                            CodigoSolicitud = codigoSolicitud,
                            CodigoSolicitudObjeto = codigoObjetoGasto
                        };

                        int count = await connection.ExecuteScalarAsync<int>(QueryBien, parametrosBien);
                        validar = count;

                    }
                }
                else if (eliminar)
                {
                    QueryBien = "SELECT COUNT(*) FROM  solicitudes_objetos_bienes_detalle where codigo_solicitud_objeto = @CodigoSolicitudObjeto " +
                        "and codigo_bien_detalle=@CodigoBienDetalle and codigo_solicitud = @CodigoSolicitud ";

                    if (ejercicio != 0)
                    {
                        QueryBien += " and v.ejercicio in (" + ejercicio+ ", 0)";
                        QueryBienDoble += " and v.ejercicio in (" + ejercicio + ", 0)";
                    }
                    using (var connection = this._conexion.CreateSqlConnection())

                    {
                        var parametrosBien = new
                        {
                            CodigoSolicitud = codigoSolicitud,
                            CodigoSolicitudObjeto = codigoObjetoGasto,
                            CodigoBienDetalle = codigoBienDetalle
                        };

                        int count = await connection.ExecuteScalarAsync<int>(QueryBien, parametrosBien);
                        validar = count;

                    }
                }
                else if (!eliminar /*&& codigoBienDetalle!="0"*/ && descripcionBienesNacional != "")
                {

                    using (var connection = this._conexion.CreateSqlConnection())

                    {
                        if (codigoBienDetalle == "0")
                        {
                            var parametrosBien = new
                            {
                                CodigoSolicitud = codigoSolicitud,
                                DescripcionBienesNacional = descripcionBienesNacional,
                                NumeroBienesNacional = NumeroBienesNacional,
                                CodigoSolicitudObjeto = codigoObjetoGasto
                            };
                            int count = await connection.ExecuteScalarAsync<int>(QueryBien, parametrosBien);

                            validar = count;
                        }
                        else
                        {
                            var parametrosBien = new
                            {
                                CodigoSolicitud = codigoSolicitud,
                                CodigoBienDetalle = codigoBienDetalle,
                                DescripcionBienesNacional = descripcionBienesNacional,
                                NumeroBienesNacional = NumeroBienesNacional,
                                CodigoSolicitudObjeto = codigoObjetoGasto
                            };
                            int count = await connection.ExecuteScalarAsync<int>(QueryBien, parametrosBien);

                            var parametrosBienDoble = new
                            {
                                CodigoSolicitud = codigoSolicitud,
                                DescripcionBienesNacional = descripcionBienesNacional,
                                NumeroBienesNacional = NumeroBienesNacional,
                                CodigoSolicitudObjeto = codigoObjetoGasto
                            };

                            count = await connection.ExecuteScalarAsync<int>(QueryBienDoble, parametrosBienDoble);
                            //if (codigoBienDetalle == "0")
                            //{
                                
                            //}
                            validar = count;

                        }

                    }
                }


                return validar;

            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error al Obtener los datos de Bien para registro");
            }

        }

        public async Task<int> EliminarSolicitudObjetosBienesDetalle(int codigoBienDetalle, int codigoSolicitud, int codigoObjetoGasto, string descripcionBienesNacional, int ejercicio)
        {
            _logger.LogInformation("Inicio de Proceso de Eliminar valores de bienes del Poder Juducial");
            Boolean eliminar = true;

            int existeDuplicado = await ExisteSolicitudObjetoBienesDetalle(eliminar, codigoBienDetalle.ToString(), codigoSolicitud, codigoObjetoGasto, descripcionBienesNacional, "", ejercicio);
            if (existeDuplicado==0)
            {
                // Si ya existe un registro con el mismo código de bien detalle, no se inserta
                //throw new GeneracionSolicitudesException(" Error de bien inexistente en el proceso de eliminación de bien de solicitudes_objetos_bienes_detalle");
                return -1;
            }

            else
            {

                string query = @"delete solicitudes_objetos_bienes_detalle                        
                             WHERE codigo_bien_detalle = @CodigoBienDetalle and codigo_solicitud_objeto = @CodigoSolicitudObjeto and codigo_solicitud = @CodigoSolicitud";
                try
                {
                    using (var connection = this._conexion.CreateSqlConnection())

                    {
                        var parametros = new
                        {
                            CodigoSolicitud = codigoSolicitud,
                            CodigoBienDetalle = codigoBienDetalle,
                            CodigoSolicitudObjeto = codigoObjetoGasto
                        };

                        _logger.LogInformation("Fin de Proceso eliminar bien de solicitudes_objetos_bienes_detalle");

                        return await connection.ExecuteAsync(query, parametros);



                    }
                }
                catch (Exception ex)
                {
                    return -2;//throw new GeneracionSolicitudesException("Ocurrio un error al eliminar bien de solicitudes_objetos_bienes_detalle");
                }
            }
        }

        public async Task<List<SolicitudObjetoBienesDetalleDTO>> ObtenerDatosSolicitudObjetoBienesDetallePorCodigoSolicitud(int codigo, int pagina, int cantidadRegistros)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener valores de Bienes de solicitudes_objetos_bienes_detalle ");

            int saltarRegistros = (pagina - 1) * cantidadRegistros;
            string query = @" SELECT COUNT(*) OVER () as TotalRegistros                              
                             ,d.codigo_bien_detalle as CodigoBienDetalle
                             ,v.codigo_catalogo as NumeroBienesNacional
                             ,v.descripcion_catalogo as DescripcionBienesNacional
                             ,d.costo_unitario as CostoUnitario
                             ,d.cantidad as Cantidad
                             ,d.codigo_solicitud_objeto as CodigoSolicitudObjeto
					         ,d.usuario_inserto as UsuarioInserto
                             ,d.fecha_inserto as FechaInserto
                             ,d.codigo_solicitud as CodigoSolicitud
                             , d.cantidad * d.costo_unitario as MontoTotal

                  FROM solicitudes_objetos_bienes_detalle  d 
                  join vListaBienesPrioritarios v on(v.codigo_catalogo=d.numero_bien) 
                  WHERE codigo_solicitud_objeto= @Codigo
                            ORDER BY d.codigo_bien_detalle
                            OFFSET @saltarRegistros ROWS
                            FETCH NEXT @cantidadRegistros ROWS ONLY";

            ;
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var resultado = await connection.QueryAsync<SolicitudObjetoBienesDetalleDTO>(query, new { Codigo = codigo, pagina, cantidadRegistros, saltarRegistros });
                    if (resultado == null)
                    {
                        _logger.LogInformation("Fin de Proceso de Obtener valores de Bienes de solicitudes_objetos_bienes_detalle");
                        return null;
                    }
                    _logger.LogInformation("Fin de Proceso de Obtener valores de Bienes de solicitudes_objetos_bienes_detalle");

                    return resultado.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al Obtener los datos de Bienes de solicitudes_objetos_bienes_detalle");
            }
        }

        public async Task<List<SolicitudObjetoBienesDetalleDTO>> ListadoBienesPorCriterioBusqueda(int ejercicio, int codigoSolicitud, int codigoSolicitudObjeto, int codigo, string descripcionSolicitudObjetoBienesDetalle, int pagina, int cantidadRegistros)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener solicitudes de bienes");

            int saltarRegistros = (pagina - 1) * cantidadRegistros;

            string query = @"SELECT COUNT(*) OVER () as TotalRegistros 
                             ,d.codigo_bien_detalle as CodigoBienDetalle
                             ,v.codigo_catalogo as NumeroCatalogo
                             ,v.descripcion_catalogo as DescripcionBienesNacional
                             ,d.costo_unitario as CostoUnitario
                             ,d.cantidad as Cantidad
                             ,d.codigo_solicitud_objeto as CodigoSolicitudObjeto
					         ,d.usuario_inserto as UsuarioInserto
                             ,d.fecha_inserto as FechaInserto
                             ,d.codigo_solicitud as CodigoSolicitud
                             ,d.fundamentacion as Fundamentacion
                             , d.cantidad * d.costo_unitario as MontoTotal
                             ,d.usuario_ultima_modificacion as UsuarioUltimaModificacion
                             ,d.fecha_ultima_modificacion as FechaUltimaModificacion
                             ,v.ejercicio as ejercicio

                  FROM solicitudes_objetos_bienes_detalle  d 
                  join vListaBienesPrioritarios v on(v.codigo_catalogo=d.numero_bien)
                              join  solicitudes_bienes_circunscripcion s on (s.codigo_solicitud=d.codigo_solicitud)
                   WHERE d.codigo_solicitud =@CodigoSolicitud   and d.codigo_solicitud_objeto=@codigoSolicitudObjeto
                   and @ejercicio= v.ejercicio ";

            if (descripcionSolicitudObjetoBienesDetalle != "")
            {
                query += "and UPPER(v.descripcion_catalogo) LIKE '%' + @desc + '%'";
            }
            if (codigo != 0)
            {
                query += "and UPPER(v.codigo_catalogo) LIKE '%' + @cod + '%' ";
            }

            query += " ORDER BY  v.codigo_catalogo  OFFSET @saltarRegistros ROWS  FETCH NEXT @cantidadRegistros ROWS ONLY";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string vacio = "";
                    if (codigo == 0)
                    {
                        vacio = "";
                    }
                    else
                    {
                        vacio = codigo.ToString();
                    }
                    string descripcion = "";
                    if (descripcionSolicitudObjetoBienesDetalle == "" || descripcionSolicitudObjetoBienesDetalle == null)
                    {
                        descripcion = "";
                    }
                    else
                    {
                        descripcion = $"%{descripcionSolicitudObjetoBienesDetalle.ToUpper()}%";
                    }

                    var parametro = new
                    {
                        cod = vacio,
                        desc = descripcion,
                        pagina,
                        saltarRegistros = saltarRegistros,
                        cantidadRegistros = cantidadRegistros,
                        ejercicio = ejercicio,
                        codigoSolicitudObjeto = codigoSolicitudObjeto,
                        CodigoSolicitud = codigoSolicitud
                    };

                    var resultado = await connection.QueryAsync<SolicitudObjetoBienesDetalleDTO>(query, parametro);
                    _logger.LogInformation("Fin de Proceso de obtener los bienes de la solicitud {@descrip}", descripcionSolicitudObjetoBienesDetalle);

                    if (resultado == null)
                    {
                        _logger.LogInformation("Fin de Proceso de obtener los bienes de la solicitud");
                        return null;
                    }
                    _logger.LogInformation("Fin de Proceso de Obtener obtener los bienes de la solicitud");

                    return resultado.ToList();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error inesperado.");
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener los bienes de la solicitud");
            }
        }

        // Listar bienes por criterio con filtros
        public async Task<Datos<IEnumerable<SolicitudObjetoBienesDetalleDTO>>> ListadoBienesPorCriterioBusquedaPorFiltros(SolicitudObjetoBienesDetalleRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de Buscar Expresiones en la tabla solicitudes_objetos_bienes_detalle");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
            var query = string.Empty;

            try
            {
                query = @"SELECT d.codigo_bien_detalle as CodigoBienDetalle                             
                             ,v.codigo_catalogo as NumeroCatalogo
                             ,v.descripcion_catalogo as DescripcionBienesNacional
                             ,d.costo_unitario as CostoUnitario
                             ,d.cantidad as Cantidad
                             ,d.codigo_solicitud_objeto as CodigoSolicitudObjeto
					         ,d.usuario_inserto as UsuarioInserto
                             ,d.fecha_inserto as FechaInserto
                             ,d.codigo_solicitud as CodigoSolicitud
                             ,d.fundamentacion as Fundamentacion
                             , d.cantidad * d.costo_unitario as MontoTotal
                             , d.costo_unitario as Monto
                             ,d.usuario_ultima_modificacion as UsuarioUltimaModificacion
                             ,d.fecha_ultima_modificacion as FechaUltimaModificacion,
                              v.ejercicio as ejercicio

                  FROM solicitudes_objetos_bienes_detalle  d
                  join vListaBienesPrioritarios v on(v.codigo_catalogo = d.numero_bien) where v.ejercicio in (@ejercicio,0) and d.codigo_solicitud = @parametroNroSolicitud and d.codigo_solicitud_objeto =@parametroCodigoSolicitudObjeto";


                if (request.CodigoBienDetalle != "0" ||
                    request.CodigoSolicitud.ToString() != "0" ||
                    !string.IsNullOrEmpty(request.NumeroBien) ||
                    !string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                    !string.IsNullOrEmpty(request.DescripcionBienesNacional) ||
                    (request.Cantidad != "0") ||
                    !string.IsNullOrEmpty(request.Fundamentacion) ||
                    (request.Monto != 0) ||
                     (request.MontoTotal != "0")
                    )
                {
                    // Se proporcionaron parámetros de búsqueda, agregar filtros adicionales
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        query += @"
                                   AND (
                                        d.numero_bien LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR v.descripcion_bien LIKE '%' + @terminoDeBusqueda + '%'                                        
                                        OR CONVERT(NVARCHAR(MAX), d.fecha_inserto, 126) LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.codigo_catalogo LIKE '%' + @terminoDeBusqueda + '%'  
                                        OR d.codigo_bien_detalle LIKE '%' + @terminoDeBusqueda + '%'
                                        OR d.fundamentacion LIKE '%' + @terminoDeBusqueda + '%'
                                        OR d.codigo_solicitud_objeto LIKE '%' + @terminoDeBusqueda + '%'
                                        OR d.cantidad LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR d.costo_unitario LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR d.cantidad * d.costo_unitario LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'

                                    )";
                    }
                    else
                    {
                        query += @"";
                        if (request.CodigoSolicitud != 0 && request.CodigoSolicitud != null)
                        { query += " and d.codigo_solicitud = @parametroNroSolicitud"; }
                        if (request.CodigoSolicitudObjeto != 0 && request.CodigoSolicitudObjeto != null)
                        { query += " and d.codigo_solicitud_objeto =@parametroCodigoSolicitudObjeto"; }

                        if (request.CodigoBienDetalle != "0" && request.CodigoBienDetalle != null)
                        { query += " and d.codigo_bien_detalle  LIKE '%' + @parametroCodigoBienDetalle  + '%'"; }
                        if (request.NumeroCatalogo != "0" && request.NumeroCatalogo != null)
                        { query += " and v.codigo_catalogo LIKE '%' + @parametroCodigoCatalogo + '%'"; }
                        if (request.DescripcionBienesNacional != "" && request.DescripcionBienesNacional != null)
                        { query += " and v.descripcion_bien LIKE '%' + @parametroDescripcion + '%'"; }
                        if (request.Fundamentacion != "" && request.Fundamentacion != null)
                        { query += " and d.fundamentacion LIKE '%' + @fundamentacion + '%'"; }
                        if (request.Cantidad != "0" && request.Cantidad != null)
                        { query += " and d.cantidad LIKE '%' + REPLACE(@cantidad, '.', '') + '%'"; }
                        if (request.MontoTotal != "0" && request.MontoTotal != null)
                        { query += " and d.cantidad * d.costo_unitario   LIKE '%' + REPLACE(@parametroMontoTotal, '.', '') + '%'"; }
                        if (request.ValorUnitario != "0" && request.ValorUnitario != null)
                        { query += " and v.valor_unitario LIKE '%' + REPLACE(@parametroValorUnitario, '.', '') + '%'"; }
                        if (request.CostoUnitario != "0" && request.CostoUnitario != null)
                        { query += " and d.costo_unitario LIKE '%' +REPLACE(@parametroMonto, '.', '') + '%'"; }
                    }
                }


                query += @" ORDER BY d.codigo_bien_detalle";
                query += @" OFFSET @saltarRegistros ROWS";
                query += @" FETCH NEXT @cantidadRegistros ROWS ONLY";


                string queryCantidadTotalREgistros = @" SELECT COUNT(v.numero_bien)                           
                   FROM solicitudes_objetos_bienes_detalle  d                   
                   join vListaBienesPrioritarios v on(v.codigo_catalogo=d.numero_bien)
                   join  solicitudes_bienes_circunscripcion s on (s.codigo_solicitud=d.codigo_solicitud)
                   WHERE  v.ejercicio in (@ejercicio,0) and d.codigo_solicitud = @parametroNroSolicitud and d.codigo_solicitud_objeto =@parametroCodigoSolicitudObjeto";


                if (request.CodigoBienDetalle != "0" ||
                    request.CodigoSolicitud.ToString() != "0" ||
                    !string.IsNullOrEmpty(request.NumeroBien) ||
                    !string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                    !string.IsNullOrEmpty(request.DescripcionBienesNacional) ||
                    (request.Cantidad != "0") ||
                    !string.IsNullOrEmpty(request.Fundamentacion) ||
                    (request.Monto != 0) ||
                     (request.MontoTotal != "0")
                    )
                {
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        queryCantidadTotalREgistros += @"
                                    AND (
                                        CONVERT(NVARCHAR(MAX), s.numero_solicitud) LIKE '%' + @terminoDeBusqueda + '%'
                                        OR d.numero_bien LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR v.descripcion_bien LIKE '%' + @terminoDeBusqueda + '%'                                        
                                        OR CONVERT(NVARCHAR(MAX), d.fecha_inserto, 126) LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.codigo_catalogo LIKE '%' + @terminoDeBusqueda + '%'  
                                        OR d.codigo_bien_detalle LIKE '%' + @terminoDeBusqueda + '%'
                                        OR d.codigo_solicitud_objeto LIKE '%' + @terminoDeBusqueda + '%'
                                        OR d.fundamentacion LIKE '%' + @terminoDeBusqueda + '%'
                                        OR d.cantidad LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR d.costo_unitario LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR d.cantidad * d.costo_unitario LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                    )";
                    }
                    else
                    {
                        queryCantidadTotalREgistros += @"";
                        if (request.CodigoSolicitud != 0 && request.CodigoSolicitud != null)
                        { queryCantidadTotalREgistros += " and d.codigo_solicitud = @parametroNroSolicitud"; }
                        if (request.CodigoSolicitudObjeto != 0 && request.CodigoSolicitudObjeto != null)
                        { queryCantidadTotalREgistros += " and d.codigo_solicitud_objeto =@parametroCodigoSolicitudObjeto"; }
                        if (request.CodigoBienDetalle != "0" && request.CodigoBienDetalle != null)
                        { queryCantidadTotalREgistros += " and d.codigo_bien_detalle LIKE '%' + @parametroCodigoBienDetalle  + '%'"; }
                        if (request.NumeroCatalogo != "0" && request.NumeroCatalogo != null)
                        { queryCantidadTotalREgistros += " and v.codigo_catalogo LIKE '%' + @parametroCodigoCatalogo + '%'"; }

                        if (request.DescripcionBienesNacional != "" && request.DescripcionBienesNacional != null)
                        { queryCantidadTotalREgistros += " and v.descripcion_bien LIKE '%' + @parametroDescripcion + '%'"; }
                        if (request.Fundamentacion != "" && request.Fundamentacion != null)
                        { queryCantidadTotalREgistros += " and d.fundamentacion LIKE '%' + @fundamentacion + '%'"; }
                        if (request.Cantidad != "0" && request.Cantidad != null)
                        { queryCantidadTotalREgistros += " and d.cantidad LIKE '%' + REPLACE(@cantidad, '.', '') + '%'"; }
                        if (request.MontoTotal != "0" && request.MontoTotal != null)
                        { queryCantidadTotalREgistros += " and d.cantidad * d.costo_unitario   LIKE '%' + REPLACE(@parametroMontoTotal, '.', '') + '%'"; }
                        if (request.ValorUnitario != "0" && request.ValorUnitario != null)
                        { queryCantidadTotalREgistros += " and v.valor_unitario LIKE '%' + REPLACE(@parametroValorUnitario, '.', '') + '%'"; }
                        if (request.CostoUnitario != "0" && request.CostoUnitario != null)
                        { queryCantidadTotalREgistros += " and d.costo_unitario LIKE '%' +REPLACE(@parametroMonto, '.', '') + '%'"; }


                    }
                }


                // Definición de parámetros
                var parametros = new DynamicParameters();
                parametros.Add("@terminoDeBusqueda", $"%{request.TerminoDeBusqueda}%");
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.CantidadRegistros);

                // Añadir los parámetros correspondientes de búsqueda
                parametros.Add("@parametroNroSolicitud", request.CodigoSolicitud.ToString());
                parametros.Add("@parametroCodigoBienDetalle", request.CodigoBienDetalle);
                parametros.Add("@parametroDescripcion", request.DescripcionBienesNacional);
                parametros.Add("@parametroUsuario", request.UsuarioInserto);
                parametros.Add("@ejercicio", request.Ejercicio);
                parametros.Add("@fundamentacion", request.Fundamentacion);
                parametros.Add("@parametroMontoTotal", request.MontoTotal);
                parametros.Add("@parametroMonto", request.CostoUnitario);
                parametros.Add("@cantidad", request.Cantidad);
                parametros.Add("@parametroCodigoSolicitudObjeto", request.CodigoSolicitudObjeto.ToString());
                parametros.Add("@parametroCodigoCatalogo", request.NumeroCatalogo);
                parametros.Add("@parametroValorUnitario", request.ValorUnitario);


                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);

                    var resultado = await connection.QueryAsync<SolicitudObjetoBienesDetalleDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<SolicitudObjetoBienesDetalleDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    _logger.LogInformation("Fin de Proceso de Búsqueda de Solicitudes de Bienes por Criterio de Búsqueda");
                    return listado;
                }
            }

            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al buscar datos en la tabla solicitudes_objetos_bienes_detalle" + "||-->" + ex.Message + "<--||");
            }
        }

        public async Task<Datos<IEnumerable<SolicitudObjetoCodigoDescripcionDTO>>> ListadoBienesPorCodigoDescripcion(SolicitudObjetoBienesDetalleRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de Buscar Expresiones en la tabla solicitudes_objetos_bienes_detalle");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
            var query = string.Empty;

            try
            {
                query = @" SELECT  v.codigo_catalogo as NumeroCatalogo
                             ,v.descripcion_catalogo as DescripcionBienesNacional 
                             ,v.valor_unitario as costoUnitario 

                  FROM  vListaBienesPrioritarios v  where v.ejercicio in (@ejercicio,0) and v.codigo_objeto_gasto =@parametroCodigoObjetoGasto ";


                if (request.CodigoBienDetalle != "0" ||
                    request.CodigoSolicitud.ToString() != "0" ||
                    !string.IsNullOrEmpty(request.NumeroBien) ||
                    !string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                    !string.IsNullOrEmpty(request.DescripcionBienesNacional) ||
                    (request.Cantidad != "0") ||
                    !string.IsNullOrEmpty(request.Fundamentacion) ||
                    (request.Monto != 0) ||
                     (request.MontoTotal != "0")
                    )
                {
                    // Se proporcionaron parámetros de búsqueda, agregar filtros adicionales
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        query += @"
                                   AND (
                                        v.codigo_catalogo LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR v.descripcion_catalogo LIKE '%' + @terminoDeBusqueda + '%' OR v.valor_unitario LIKE '%' + @terminoDeBusqueda + '%'  
                                        OR v.codigo_objeto_gasto LIKE '%' + @terminoDeBusqueda + '%' 
                                        or v.valor_unitario LIKE '%' + @terminoDeBusqueda + '%' 
                                    )";
                    }
                    else
                    {
                        query += @"";             

                        if (request.CodigoObjetoGasto != 0 && request.CodigoObjetoGasto != null)
                        { query += " and v.codigo_objeto_gasto =@parametroCodigoObjetoGasto"; }
                       
                        if (request.NumeroCatalogo != "0" && request.NumeroCatalogo != null)
                        { query += " and v.codigo_catalogo LIKE '%' + @parametroCodigoCatalogo + '%'"; }
                        if (request.DescripcionBienesNacional != "" && request.DescripcionBienesNacional != null)
                        { query += " and v.descripcion_bien LIKE '%' + @parametroDescripcion + '%'"; }
                        
                        if (request.ValorUnitario != "0" && request.ValorUnitario!= null)
                        { query += " and v.valor_unitario LIKE '%' + @parametroValorUnitario + '%'"; }
                     
                    }
                }


                query += @" ORDER BY v.codigo_catalogo";
                query += @" OFFSET @saltarRegistros ROWS";
                query += @" FETCH NEXT @cantidadRegistros ROWS ONLY";


                string queryCantidadTotalREgistros = @" SELECT COUNT(v.codigo_catalogo)                           
                   
                  FROM  vListaBienesPrioritarios v  where v.ejercicio in (@ejercicio,0) ";


                if (request.CodigoBienDetalle != "0" ||
                    request.CodigoSolicitud.ToString() != "0" ||
                    !string.IsNullOrEmpty(request.NumeroBien) ||
                    !string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                    !string.IsNullOrEmpty(request.DescripcionBienesNacional) ||
                    (request.Cantidad != "0") ||
                    !string.IsNullOrEmpty(request.Fundamentacion) ||
                    (request.Monto != 0) ||
                     (request.MontoTotal != "0")
                    )
                {
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        queryCantidadTotalREgistros += @"
                                   AND (
                                        v.codigo_catalogo LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR v.descripcion_catalogo LIKE '%' + @terminoDeBusqueda + '%' OR v.valor_unitario LIKE '%' + @terminoDeBusqueda + '%'  
                                        OR v.codigo_objeto_gasto LIKE '%' + @terminoDeBusqueda + '%' 
                                        or v.valor_unitario LIKE '%' + @terminoDeBusqueda + '%' 
                                    )";
                    }
                    else
                    {
                        queryCantidadTotalREgistros += @"";

                        if (request.CodigoObjetoGasto != 0 && request.CodigoObjetoGasto != null)
                        { queryCantidadTotalREgistros += " and v.codigo_objeto_gasto =@parametroCodigoObjetoGasto"; }

                        if (request.NumeroCatalogo != "0 "&& request.NumeroCatalogo != null)
                        { queryCantidadTotalREgistros += " and v.codigo_catalogo LIKE '%' + @parametroCodigoCatalogo + '%'"; }
                        if (request.DescripcionBienesNacional != "" && request.DescripcionBienesNacional != null)
                        { queryCantidadTotalREgistros += " and v.descripcion_bien LIKE '%' + @parametroDescripcion + '%'"; }

                        if (request.ValorUnitario != "0" && request.ValorUnitario != null)
                        { queryCantidadTotalREgistros += " and v.valor_unitario LIKE '%' + @parametroValorUnitario + '%'"; }
                    }
                }


                // Definición de parámetros
                var parametros = new DynamicParameters();
                parametros.Add("@terminoDeBusqueda", $"%{request.TerminoDeBusqueda}%");
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.CantidadRegistros);

                // Añadir los parámetros correspondientes de búsqueda
                parametros.Add("@parametroNroSolicitud", request.CodigoSolicitud);
                parametros.Add("@parametroCodigoBienDetalle", request.CodigoBienDetalle);
                parametros.Add("@parametroDescripcion", request.DescripcionBienesNacional);
                parametros.Add("@parametroUsuario", request.UsuarioInserto);
                parametros.Add("@ejercicio", request.Ejercicio);
                parametros.Add("@fundamentacion", request.Fundamentacion);
                parametros.Add("@parametroMontoTotal", request.MontoTotal);
                parametros.Add("@parametroValorUnitario", request.ValorUnitario);
                parametros.Add("@cantidad", request.Cantidad);
                parametros.Add("@parametroCodigoSolicitudObjeto", request.CodigoSolicitudObjeto.ToString());
                parametros.Add("@parametroCodigoObjetoGasto", request.CodigoObjetoGasto.ToString()); 
                parametros.Add("@parametroCodigoCatalogo", request.NumeroCatalogo);
               



                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);

                    var resultado = await connection.QueryAsync<SolicitudObjetoCodigoDescripcionDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<SolicitudObjetoCodigoDescripcionDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    _logger.LogInformation("Fin de Proceso de Búsqueda de Solicitudes de Bienes por Criterio de Búsqueda");
                    return listado;
                }
            }

            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrió un error inesperado al buscar datos en la tabla solicitudes_objetos_bienes_detalle" + "||-->" + ex.Message + "<--||");
            }
        }

      /*  public async Task<List<SolicitudObjetoCodigoDescripcionDTO>> ListadoBienesPorCodigoDescripcion(double costoUnitario, int ejercicio, int codigo, int codigoObjetoGasto, int codigoSolicitudGasto, string descripcionSolicitudObjetoBienesDetalle, int pagina, int cantidadRegistros)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener solicitudes de bienes");

            int saltarRegistros = (pagina - 1) * cantidadRegistros;

            string query = @" SELECT   v.codigo_catalogo as NumeroCatalogo
                             ,v.descripcion_catalogo as DescripcionBienesNacional 
                             ,v.valor_unitario as costoUnitario 
                  FROM  vListaBienesPrioritarios v ";

            if (codigoSolicitudGasto != 0)
            {
                query += " join solicitudes_objetos_detalle g on (g.codigo_objeto_gasto=v.codigo_objeto_gasto ) ";
            }
            query += " WHERE v.ejercicio=@ejercicio ";

            if (costoUnitario != 0)
            {
                query += " and UPPER(v.valor_unitario) LIKE '%' + @costoUnitario + '%' ";

            }
            if (codigo != 0)
            {
                query += " and UPPER(v.codigo_catalogo) LIKE '%' + @cod + '%' ";
            }
            if (descripcionSolicitudObjetoBienesDetalle != "" && descripcionSolicitudObjetoBienesDetalle != null)
            {
                query += " and   UPPER(v.descripcion_catalogo) LIKE '%' + @desc + '%'    ";
            }
            if (codigoObjetoGasto != 0)
            {
                query += " and v.codigo_objeto_gasto= " + codigoObjetoGasto.ToString();
            }

            if (codigoSolicitudGasto != 0)
            {
                query += " and g.codigo_solicitud_objeto = " + codigoSolicitudGasto.ToString();
            }

            query += " UNION ALL SELECT  vlb.codigo_catalogo as NumeroCatalogo ,vlb.descripcion_catalogo as DescripcionBienesNacional, vlb.valor_unitario as costoUnitario " +
                     " from vListaBienesPrioritarios vlb  ";

            if (codigoSolicitudGasto != 0)
            {
                query += " join solicitudes_objetos_detalle g on (g.codigo_objeto_gasto=vlb.codigo_objeto_gasto ) ";
            }
            query += " where vlb.origen='SERVICIO'";

            if (costoUnitario != 0)
            {
                query += " UPPER(vlb.valor_unitario) LIKE '%' + @costoUnitario + '%' ";

            }
            if (codigo != 0)
            {
                query += " and UPPER(vlb.codigo_catalogo) LIKE '%' + @cod + '%' ";
            }
            if (descripcionSolicitudObjetoBienesDetalle != "" && descripcionSolicitudObjetoBienesDetalle != null)
            {
                query += " and   UPPER(vlb.descripcion_catalogo) LIKE '%' + @desc + '%'    ";
            }
            if (codigoObjetoGasto != 0)
            {
                query += " and vlb.codigo_objeto_gasto= " + codigoObjetoGasto.ToString();
            }

            if (codigoSolicitudGasto != 0)
            {
                query += " and g.codigo_solicitud_objeto = " + codigoSolicitudGasto.ToString();
            }




            query += " ORDER BY v.codigo_catalogo OFFSET @saltarRegistros ROWS FETCH NEXT @cantidadRegistros ROWS ONLY";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {

                    var parametro = new
                    {
                        cod = codigo.ToString(),
                        desc = descripcionSolicitudObjetoBienesDetalle,
                        pagina,
                        saltarRegistros = saltarRegistros,
                        ejercicio = ejercicio,
                        cantidadRegistros = cantidadRegistros,
                        costoUnitario = costoUnitario.ToString()
                    };

                    var resultado = await connection.QueryAsync<SolicitudObjetoCodigoDescripcionDTO>(query, parametro);
                    _logger.LogInformation("Fin de Proceso de obtener los bienes de la solicitud {@descrip}", descripcionSolicitudObjetoBienesDetalle);

                    if (resultado == null)
                    {
                        _logger.LogInformation("Fin de Proceso de obtener los bienes de la solicitud");
                        return null;
                    }
                    _logger.LogInformation("Fin de Proceso de Obtener obtener los bienes de la solicitud");

                    return resultado.ToList();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error inesperado.");
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener los bienes de la solicitud");
            }
        }

        */
        public async Task<int> CantidadRegistrosBienes(double costoUnitario, int ejercicio, int codigoObjetoGasto, int codigoSolicitudGasto, int codigo, string descripcionSolicitudObjetoBienesDetalle)
        {
            _logger.LogInformation("Inicio de Proceso de contar Bienes de solicitudes_objetos_bienes_detalle ");


            string query = @"select SUM(total_count) AS total_sum from ( select COUNT(v.codigo_catalogo) as total_count
                           FROM vListaBienesPrioritarios v ";


            if (codigoSolicitudGasto != 0)
            {
                query += " join solicitudes_objetos_detalle g on (g.codigo_objeto_gasto=v.codigo_objeto_gasto ) ";
            }
            query += " WHERE v.ejercicio=@ejercicio and v.activo=1 ";

            if (costoUnitario != 0)
            {
                query += " and UPPER(v.valor_unitario) LIKE '%' + @costoUnitario + '%' ";

            }
            if (codigo != 0)
            {
                query += " and UPPER(v.codigo_catalogo) LIKE '%' + @cod + '%' ";
            }
            if (descripcionSolicitudObjetoBienesDetalle != "" && descripcionSolicitudObjetoBienesDetalle != null)
            {
                query += " and   UPPER(v.descripcion_catalogo) LIKE '%' + @desc + '%'    ";
            }
            if (codigoObjetoGasto != 0)
            {
                query += " and v.codigo_objeto_gasto= " + codigoObjetoGasto.ToString();
            }

            if (codigoSolicitudGasto != 0)
            {
                query += " and g.codigo_solicitud_objeto = " + codigoSolicitudGasto.ToString();
            }

            query += " UNION ALL SELECT COUNT(  vlb.codigo_catalogo) AS total_count " +
                     " from vListaBienesPrioritarios vlb  ";

            if (codigoSolicitudGasto != 0)
            {
                query += " join solicitudes_objetos_detalle g on (g.codigo_objeto_gasto=vlb.codigo_objeto_gasto ) ";
            }
            query += " where vlb.origen='SERVICIO'";

            if (costoUnitario != 0)
            {
                query += " UPPER(vlb.valor_unitario) LIKE '%' + @costoUnitario + '%' ";

            }
            if (codigo != 0)
            {
                query += " and UPPER(vlb.codigo_catalogo) LIKE '%' + @cod + '%' ";
            }
            if (descripcionSolicitudObjetoBienesDetalle != "" && descripcionSolicitudObjetoBienesDetalle != null)
            {
                query += " and   UPPER(vlb.descripcion_catalogo) LIKE '%' + @desc + '%'    ";
            }
            if (codigoObjetoGasto != 0)
            {
                query += " and vlb.codigo_objeto_gasto= " + codigoObjetoGasto.ToString();
            }

            if (codigoSolicitudGasto != 0)
            {
                query += " and g.codigo_solicitud_objeto = " + codigoSolicitudGasto.ToString();
            }

            query += " and vlb.activo=1 ) AS counts";






            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {


                    var parametro = new
                    {
                        cod = codigo.ToString(),
                        desc = descripcionSolicitudObjetoBienesDetalle,
                        ejercicio = ejercicio,

                    };
                    var resultado = await connection.QuerySingleOrDefaultAsync<int>(query, parametro);

                    _logger.LogInformation("Fin de Proceso de Obtener valores de Bienes de solicitudes_objetos_bienes_detalle");

                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al Obtener los datos de Bienes de solicitudes_objetos_bienes_detalle");
            }
        }

        public async Task<int> EjercicioSolicitud()
        {


            string queryFechaCierre = "select c.Ejercicio from version_fecha_cierre c where c.activo=1";
            
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {


                    var Ejercicio = await connection.QueryAsync<int>(queryFechaCierre);

                    _logger.LogInformation("Fin de Proceso de Obtener valores de Bienes de solicitudes_objetos_bienes_detalle");

                    return Ejercicio.First();
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al Obtener los datos del ejercicio activo");
            }
        }

        public async Task<CantidadTotalGenericaDTO> TotalesCatidadBienesSolicitud(int codigoSolicitud, int codigoSolicitudObjeto)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener Cantidades  y Montos Totales");
            // Definir la consulta SQL para obtener los valores
            string query = @"Select Sum(solicitudes_objetos_bienes_detalle.cantidad) as CantidadTotal,  SUM(solicitudes_objetos_bienes_detalle.cantidad * solicitudes_objetos_bienes_detalle.costo_unitario)  as MontoTotal 
                         from solicitudes_objetos_bienes_detalle 
                         WHERE codigo_solicitud= @Codigo and codigo_solicitud_objeto= @codigoSolicitudObjeto";

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryFirstOrDefaultAsync<CantidadTotalGenericaDTO>(query, new { Codigo = codigoSolicitud, codigoSolicitudObjeto = codigoSolicitudObjeto });
                if (resultado == null)
                {
                    _logger.LogInformation("Fin de Proceso de Obtener Cantidades  y Montos Totales");
                    return null;
                }
                _logger.LogInformation("Fin de Proceso de Obtener Cantidades  y Montos Totales");
                return resultado;
            }

        }

        #endregion

    }
}

