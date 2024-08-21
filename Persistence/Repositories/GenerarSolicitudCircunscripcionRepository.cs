using Application.Services.Interfaces.IRepository;
using Dapper;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Entities;
using Domain.Entities.Anteproyecto;
using Domain.Entities.GenerarSolicitudporCircunscripcion;
using Domain.Entities.Request;
using Domain.Entities.Server;
using Domain.Entities.Sincronizacion;
using Domain.Exceptions.ImportarArchivoSIPOIExcepcions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using static Dapper.SqlMapper;

namespace Persistence.Repositories
{
    public class GenerarSolicitudCircunscripcionRepository : IGenerarSolicitudCircunscripcionRepository
    {
        private readonly DbConnections _conexion;
        private readonly ILogger<GenerarSolicitudCircunscripcionRepository> _logger;
        private readonly HttpClient _httpClient;
        private readonly GestionArchivos _gestionArchivos;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GenerarSolicitudCircunscripcionRepository(DbConnections conexion, ILogger<GenerarSolicitudCircunscripcionRepository> logger,
            IHttpClientFactory httpClientFactory, IOptions<GestionArchivos> gestionArchivos, IHttpContextAccessor httpContextAccessor)
        {
            _conexion = conexion;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("GestionArchivosClient");
            _gestionArchivos = gestionArchivos.Value;
            _httpContextAccessor = httpContextAccessor;
        }


        #region DESARROLLO DE LA INCIDENCIA CSJ-152

        public async Task<Datos<IEnumerable<VersionAnteproyectoDTO>>> VersionAnteproyectoRequest(VersionAnteproyectoRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener Versiones Anteproyecto");
            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
            var query = string.Empty;
            try
            {
                query = @"SELECT va.NUMERO_VERSION as NumeroVersion, va.CODIGO_VERSION as CodigoVersion, va.EJERCICIO, va.ESTADO, r.valor_alfanumerico as EstadoDescripcion ,
                    ISNULL(va.FECHA_MODIFICACION, va.FECHA_INSERTO) AS FECHA
                    FROM VERSIONES_ANTEPROYECTOS va
                    join referencias r on va.ESTADO=r.valor_decimal
                    JOIN tipo_referencias tr on r.codigo_tipo_referencia=tr.codigo_tipo_referencia
                    where tr.dominio_tipo_referencia='ESTADO_VERSION'
                    and va.VERSION_CONSOLIDADO = 0 and va.numero_version = (SELECT min (numero_version) FROM VERSIONES_ANTEPROYECTOS  
                    WHERE EJERCICIO = va.ejercicio and version_consolidado=0)";

                //Tratamos de identificar si el dato que viene es una fecha
                DateTime fechaBusqueda;
                string terminoDeBusquedaSQL = string.Empty;
                bool esFechaBusqueda = DateTime.TryParseExact(request.TerminoDeBusqueda, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out fechaBusqueda);
                if (esFechaBusqueda)
                {
                    terminoDeBusquedaSQL = esFechaBusqueda ? fechaBusqueda.ToString("yyyy-MM-dd") : request.TerminoDeBusqueda;
               
                    terminoDeBusquedaSQL = terminoDeBusquedaSQL.Replace(" ", "");
                }
                if (request.CodigoVersion.ToString() != "0" ||
                    request.Ejercicio.ToString() != "0" ||
                    !string.IsNullOrEmpty(request.Estado) ||
                    !string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                    null != request.Fecha)


                {
                    // Se proporcionaron parámetros de búsqueda, agregar filtros adicionales
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        query += @"
                                   AND (
                                        va.CODIGO_VERSION LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR va.EJERCICIO LIKE '%' + @terminoDeBusqueda + '%'   
                                        OR r.valor_alfanumerico LIKE '%' + @terminoDeBusqueda + '%'  
                                        OR CONVERT(NVARCHAR(MAX), va.FECHA_MODIFICACION, 126) LIKE '%' + @terminoDeBusquedaSQL + '%' 
                                        OR va.NUMERO_VERSION LIKE '%' + @terminoDeBusqueda + '%'
                                    )";
                    }
                    else
                    {
                        if (request.CodigoVersion != 0 && request.CodigoVersion != null)
                        { query += " and va.CODIGO_VERSION LIKE '%' + @parametroCodigoVersion + '%'"; }
                       
                        
                        if (!string.IsNullOrEmpty(request.Estado)) 
                        { query += " and r.valor_alfanumerico LIKE '%' + @parametroEstado + '%'"; }

                        if (request.Ejercicio != 0 && request.Ejercicio != null)
                        { query += " and va.EJERCICIO LIKE '%' + @parametroEJERCICIO + '%'"; }

                        if (request.Fecha != null)
                        { query += " and CONVERT(NVARCHAR(MAX), va.FECHA_MODIFICACION, 126) LIKE '%' + @parametroFecha + '%'"; }

                        if (request.NumeroVersion != 0 && request.NumeroVersion != null)
                        { query += " and va.NUMERO_VERSION LIKE '%' + @parametroNumeroVersion + '%'"; }
                    }
                }


                query += @" ORDER BY va.EJERCICIO
                    OFFSET @saltarRegistros ROWS
                    FETCH NEXT @cantidadRegistros ROWS ONLY";


                string queryCantidadTotalREgistros = @" SELECT COUNT(*)
                   FROM VERSIONES_ANTEPROYECTOS va
                    join referencias r on va.ESTADO=r.valor_decimal
                    JOIN tipo_referencias tr on r.codigo_tipo_referencia=tr.codigo_tipo_referencia
                    where tr.dominio_tipo_referencia='ESTADO_VERSION'
                    and va.VERSION_CONSOLIDADO = 0 and va.numero_version = (SELECT min (numero_version) FROM VERSIONES_ANTEPROYECTOS  
                    WHERE EJERCICIO = va.ejercicio and version_consolidado=0) "
                ;


                if (request.CodigoVersion.ToString() != "0" ||
                   request.Ejercicio.ToString() != "0" ||
                   !string.IsNullOrEmpty(request.Estado) ||
                   !string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                   null != request.Fecha ||
                   request.NumeroVersion.ToString() != "0")


                {
                    // Se proporcionaron parámetros de búsqueda, agregar filtros adicionales
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        queryCantidadTotalREgistros += @"
                                   AND (
                                        va.CODIGO_VERSION LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR va.EJERCICIO LIKE '%' + @terminoDeBusqueda + '%'
                                        OR r.valor_alfanumerico LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR CONVERT(NVARCHAR(MAX), va.FECHA_MODIFICACION, 126) LIKE '%' + @terminoDeBusquedaSQL + '%'
                                        OR va.NUMERO_VERSION LIKE '%' + @terminoDeBusqueda + '%'
                                    )";
                    }
                    else
                    {
                        if (request.CodigoVersion != 0 && request.CodigoVersion != null)
                        { queryCantidadTotalREgistros += " and va.CODIGO_VERSION LIKE '%' + @parametroCodigoVersion + '%'"; }

                        if (request.Ejercicio != 0 && request.Ejercicio != null)
                        { queryCantidadTotalREgistros += " and va.EJERCICIO LIKE '%' + @parametroEJERCICIO + '%'"; }

                        if (!string.IsNullOrEmpty(request.Estado))
                        { queryCantidadTotalREgistros += " and r.valor_alfanumerico LIKE '%' + @parametroEstado + '%'"; }

                        if (request.Fecha != null)
                        { queryCantidadTotalREgistros += " and CONVERT(NVARCHAR(MAX), va.FECHA_MODIFICACION, 126) LIKE '%' + @parametroFecha + '%'"; }

                    }
                }


                // Definición de parámetros
                var convertDate = request.Fecha?.ToString("yyyy-MM-dd");
                var parametros = new DynamicParameters();
                parametros.Add("@terminoDeBusqueda", $"%{request.TerminoDeBusqueda}%");
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.CantidadRegistros);
                parametros.Add("@terminoDeBusquedaSQL", terminoDeBusquedaSQL);  
                // Añadir los parámetros correspondientes de búsqueda
                parametros.Add("@parametroCodigoVersion", request.CodigoVersion.ToString());
                parametros.Add("@parametroEJERCICIO", request.Ejercicio.ToString());
                parametros.Add("@parametroEstado", request.Estado);
                parametros.Add("@parametroFecha", convertDate);
                parametros.Add("@parametroNumeroVersion", request.NumeroVersion.ToString());


                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);

                    var resultado = await connection.QueryAsync<VersionAnteproyectoDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<VersionAnteproyectoDTO>>
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
        public async Task<Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>> AnteproyectoPresupuestarioRequest(AnteproyectoPresupuestarioRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener Versiones Anteproyecto");
            int saltarRegistros = (request.pagina - 1) * request.cantidadRegistros;
            var query = string.Empty;
            try
            {
                query = @"
                    SELECT
                        v.EJERCICIO AS ejercicio,
                        v.CODIGO_VERSION AS codigoVersion,
                        v.CODIGO_ANTEPROYECTO_OBJETO AS codigoAnteproyectoObjeto,
                        v.numero_og AS numeroOg,
                        v.numero_ff AS numeroFf,
                        v.numero_of AS numeroOf,
                        v.numero_dpto AS numeroDpto,
                        v.descripcion_centro_responsabilidad AS descripcionCentroResponsabilidad,
                        v.descripcion_materia AS descripcionMateria,
                        v.fundamentacion AS fundamentacion,
                        v.presupuesto_inicial AS presupuestoInicial,
                        v.modificaciones AS modificaciones,
                        v.presupuesto_vigente AS presupuestoVigente,
                        v.proyecto_presupuesto AS proyectoPresupuesto,
                        v.diferencia AS diferencia,
                        ROUND(
                            CASE
                                WHEN v.presupuesto_vigente <> 0
                                THEN (CAST(v.diferencia AS FLOAT) / CAST(v.presupuesto_vigente AS FLOAT) * 100)
                                ELSE 0
                            END,
                            0
                        ) AS porcentaje,
                        v.total_contrato AS totalContrato
                    FROM vlistaAnteproyectoPresupuestarioCircunscripcion v
                    WHERE v.CODIGO_VERSION = @parametroCodigoVersion ";

                if (!string.IsNullOrEmpty(request.terminoDeBusqueda))
                {
                    query += @"AND(
                                       v.numero_og LIKE '%' + @terminoDeBusqueda + '%'                       
                                       OR v.numero_ff LIKE '%' + @terminoDeBusqueda + '%'  
                                       OR v.descripcion_materia LIKE '%' + @terminoDeBusqueda + '%'
                                       OR v.descripcion_centro_responsabilidad LIKE '%' + @terminoDeBusqueda + '%'
                                       OR v.numero_of LIKE '%' + @terminoDeBusqueda + '%' 
                                       OR v.numero_dpto LIKE '%' + @terminoDeBusqueda + '%'
                                       OR v.fundamentacion LIKE '%' + @terminoDeBusqueda + '%' 
                                       OR v.presupuesto_inicial LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                       OR v.modificaciones LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%' 
                                       OR v.presupuesto_vigente LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                       OR v.total_contrato LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '')+ '%'  
                                       OR v.proyecto_presupuesto LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%' 
                                       OR (v.diferencia) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'  
                                      OR (ROUND(
                            CASE
                                WHEN v.presupuesto_vigente <> 0
                                THEN (CAST(v.diferencia AS FLOAT) / CAST(v.presupuesto_vigente AS FLOAT) * 100)
                                ELSE 0
                            END,
                            0
                        )) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'  
                                    )";
                }
                else
                {
                    query += @"";
                    if (request.numeroOg != 0 && request.numeroOg != null)
                    { query += " and v.numero_og LIKE '%' + @parametroNumero_og + '%'"; }
                    if (request.numeroFf != 0 && request.numeroFf != null)
                    { query += " and v.numero_ff LIKE '%' + @parametroNumero_ff + '%'"; }
                    if (request.totalContrato != "0" && request.totalContrato != null)
                    { query += " and v.total_contrato LIKE '%' + REPLACE( @parametroTotalContrato, '.', '') + '%'"; }
                    if (request.codigoAnteproyectoObjeto != 0 && request.codigoAnteproyectoObjeto != null)
                    { query += " and v.CODIGO_ANTEPROYECTO_OBJETO LIKE '%' + @parametroCodigoAnteproyectoObjeto + '%'"; }
                    if (!string.IsNullOrEmpty(request.descripcionMateria))
                    { query += " and v.descripcion_materia LIKE '%' + @parametroDescripcionMateria + '%'"; }
                    if (!string.IsNullOrEmpty(request.descripcionCentroResponsabilidad))
                    { query += " and v.descripcion_centro_responsabilidad LIKE '%' + @parametroDescripcionCentroResponsabilidad + '%'"; }
                    if (request.codigoVersion != 0 && request.codigoVersion != null)
                    { query += " and v.CODIGO_VERSION LIKE '%' + @parametroCodigoVersion + '%'"; }
                    if (request.numeroOf != 0 && request.numeroOf != null)
                    { query += " and v.numero_of LIKE '%' + @parametroNumero_of + '%'"; }
                    if (request.numeroDpto != 0 && request.numeroDpto != null)
                    { query += " and v.numero_dpto LIKE '%' + @parametroNumero_dpto + '%'"; }
                    if (!string.IsNullOrEmpty(request.fundamentacion))
                    { query += " and v.fundamentacion LIKE '%' + @parametroFundamentacion + '%'"; }
                    if (request.presupuestoInicial != "0" && request.presupuestoInicial != null)
                    { query += " and v.presupuesto_inicial LIKE '%' + REPLACE(@parametroPresupuesto_inicial, '.', '') + '%'"; }
                    if (request.diferencia != "0" && request.diferencia != null)
                    { query += " and v.diferencia LIKE '%' + REPLACE( @parametroDiferencia, '.', '') + '%'"; }

                    if (request.modificaciones != "0" && request.modificaciones != null)
                    { query += " and v.modificaciones LIKE '%' + REPLACE(@parametroModificaciones, '.', '') + '%'"; }
                    if (request.presupuestoVigente != "0" && request.presupuestoVigente != null)
                    { query += " and v.presupuesto_vigente LIKE '%' + REPLACE( @presupuestoVigente, '.', '') + '%'"; }

                    if (request.proyectoPresupuesto != "0" && request.proyectoPresupuesto != null)
                    { query += " and v.proyecto_presupuesto LIKE '%' + REPLACE( @parametroProyectoPresupuesto, '.', '') + '%'"; }

                    if (request.porcentaje != "0" && request.porcentaje != null)
                    { query += @" and ROUND(CASE WHEN v.presupuesto_vigente <> 0 
                                 THEN (CAST(v.diferencia AS FLOAT) / CAST(v.presupuesto_vigente AS FLOAT) * 100)  ELSE 0  END,  0 ) 
                                 LIKE '%' + REPLACE( @parametroPorcentaje, '.', '') + '%'"; }
                }

                query += @" order by numeroOg, numeroFf, numeroOf";
                query += @" OFFSET @saltarRegistros ROWS";
                query += @" FETCH NEXT @cantidadRegistros ROWS ONLY";


                string queryCantidadTotalREgistros = @"
                    SELECT COUNT(*) AS TotalRegistros
                    FROM vlistaAnteproyectoPresupuestarioCircunscripcion v      
                    WHERE v.CODIGO_VERSION = @parametroCodigoVersion ";

                if (!string.IsNullOrEmpty(request.terminoDeBusqueda))
                {
                    queryCantidadTotalREgistros += @"AND(
                                       v.numero_og LIKE '%' + @terminoDeBusqueda + '%'                       
                                       OR v.numero_ff LIKE '%' + @terminoDeBusqueda + '%'  
                                       OR v.descripcion_materia LIKE '%' + @terminoDeBusqueda + '%'
                                       OR v.descripcion_centro_responsabilidad LIKE '%' + @terminoDeBusqueda + '%'
                                       OR v.numero_of LIKE '%' + @terminoDeBusqueda + '%' 
                                       OR v.numero_dpto LIKE '%' + @terminoDeBusqueda + '%'
                                       OR v.fundamentacion LIKE '%' + @terminoDeBusqueda + '%' 
                                       OR v.presupuesto_inicial LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                       OR v.modificaciones LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%' 
                                       OR v.presupuesto_vigente LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                       OR v.total_contrato LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '')+ '%'  
                                       OR v.proyecto_presupuesto LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%' 
                                       OR (v.diferencia) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'  
                                      OR (ROUND(
                            CASE
                                WHEN v.presupuesto_vigente <> 0
                                THEN (CAST(v.diferencia AS FLOAT) / CAST(v.presupuesto_vigente AS FLOAT) * 100)
                                ELSE 0
                            END,
                            0
                        )) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'  
                                    )";
                }
                else
                {
                    queryCantidadTotalREgistros += @"";
                    if (request.numeroOg != 0 && request.numeroOg != null)
                    { queryCantidadTotalREgistros += " and v.numero_og LIKE '%' + @parametroNumero_og + '%'"; }
                    if (request.numeroFf != 0 && request.numeroFf != null)
                    { queryCantidadTotalREgistros += " and v.numero_ff LIKE '%' + @parametroNumero_ff + '%'"; }
                    if (request.totalContrato != "0" && request.totalContrato != null)
                    { queryCantidadTotalREgistros += " and v.total_contrato LIKE '%' + REPLACE( @parametroTotalContrato, '.', '') + '%'"; }
                    if (request.codigoAnteproyectoObjeto != 0 && request.codigoAnteproyectoObjeto != null)
                    { queryCantidadTotalREgistros += " and v.CODIGO_ANTEPROYECTO_OBJETO LIKE '%' + @parametroCodigoAnteproyectoObjeto + '%'"; }
                    if (!string.IsNullOrEmpty(request.descripcionMateria))
                    { queryCantidadTotalREgistros += " and v.descripcion_materia LIKE '%' + @parametroDescripcionMateria + '%'"; }
                    if (!string.IsNullOrEmpty(request.descripcionCentroResponsabilidad))
                    { queryCantidadTotalREgistros += " and v.descripcion_centro_responsabilidad LIKE '%' + @parametroDescripcionCentroResponsabilidad + '%'"; }
                    if (request.numeroOf != 0 && request.numeroOf != null)
                    { queryCantidadTotalREgistros += " and v.numero_of LIKE '%' + @parametroNumero_of + '%'"; }
                    if (request.numeroDpto != 0 && request.numeroDpto != null)
                    { queryCantidadTotalREgistros += " and v.numero_dpto LIKE '%' + @parametroNumero_dpto + '%'"; }
                    if (!string.IsNullOrEmpty(request.fundamentacion))
                    { queryCantidadTotalREgistros += " and v.fundamentacion LIKE '%' + @parametroFundamentacion + '%'"; }
                    if (request.presupuestoInicial != "0" && request.presupuestoInicial != null)
                    { queryCantidadTotalREgistros += " and v.presupuesto_inicial LIKE '%' + REPLACE(@parametroPresupuesto_inicial, '.', '') + '%'"; }
                    if (request.diferencia != "0" && request.diferencia != null)
                    { queryCantidadTotalREgistros += " and v.diferencia LIKE '%' + REPLACE( @parametroDiferencia, '.', '') + '%'"; }

                    if (request.modificaciones != "0" && request.modificaciones != null)
                    { queryCantidadTotalREgistros += " and v.modificaciones LIKE '%' + REPLACE(@parametroModificaciones, '.', '') + '%'"; }
                    if (request.presupuestoVigente != "0" && request.presupuestoVigente != null)
                    { queryCantidadTotalREgistros += " and v.presupuesto_vigente LIKE '%' + REPLACE( @presupuestoVigente, '.', '') + '%'"; }

                    if (request.proyectoPresupuesto != "0" && request.proyectoPresupuesto != null)
                    { queryCantidadTotalREgistros += " and v.proyecto_presupuesto LIKE '%' + REPLACE( @parametroProyectoPresupuesto, '.', '') + '%'"; }
                    if (request.porcentaje != "0" && request.porcentaje != null)
                    { queryCantidadTotalREgistros += @" and ROUND(CASE WHEN v.presupuesto_vigente <> 0 
                                 THEN (CAST(v.diferencia AS FLOAT) / CAST(v.presupuesto_vigente AS FLOAT) * 100)  ELSE 0  END,  0 ) 
                                 LIKE '%' + REPLACE( @parametroPorcentaje, '.', '') + '%'"; }
                


            }


                // Definición de parámetros
                var parametros = new DynamicParameters();
                parametros.Add("@terminoDeBusqueda", $"%{request.terminoDeBusqueda}%");
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.cantidadRegistros);

                // Añadir los parámetros correspondientes de búsqueda
                parametros.Add("@parametroNumero_og", request.numeroOg.ToString());
                parametros.Add("@parametroNumero_ff", request.numeroFf.ToString());
                parametros.Add("@parametroNumero_of", request.numeroOf.ToString());
                parametros.Add("@parametroModificaciones", request.modificaciones);
                parametros.Add("@parametroNumero_dpto", request.numeroDpto.ToString());
                parametros.Add("@parametroFundamentacion", request.fundamentacion);
                parametros.Add("@parametroPresupuesto_inicial", request.presupuestoInicial);
                parametros.Add("@parametroPresupuesto_aprobado", request.presupuestoAprobado);
                parametros.Add("@parametroProyectoPresupuesto", request.proyectoPresupuesto);
                parametros.Add("@presupuestoVigente", request.presupuestoVigente);
                parametros.Add("@parametroDiferencia", request.diferencia);
                parametros.Add("@parametroCodigoVersion", request.codigoVersion.ToString());
                parametros.Add("@parametroTotalContrato", request.totalContrato);
                parametros.Add("@parametroCodigoAnteproyectoObjeto", request.codigoAnteproyectoObjeto.ToString());
                parametros.Add("@parametroDescripcionMateria", request.descripcionMateria);
                parametros.Add("@parametrodescripcionCentroResponsabilidad", request.descripcionCentroResponsabilidad);
                parametros.Add("@parametroPorcentaje", request.porcentaje);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);
                    var resultado = await connection.QueryAsync<AnteproyectoPresupuestarioDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>
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
        public async Task<CantidadTotalDiasDTO> CantidadTotalDias()
        {
            _logger.LogInformation("Inicio de Proceso de Obtener Cantidad total de días");
            // Definir la consulta SQL para obtener los valores
            string query = @"SELECT DATEDIFF(DAY, GETDATE(), vfc.FECHA_CIERRE) AS CantidadTotal FROM  
                                    VERSION_FECHA_CIERRE vfc WHERE  vfc.activo = 1";

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryFirstOrDefaultAsync<CantidadTotalDiasDTO>(query);
                if (resultado == null)
                {
                    _logger.LogInformation("Fin de Proceso de Obtener Cantidades de días");
                    return null;
                }
                _logger.LogInformation("Fin de Proceso de Obtener  Cantidades de días");
                return resultado;
            }

        }
        public async Task<int> ActualizarEstadoVersionAnteproyecto(ActualizarEstadoVersionAnteproyectoRequest version)
        {
            _logger.LogInformation("Inicio de Proceso de Actualizar Estado Version Anteproyecto ");
            string queryUltima = @" select Top 1 ESTADO as Estado, 
                                    CODIGO_VERSION as CodigoVersion, 
                                    CODIGO_CIRCUNSCRIPCION as CodigoCircunscripcion, 
                                    EJERCICIO  as Ejercicio,
                                    USUARIO_INSERTO as UsuarioInserto,
                                    FECHA_INSERTO as Fecha, 
                                    ES_LEY as EsLey
                                    from versiones_anteproyectos where ESTADO=2 and version_consolidado=1 
									order by versiones_anteproyectos.EJERCICIO desc, CODIGO_VERSION desc";
            string usuario;
            int versionAntProy = 0;
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultadoultima = await connection.QueryAsync<VersionAnteproyectoDTO>(queryUltima);
                if (resultadoultima.Count() != 0)
                {
                    var resultadoValores = resultadoultima.First();
                    versionAntProy = resultadoValores.CodigoVersion;
                    if (versionAntProy == 0)
                    {
                        versionAntProy = version.CodigoVersion;
                    }

                    var parametroValidar = new
                    {

                        p_codigo_version = resultadoValores.CodigoVersion.ToString(),

                    };
                }

                string query = @"UPDATE va SET va.estado= @p_estado, va.usuario_modificacion = @CODIGO_USUARIO_LOGUEADO,va.fecha_modificacion = GETDATE(),va.es_ley=@p_esLey
                    FROM versiones_anteproyectos AS va WHERE va.codigo_version= (SELECT MAX(codigo_version) FROM versiones_anteproyectos WHERE estado = 1) AND va.estado=1 ";
                       
                                            
                string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad =@UsuarioUltimaModificacion";

                usuario = await connection.QueryFirstOrDefaultAsync<string>(queryUsuario, new { UsuarioUltimaModificacion = version.CodigoUsuarioLoggeado });

                var parametro = new
                {
                    p_estado = 2,//version.Estado,                    
                    CODIGO_USUARIO_LOGUEADO = usuario,
                    p_esLey=version.Esley
                };

               

                if (version.Esley)
                {
                    string queryUltimoValorPresupuesto = @"SELECT ISNULL(MAX(codigo_presupuesto),0) FROM presupuesto_gastos";
                    int UltimoValorPresupuesto = await connection.ExecuteScalarAsync<int>(queryUltimoValorPresupuesto);



                    // Seleccionar registros de SourceTable
                    var sourceQueryPresupuesto = "select vcr.codigo_tipo_presupuesto as CodigoTipoPresupuesto ," +
                    " vcr.codigo_programa as CodigoPrograma," +
                    " vao.codigo_fuente_financiamiento as CodigoFuenteFinanciamiento," +
                    " vao.codigo_organismo_financiador as CodigoOrganismoFinanciador," +
                    " cp.codigo_objeto_gasto as CodigoObjetoGasto, " +
                    " va.ejercicio as Ejercicio , " +
                    " vcr.codigo_actividad as CodigoActividad, " +
                    " vcr.codigo_departamento as CodigoDepartamento, " +
                    " sum(vab.CANTIDAD*vab.VALOR_UNITARIO)  as MontoPresupuesto " +
                    " FROM  versiones_anteproyectos va   " +
                    " join versiones_anteproyectos_objetos vao ON vao.codigo_version= va.codigo_version " +
                    " join configuracion_presupuestaria cp  " +
                    " on cp.codigo_configuracion_presupuestaria=vao.codigo_configuracion_presupuestaria " +
                    " join vListaCentrosResponsabilidadPorCircunscripcion vcr " +
                    " on vcr.codigo_circunscripcion=va.codigo_circunscripcion  " +
                    " and vcr.codigo_centro_responsabilidad=cp.CODIGO_CENTRO_RESPONSABILIDAD " +
                    " and vcr.codigo_materia=cp.codigo_materia" +
                    " join VERSIONES_ANTEPROYECTOS_BIENES vab on vab.CODIGO_VERSION=vao.CODIGO_VERSION " +
                    " and vab.CODIGO_ANTEPROYECTO_OBJETO=vao.CODIGO_ANTEPROYECTO_OBJETO " +
                    " where  va.CODIGO_VERSION= " + versionAntProy.ToString() +
                    " and va.version_consolidado=1 " +
                    " group by   vcr.codigo_tipo_presupuesto," +
                    " vcr.codigo_programa, " +
                    " vao.codigo_fuente_financiamiento," +
                    " vao.codigo_organismo_financiador, " +
                    " cp.codigo_objeto_gasto," +
                    " va.ejercicio, " +
                    " vcr.codigo_actividad," +
                    " vcr.codigo_departamento ";
                    var sourceRecordsPresupuesto = await connection.QueryAsync<PresupuestoGastos>(sourceQueryPresupuesto);

                    // Insertar registros en presupuesto_gastos

                    var insertQueryPresupuesto = "INSERT INTO presupuesto_gastos " +
                        " (codigo_tipo_presupuesto," +
                        " codigo_programa," +
                        " codigo_fuente_financiamiento, " +
                        " codigo_organismo_financiador," +
                        " codigo_objeto_gasto, " +
                        " Activo, " +
                        " monto_presupuesto," +
                        " Ejercicio," +
                        " codigo_actividad," +
                        " codigo_subprograma," +
                        " codigo_departamento ) " +
                        " VALUES (@CodigoTipoPresupuesto," +
                        " @CodigoPrograma, " +
                        " @CodigoFuenteFinanciamiento," +
                        " @CodigoOrganismoFinanciador," +
                        " @CodigoObjetoGasto," +
                        " @Activo," +
                        " @MontoPresupuesto," +
                        " @Ejercicio," +
                        " -1," +
                        " @CodigoActividad," +
                        " @CodigoDepartamento)";
                    foreach (var recordPresupuesto in sourceRecordsPresupuesto)
                    {
                        UltimoValorPresupuesto = UltimoValorPresupuesto + 1;
                        var modifiedRecordPresupuesto = new PresupuestoGastos
                        {
                            //CodigoPresupuesto = UltimoValorPresupuesto,
                            CodigoTipoPresupuesto = recordPresupuesto.CodigoTipoPresupuesto,
                            CodigoPrograma = recordPresupuesto.CodigoPrograma,
                            CodigoFuenteFinanciamiento = recordPresupuesto.CodigoFuenteFinanciamiento,
                            CodigoOrganismoFinanciador = recordPresupuesto.CodigoOrganismoFinanciador,
                            CodigoObjetoGasto = recordPresupuesto.CodigoObjetoGasto,
                            Activo = recordPresupuesto.Activo,
                            MontoPresupuesto = recordPresupuesto.MontoPresupuesto,
                            Ejercicio = recordPresupuesto.Ejercicio,
                            CodigoActividad = -1,//134437,////////////////////////////////////////
                            CodigoDepartamento = recordPresupuesto.CodigoDepartamento,

                        };
                        await connection.ExecuteAsync(insertQueryPresupuesto, modifiedRecordPresupuesto);

                    }


                }


                
                #region querys
                string selectSbcNoPendientes = @"select 
                    sbc.codigo_solicitud as CodigoSolicitud,
                    sbc.estado as Estado
                from solicitudes_bienes_circunscripcion sbc 
                where 1 != ANY(select estado from solicitudes_objetos_detalle sod where sbc.codigo_solicitud = sod.codigo_solicitud) and sbc.estado = 1 ";


                string selectSbcTodosPendientes = @"select 
                    sbc.codigo_solicitud as CodigoSolicitud,
                    sbc.estado as Estado
                from solicitudes_bienes_circunscripcion sbc 
                where 1 = ALL(select estado from solicitudes_objetos_detalle sod where sbc.codigo_solicitud = sod.codigo_solicitud) and sbc.estado = 1 ";

                string updateSbcNoPendientes = @"UPDATE solicitudes_bienes_circunscripcion
                SET estado = 2
                WHERE codigo_solicitud = @CodigoSolicitud ";


                string updateSbcTodosPendientes = @"UPDATE solicitudes_bienes_circunscripcion
                SET estado = 3
                WHERE codigo_solicitud = @CodigoSolicitud ";

                string updateSodNoPendientes = $@"update solicitudes_objetos_detalle
                SET estado = 3
                where estado = 1 AND codigo_solicitud = @CodigoSolicitud ";

                string updateSodTodosPendientes = @"update solicitudes_objetos_detalle
                SET estado = 3
                where estado = 1 AND codigo_solicitud = @CodigoSolicitud ";
                #endregion

                try
                {

                    var resultado = await connection.ExecuteAsync(query, parametro);

                    // Actualizacion de Estados de Solicitudes y objetos de Gasto
                    // Selecciona las solicitudes que poseen OG que se encuentran en estado diferente a pendiente
                    var solicitudesNoPendientes = (await connection.QueryAsync<SolicitudBienesCircunscripcionDTO>(selectSbcNoPendientes)).ToList();
                    // Selecciona las solicitudes que no poseen OG que se encuentren en estado diferente a pendiente
                    var solicitudesTodosPendientes = (await connection.QueryAsync<SolicitudBienesCircunscripcionDTO>(selectSbcTodosPendientes)).ToList();

                    if (solicitudesNoPendientes.Any())
                    {
                        await connection.ExecuteAsync(updateSbcNoPendientes, solicitudesNoPendientes);
                        await connection.ExecuteAsync(updateSodNoPendientes, solicitudesNoPendientes);
                    }
                    if (solicitudesTodosPendientes.Any())
                    {
                        await connection.ExecuteAsync(updateSbcTodosPendientes, solicitudesTodosPendientes);
                        await connection.ExecuteAsync(updateSodTodosPendientes, solicitudesTodosPendientes);
                    }

                    _logger.LogInformation("Fin de Proceso de Actualizar Estado Version Anteproyecto");
                    return resultado;

                }
                catch (Exception ex)
                {
                    throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Estado Version Anteproyecto de versiones_anteproyectos");
                }
            }
        }

        public async Task<int> CerrarEstadoVersionAnteproyecto(ActualizarEstadoVersionAnteproyectoRequest version)
        {
            _logger.LogInformation("Inicio de Proceso de Actualizar Estado Version Anteproyecto ");
            string queryUltima = @" select Top 1 ESTADO as Estado, 
                                    CODIGO_VERSION as CodigoVersion, 
                                    CODIGO_CIRCUNSCRIPCION as CodigoCircunscripcion, 
                                    EJERCICIO  as Ejercicio,
                                    USUARIO_INSERTO as UsuarioInserto,
                                    FECHA_INSERTO as Fecha, 
                                    ES_LEY as EsLey
                                    from versiones_anteproyectos where ESTADO=2 and version_consolidado=1 
									order by versiones_anteproyectos.EJERCICIO desc, CODIGO_VERSION desc";
            string usuario;
            int versionAntProy = 0;
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultadoultima = await connection.QueryAsync<VersionAnteproyectoDTO>(queryUltima);
                if (resultadoultima.Count() != 0)
                {
                    var resultadoValores = resultadoultima.First();
                    versionAntProy = resultadoValores.CodigoVersion;
                    if (versionAntProy == 0)
                    {
                        versionAntProy = version.CodigoVersion;
                    }

                    var parametroValidar = new
                    {

                        p_codigo_version = resultadoValores.CodigoVersion.ToString(),

                    };
                }

                string query = @"UPDATE va SET va.estado= @p_estado, va.usuario_modificacion = @CODIGO_USUARIO_LOGUEADO,va.fecha_modificacion = GETDATE(),va.es_ley=@p_esLey
                    FROM versiones_anteproyectos AS va WHERE va.codigo_version= (SELECT MAX(codigo_version) FROM versiones_anteproyectos WHERE estado = 1) AND va.estado=1 ";


                string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad =@UsuarioUltimaModificacion";

                usuario = await connection.QueryFirstOrDefaultAsync<string>(queryUsuario, new { UsuarioUltimaModificacion = version.CodigoUsuarioLoggeado });

                var parametro = new
                {
                    p_estado = 2,//version.Estado,                    
                    CODIGO_USUARIO_LOGUEADO = usuario,
                    p_esLey = version.Esley
                };



                if (version.Esley)
                {
                    string queryUltimoValorPresupuesto = @"SELECT ISNULL(MAX(codigo_presupuesto),0) FROM presupuesto_gastos";
                    int UltimoValorPresupuesto = await connection.ExecuteScalarAsync<int>(queryUltimoValorPresupuesto);



                    // Seleccionar registros de SourceTable
                    var sourceQueryPresupuesto = "select vcr.codigo_tipo_presupuesto as CodigoTipoPresupuesto ," +
                    " vcr.codigo_programa as CodigoPrograma," +
                    " vao.codigo_fuente_financiamiento as CodigoFuenteFinanciamiento," +
                    " vao.codigo_organismo_financiador as CodigoOrganismoFinanciador," +
                    " cp.codigo_objeto_gasto as CodigoObjetoGasto, " +
                    " va.ejercicio as Ejercicio , " +
                    " vcr.codigo_actividad as CodigoActividad, " +
                    " vcr.codigo_departamento as CodigoDepartamento, " +
                    " sum(vab.CANTIDAD*vab.VALOR_UNITARIO)  as MontoPresupuesto " +
                    " FROM  versiones_anteproyectos va   " +
                    " join versiones_anteproyectos_objetos vao ON vao.codigo_version= va.codigo_version " +
                    " join configuracion_presupuestaria cp  " +
                    " on cp.codigo_configuracion_presupuestaria=vao.codigo_configuracion_presupuestaria " +
                    " join vListaCentrosResponsabilidadPorCircunscripcion vcr " +
                    " on vcr.codigo_circunscripcion=va.codigo_circunscripcion  " +
                    " and vcr.codigo_centro_responsabilidad=cp.CODIGO_CENTRO_RESPONSABILIDAD " +
                    " and vcr.codigo_materia=cp.codigo_materia" +
                    " join VERSIONES_ANTEPROYECTOS_BIENES vab on vab.CODIGO_VERSION=vao.CODIGO_VERSION " +
                    " and vab.CODIGO_ANTEPROYECTO_OBJETO=vao.CODIGO_ANTEPROYECTO_OBJETO " +
                    " where  va.CODIGO_VERSION= " + versionAntProy.ToString() +
                    " and va.version_consolidado=1 " +
                    " group by   vcr.codigo_tipo_presupuesto," +
                    " vcr.codigo_programa, " +
                    " vao.codigo_fuente_financiamiento," +
                    " vao.codigo_organismo_financiador, " +
                    " cp.codigo_objeto_gasto," +
                    " va.ejercicio, " +
                    " vcr.codigo_actividad," +
                    " vcr.codigo_departamento ";
                    var sourceRecordsPresupuesto = await connection.QueryAsync<PresupuestoGastos>(sourceQueryPresupuesto);

                    // Insertar registros en presupuesto_gastos

                    var insertQueryPresupuesto = "INSERT INTO presupuesto_gastos " +
                        " (codigo_tipo_presupuesto," +
                        " codigo_programa," +
                        " codigo_fuente_financiamiento, " +
                        " codigo_organismo_financiador," +
                        " codigo_objeto_gasto, " +
                        " Activo, " +
                        " monto_presupuesto," +
                        " Ejercicio," +
                        " codigo_actividad," +
                        " codigo_subprograma," +
                        " codigo_departamento ) " +
                        " VALUES (@CodigoTipoPresupuesto," +
                        " @CodigoPrograma, " +
                        " @CodigoFuenteFinanciamiento," +
                        " @CodigoOrganismoFinanciador," +
                        " @CodigoObjetoGasto," +
                        " @Activo," +
                        " @MontoPresupuesto," +
                        " @Ejercicio," +
                        " -1," +
                        " @CodigoActividad," +
                        " @CodigoDepartamento)";
                    foreach (var recordPresupuesto in sourceRecordsPresupuesto)
                    {
                        UltimoValorPresupuesto = UltimoValorPresupuesto + 1;
                        var modifiedRecordPresupuesto = new PresupuestoGastos
                        {
                            //CodigoPresupuesto = UltimoValorPresupuesto,
                            CodigoTipoPresupuesto = recordPresupuesto.CodigoTipoPresupuesto,
                            CodigoPrograma = recordPresupuesto.CodigoPrograma,
                            CodigoFuenteFinanciamiento = recordPresupuesto.CodigoFuenteFinanciamiento,
                            CodigoOrganismoFinanciador = recordPresupuesto.CodigoOrganismoFinanciador,
                            CodigoObjetoGasto = recordPresupuesto.CodigoObjetoGasto,
                            Activo = recordPresupuesto.Activo,
                            MontoPresupuesto = recordPresupuesto.MontoPresupuesto,
                            Ejercicio = recordPresupuesto.Ejercicio,
                            CodigoActividad = -1,//134437,////////////////////////////////////////
                            CodigoDepartamento = recordPresupuesto.CodigoDepartamento,

                        };
                        await connection.ExecuteAsync(insertQueryPresupuesto, modifiedRecordPresupuesto);

                    }


                }
                            

                try
                {

                    var resultado = await connection.ExecuteAsync(query, parametro);

                    

                    _logger.LogInformation("Fin de Proceso de Actualizar Estado Version Anteproyecto");
                    return resultado;

                }
                catch (Exception ex)
                {
                    throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Estado Version Anteproyecto de versiones_anteproyectos");
                }
            }
        }

        public async Task<int> ActualizarVersionAnteproyectoObjeto(ActualizarVersionAnteproyectoObjetoRequest version)
        {
            _logger.LogInformation("Inicio de Proceso de Actualizar Objeto Version Anteproyecto ");

            string query = @" UPDATE vao SET vao.presupuesto_inicial = @p_valor_presupuesto , vao.modificaciones = @p_modificacion                          
                            FROM VERSIONES_ANTEPROYECTOS_OBJETOS vao 
                              where vao.codigo_version= @p_codigo_version and vao.codigo_anteproyecto_objeto= @p_codigo_anteproyecto_objeto; 
                              UPDATE va SET  
                            va.USUARIO_MODIFICACION = @UsuarioUltimaModificacion, va.fecha_modificacion = GETDATE()
                            FROM  VERSIONES_ANTEPROYECTOS va
                            WHERE va.estado = 1 AND    va.codigo_version= @p_codigo_version";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string usuario;
                    string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad =@UsuarioUltimaModificacion";

                    usuario = await connection.QueryFirstOrDefaultAsync<string>(queryUsuario, new { UsuarioUltimaModificacion = version.UsuarioUltimaModificacion });

                    var parametro = new
                    {
                        p_valor_presupuesto = version.PresupuestoInicial,
                        p_modificacion = version.Modificaciones,
                        UsuarioUltimaModificacion = usuario,
                        p_codigo_version = version.CodigoVersion,
                        p_codigo_anteproyecto_objeto = version.CodigoAnteproyectoObjeto,
                    };

                    var resultado = await connection.ExecuteAsync(query, parametro);

                    _logger.LogInformation("Fin de Proceso de Actualizar Estado Version Anteproyecto");

                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Onjeto Version Anteproyecto de versiones_anteproyectos");
            }


        }

        #endregion

        #region DESARROLLO DE LA INCIDENCIA CSJ-154
        public async Task<Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>> AnteproyectoPresupuestarioVerificacionesRequest(AnteproyectoPresupuestarioRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de Obtener Versiones Anteproyecto");
            int saltarRegistros = (request.pagina - 1) * request.cantidadRegistros;
            var query = string.Empty;
            try
            {
                query = @"SELECT  
                        v.CODIGO_VERSION AS codigoVersion,
                        v.CODIGO_OBJETO_GASTO AS codigoObjetoGasto,
                        v.CODIGO_ANTEPROYECTO_OBJETO as codigoAnteproyectoObjeto,
                        v.CODIGO_ORGANISMO_FINANCIADOR AS codigoOrganismoF,
                        v.CODIGO_FUENTE_FINANCIAMIENTO AS codigoFuenteF, 
                        v.NUMERO_VERSION AS numeroVersion,	
                        v.ejercicio AS ejercicio,
                        v.codigo_circunscripcion_origen AS CodigoCircunscripcion,
                        v.nombre_circunscripcion AS circunscripcion,
                        v.numero_og AS numeroOg, 
                        v.numero_ff AS numeroFf, 
                        v.numero_of AS numeroOf,
                        v.numero_dpto AS numeroDpto,
                        v.fundamentacion AS fundamentacion,
                        v.estado AS estado,
                        v.codigo_configuracion_presupuestaria AS codigoConfiguracionPresupuestaria,
                        SUM(v.presupuesto_inicial) AS presupuestoInicial, 
                        SUM(v.modificaciones) AS modificaciones,  
                        SUM(v.presupuesto_vigente) AS presupuestoVigente, 
                        SUM(v.proyecto_presupuesto) AS proyectoPresupuesto,
                        SUM(v.proyecto_presupuesto - v.presupuesto_vigente) AS diferencia, 
                        ROUND(
                            CASE 
                                WHEN SUM(v.presupuesto_vigente) <> 0 THEN 
                                    (CAST(SUM(v.proyecto_presupuesto - v.presupuesto_vigente) AS FLOAT) / CAST(SUM(v.presupuesto_vigente) AS FLOAT) * 100)
                                ELSE 
                                    0 
                            END, 0
                        ) AS porcentaje 
                    FROM 
                        vlistaAnteproyectoPresupuestarioCircunscripcion v 
                    WHERE 
                        v.codigo_version= @parametroCodigoVersion ";

                string queryCantidadTotalREgistros = @"  select COUNT(*) FROM ( SELECT  
                        v.CODIGO_VERSION AS codigoVersion,
                        v.CODIGO_OBJETO_GASTO AS codigoObjetoGasto,
                        v.CODIGO_ANTEPROYECTO_OBJETO as codigoAnteproyectoObjeto,
                        v.CODIGO_ORGANISMO_FINANCIADOR AS codigoOrganismoF,
                        v.CODIGO_FUENTE_FINANCIAMIENTO AS codigoFuenteF, 
                        v.NUMERO_VERSION AS numeroVersion,	
                        v.ejercicio AS ejercicio,
                        v.codigo_circunscripcion_origen AS CodigoCircunscripcion,
                        v.nombre_circunscripcion AS circunscripcion,
                        v.numero_og AS numeroOg, 
                        v.numero_ff AS numeroFf, 
                        v.numero_of AS numeroOf,
                        v.numero_dpto AS numeroDpto,
                        v.fundamentacion AS fundamentacion,
                        v.estado AS estado,
                        v.codigo_configuracion_presupuestaria AS codigoConfiguracionPresupuestaria,
                        SUM(v.presupuesto_inicial) AS presupuestoInicial, 
                        SUM(v.modificaciones) AS modificaciones,  
                        SUM(v.presupuesto_vigente) AS presupuestoVigente, 
                        SUM(v.proyecto_presupuesto) AS proyectoPresupuesto,
                        SUM(v.proyecto_presupuesto - v.presupuesto_vigente) AS diferencia, 
                        ROUND(
                            CASE 
                                WHEN SUM(v.presupuesto_vigente) <> 0 THEN 
                                    (CAST(SUM(v.proyecto_presupuesto - v.presupuesto_vigente) AS FLOAT) / CAST(SUM(v.presupuesto_vigente) AS FLOAT) * 100)
                                ELSE 
                                    0 
                            END, 0
                        ) AS porcentaje 
            FROM vlistaAnteproyectoPresupuestarioCircunscripcion v
            WHERE v.codigo_version = @parametroCodigoVersion";



                if (string.IsNullOrEmpty(request.terminoDeBusqueda))
                {
                    
                        query += @"";
                        if (request.codigoVersion != 0 && request.codigoVersion != null)//
                        { query += " and v.NUMERO_VERSION LIKE '%' + @parametroCodigoVersion + '%'"; }
                        if (request.codigoObjetoGasto != 0)
                        { query += " and  v.CODIGO_OBJETO_GASTO LIKE '%' + @parametroCodigoObjetoGasto + '%'"; }
                        if (request.numeroOg != 0 && request.numeroOg != null)//
                        { query += " and v.numero_og LIKE '%' + @parametroNumero_og + '%'"; }
                        if (!string.IsNullOrEmpty(request.circunscripcion))//
                        { query += " and v.nombre_circunscripcion LIKE '%' + @parametroCircunscripcion + '%'"; }
                        if (request.codigoCircunscripcion != 0)//
                        { query += " and v.codigo_circunscripcion_origen LIKE '%' + @parametroCodigoCircunscripcion + '%'"; }
                        if (!string.IsNullOrEmpty(request.estado))
                        { query += " and v.estado LIKE '%' + @parametroEstado + '%'"; }
                        if (request.ejercicio != 0 && request.ejercicio != null)
                        { query += " and v.ejercicio LIKE '%' + @parametroEjercicio + '%'"; }
                        if (request.numeroDpto != 0 && request.numeroDpto != null)
                        { query += " and v.numero_dpto LIKE '%' + @parametroNumero_dpto + '%'"; }
                        if (request.numeroOf != 0 && request.numeroOf != null)
                        { query += " and v.numero_of LIKE '%' + @parametroNumero_of + '%'"; }
                        if (request.numeroFf != 0 && request.numeroFf != null)
                        { query += " and v.numero_ff LIKE '%' + @parametroNumero_ff + '%'"; }
                        if (!string.IsNullOrEmpty(request.fundamentacion))
                        { query += " and v.fundamentacion LIKE '%' + @parametroFundamentacion + '%'"; }


                    queryCantidadTotalREgistros += @"";
                    if (request.codigoVersion != 0 && request.codigoVersion != null)//
                    { queryCantidadTotalREgistros += " and v.NUMERO_VERSION LIKE '%' + @parametroCodigoVersion + '%'"; }
                    if (request.codigoObjetoGasto != 0)
                    { queryCantidadTotalREgistros += " and  v.CODIGO_OBJETO_GASTO LIKE '%' + @parametroCodigoObjetoGasto + '%'"; }
                    if (request.numeroOg != 0 && request.numeroOg != null)//
                    { queryCantidadTotalREgistros += " and v.numero_og LIKE '%' + @parametroNumero_og + '%'"; }
                    if (!string.IsNullOrEmpty(request.circunscripcion))//
                    { queryCantidadTotalREgistros += " and v.nombre_circunscripcion LIKE '%' + @parametroCircunscripcion + '%'"; }
                    if (request.codigoCircunscripcion != 0)//
                    { queryCantidadTotalREgistros += " and v.codigo_circunscripcion_origen LIKE '%' + @parametroCodigoCircunscripcion + '%'"; }
                    if (!string.IsNullOrEmpty(request.estado))
                    { queryCantidadTotalREgistros += " and v.estado LIKE '%' + @parametroEstado + '%'"; }
                    if (request.ejercicio != 0 && request.ejercicio != null)
                    { queryCantidadTotalREgistros += " and v.ejercicio LIKE '%' + @parametroEjercicio + '%'"; }
                    if (request.numeroDpto != 0 && request.numeroDpto != null)
                    { queryCantidadTotalREgistros += " and v.numero_dpto LIKE '%' + @parametroNumero_dpto + '%'"; }
                    if (request.numeroOf != 0 && request.numeroOf != null)
                    { queryCantidadTotalREgistros += " and v.numero_of LIKE '%' + @parametroNumero_of + '%'"; }
                    if (request.numeroFf != 0 && request.numeroFf != null)
                    { queryCantidadTotalREgistros += " and v.numero_ff LIKE '%' + @parametroNumero_ff + '%'"; }
                    if (!string.IsNullOrEmpty(request.fundamentacion))
                    { queryCantidadTotalREgistros += " and v.fundamentacion LIKE '%' + @parametroFundamentacion + '%'"; }



                }

                query += @"  GROUP BY 
                            v.CODIGO_VERSION,
                            v.CODIGO_OBJETO_GASTO,
                            v.CODIGO_ANTEPROYECTO_OBJETO, 
                            v.CODIGO_ORGANISMO_FINANCIADOR,
                            v.CODIGO_FUENTE_FINANCIAMIENTO, 
                            v.NUMERO_VERSION,	
                            v.ejercicio,
                            v.codigo_circunscripcion_origen,
                            v.nombre_circunscripcion,
                            v.numero_og, 
                            v.numero_ff, 
                            v.numero_of,
                            v.numero_dpto,
                            v.fundamentacion,
                            v.estado,
                            v.codigo_configuracion_presupuestaria 
                        ";

                queryCantidadTotalREgistros += @"  GROUP BY 
                            v.CODIGO_VERSION,
                            v.CODIGO_OBJETO_GASTO, 
                            v.CODIGO_ANTEPROYECTO_OBJETO,
                            v.CODIGO_ORGANISMO_FINANCIADOR,
                            v.CODIGO_FUENTE_FINANCIAMIENTO, 
                            v.NUMERO_VERSION,	
                            v.ejercicio,
                            v.codigo_circunscripcion_origen,
                            v.nombre_circunscripcion,
                            v.numero_og, 
                            v.numero_ff, 
                            v.numero_of,
                            v.numero_dpto,
                            v.fundamentacion,
                            v.estado,
                            v.codigo_configuracion_presupuestaria 
                        ";

                if (string.IsNullOrEmpty(request.terminoDeBusqueda))
                {
                    query += @" HAVING ( ";
                    int contador = 0;
                  
                    if (request.codigoVersion != 0 && request.codigoVersion != null)
                    { query += " v.NUMERO_VERSION LIKE '%' + @parametroCodigoVersion + '%'"; contador++; }

                    if (request.presupuestoInicial != "0" && request.presupuestoInicial != null)
                    {
                        if (contador != 0) { query += "and "; }
                        query += " SUM(v.presupuesto_inicial)  LIKE '%' + REPLACE(@parametroPresupuestoInicial, '.', '') + '%'"; contador++;                    }
                    if (request.presupuestoVigente != "0" && request.presupuestoVigente != null)
                    {
                        if (contador != 0) { query += "and "; }
                        query += "  SUM(v.presupuesto_vigente) LIKE '%' + REPLACE(@parametroPresupuestoVigente, '.', '') + '%'"; contador++;
                    }

                    if (request.modificaciones != "0" && request.modificaciones != null)
                    {  if (contador != 0) { query += "and "; }
                        query += "  SUM(v.modificaciones) LIKE '%' + REPLACE(@parametroModificaciones, '.', '') + '%'"; contador++;
                    }

                    if (request.proyectoPresupuesto != "0" && request.proyectoPresupuesto != null)
                    {
                        if (contador != 0) { query += "and "; }
                        query += " SUM(v.proyecto_presupuesto) LIKE '%' + REPLACE(@parametroProyectoPresupuesto, '.', '')  +  '%'"; contador++;
                    }
                    if (request.diferencia != "0" && request.diferencia != null)
                    {
                        if (contador != 0) { query += "and "; }
                        query += " SUM(v.proyecto_presupuesto - v.presupuesto_vigente) LIKE '%' +REPLACE( @parametroDiferencia, '.', '') + '%'"; contador++;
                    }

                    if (request.porcentaje != "0" && request.porcentaje != null)
                    {
                        if (contador != 0) { query += "and "; }
                        query += @" ROUND(
                            CASE
                                WHEN SUM(v.presupuesto_vigente) <> 0 THEN
                                    (CAST(SUM(v.proyecto_presupuesto - v.presupuesto_vigente) AS FLOAT) / CAST(SUM(v.presupuesto_vigente) AS FLOAT) * 100)
                                ELSE
                                    0
                            END, 0
                        ) LIKE '%' + REPLACE(@parametroPorcentaje, '.', '')  + '%'"; contador++;
                    }

                    query += " )";


                    queryCantidadTotalREgistros += @" HAVING ( ";

                    contador = 0;
                    if (request.codigoVersion != 0 && request.codigoVersion != null)
                    { queryCantidadTotalREgistros += " v.NUMERO_VERSION LIKE '%' + @parametroCodigoVersion + '%'"; contador++; }

                    if (request.presupuestoInicial != "0" && request.presupuestoInicial != null)
                    {
                        if (contador != 0) { queryCantidadTotalREgistros += "and "; }
                        queryCantidadTotalREgistros += " SUM(v.presupuesto_inicial)  LIKE '%' + REPLACE(@parametroPresupuestoInicial, '.', '') + '%'"; contador++; }

                    if (request.presupuestoVigente != "0" && request.presupuestoVigente != null)
                    {
                        if (contador != 0) { queryCantidadTotalREgistros += "and "; }
                        queryCantidadTotalREgistros += " SUM(v.presupuesto_vigente) LIKE '%' + REPLACE(@parametroPresupuestoVigente, '.', '')  + '%'"; contador++;
                    }
                    if (request.modificaciones != "0" && request.modificaciones != null)
                    {
                        if (contador != 0) { queryCantidadTotalREgistros += "and "; }
                        queryCantidadTotalREgistros += "  SUM(v.modificaciones) LIKE '%' + REPLACE(@parametroModificaciones, '.', '')  + '%'"; contador++;
                    }
                    if (request.proyectoPresupuesto != "0" && request.proyectoPresupuesto != null)
                    {
                        if (contador != 0) { queryCantidadTotalREgistros += "and "; }
                        queryCantidadTotalREgistros += " SUM(v.proyecto_presupuesto) LIKE '%' +REPLACE(@parametroProyectoPresupuesto, '.', '')  + '%'"; contador++;
                    }
                    if (request.diferencia != "0" && request.diferencia != null)
                    {
                        if (contador != 0) { queryCantidadTotalREgistros += "and "; }
                        queryCantidadTotalREgistros += " SUM(v.proyecto_presupuesto - v.presupuesto_vigente) LIKE '%' +REPLACE( @parametroDiferencia, '.', '') + '%'"; contador++;
                    }

                    if (request.porcentaje != "0" && request.porcentaje != null)
                    {
                        if (contador != 0) { queryCantidadTotalREgistros += "and "; }
                        queryCantidadTotalREgistros += " ROUND( CASE  WHEN SUM(v.presupuesto_vigente) <> 0 " +
                            "THEN (CAST(SUM(v.proyecto_presupuesto - v.presupuesto_vigente) AS FLOAT) / CAST(SUM(v.presupuesto_vigente) AS FLOAT) * 100)" +
                            "ELSE 0 END, 0 ) LIKE '%' + REPLACE(@parametroPorcentaje, '.', '')  + '%'"; contador++;
                    }

                    queryCantidadTotalREgistros += " )";
                }
                else
                if (!string.IsNullOrEmpty(request.terminoDeBusqueda))
                {
                    query += @" HAVING 
                            ( 
                                        v.CODIGO_VERSION LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR v.CODIGO_OBJETO_GASTO LIKE '%' + @terminoDeBusqueda + '%'                          
                                        OR v.CODIGO_ORGANISMO_FINANCIADOR LIKE '%' + @terminoDeBusqueda + '%'                                        
                                        OR v.CODIGO_FUENTE_FINANCIAMIENTO LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.NUMERO_VERSION LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.ejercicio LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.codigo_circunscripcion_origen LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.nombre_circunscripcion LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.numero_og LIKE '%' + @terminoDeBusqueda + '%'  
                                        OR v.numero_ff LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.numero_of LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.numero_dpto LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.fundamentacion LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.estado LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.codigo_configuracion_presupuestaria LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR SUM(v.presupuesto_inicial) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR sum(v.modificaciones) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%' 
                                        OR SUM(v.presupuesto_vigente) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR sum(v.proyecto_presupuesto) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR SUM(v.proyecto_presupuesto -v.presupuesto_vigente)  LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR ROUND(
                            CASE 
                                WHEN SUM(v.presupuesto_vigente) <> 0 THEN 
                                    (CAST(SUM(v.proyecto_presupuesto - v.presupuesto_vigente) AS FLOAT) / CAST(SUM(v.presupuesto_vigente) AS FLOAT) * 100)
                                ELSE 
                                    0 
                            END, 0
                        )
										
										LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                    )";

                    queryCantidadTotalREgistros += @" HAVING 
                            ( 
                                        v.CODIGO_VERSION LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR v.CODIGO_OBJETO_GASTO LIKE '%' + @terminoDeBusqueda + '%'                          
                                        OR v.CODIGO_ORGANISMO_FINANCIADOR LIKE '%' + @terminoDeBusqueda + '%'                                        
                                        OR v.CODIGO_FUENTE_FINANCIAMIENTO LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.NUMERO_VERSION LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.ejercicio LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.codigo_circunscripcion_origen LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.nombre_circunscripcion LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.numero_og LIKE '%' + @terminoDeBusqueda + '%'  
                                        OR v.numero_ff LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.numero_of LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.numero_dpto LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.fundamentacion LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.estado LIKE '%' + @terminoDeBusqueda + '%'
                                        OR v.codigo_configuracion_presupuestaria LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR SUM(v.presupuesto_inicial) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR sum(v.modificaciones) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%' 
                                        OR SUM(v.presupuesto_vigente) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR sum(v.proyecto_presupuesto) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR SUM(v.proyecto_presupuesto -v.presupuesto_vigente)  LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'
                                        OR ROUND(
                            CASE 
                                WHEN SUM(v.presupuesto_vigente) <> 0 THEN 
                                    (CAST(SUM(v.proyecto_presupuesto - v.presupuesto_vigente) AS FLOAT) / CAST(SUM(v.presupuesto_vigente) AS FLOAT) * 100)
                                ELSE 
                                    0 
                            END, 0
                        )
										
										LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'  )";
                }
                
                query += @" ORDER BY  v.NUMERO_VERSION ";
                query += @" OFFSET @saltarRegistros ROWS";
                query += @" FETCH NEXT @cantidadRegistros ROWS ONLY";

                              
                queryCantidadTotalREgistros += @" ) as Subquery ";







                // Definición de parámetros
                var parametros = new DynamicParameters();
                parametros.Add("@terminoDeBusqueda", $"%{request.terminoDeBusqueda}%");
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.cantidadRegistros);

                // Añadir los parámetros correspondientes de búsqueda
                parametros.Add("@parametroNumero_og", request.numeroOg.ToString());
                parametros.Add("@parametroNumero_ff", request.numeroFf.ToString());
                parametros.Add("@parametroNumero_of", request.numeroOf.ToString());
                parametros.Add("@parametroModificaciones", request.modificaciones);
                parametros.Add("@parametroNumero_dpto", request.numeroDpto.ToString());
                parametros.Add("@parametroFundamentacion", request.fundamentacion);
                parametros.Add("@parametroPresupuestoInicial", request.presupuestoInicial);
                parametros.Add("@parametroDiferencia", request.diferencia);
                parametros.Add("@parametroCodigoVersion", request.codigoVersion.ToString());
                parametros.Add("@parametroCircunscripcion", request.circunscripcion);
                parametros.Add("@parametroCodigoCircunscripcion", request.codigoConfiguracionPresupuestaria);
                
                parametros.Add("parametroEstado", request.estado);
                parametros.Add("parametroEjercicio", request.ejercicio.ToString());
                parametros.Add("parametroCodigoObjetoGasto", request.codigoObjetoGasto.ToString());                
                parametros.Add("parametroModificaciones", request.modificaciones);
                parametros.Add("parametroPresupuestoVigente", request.presupuestoVigente);
                parametros.Add("parametroProyectoPresupuesto", request.proyectoPresupuesto); 
                parametros.Add("parametroDiferencia", request.diferencia);
                parametros.Add("parametroPorcentaje", request.porcentaje);
                parametros.Add("parametroDescripcionObjeto", request.descripcionObjeto);
                



                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);

                    var resultado = await connection.QueryAsync<AnteproyectoPresupuestarioDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>
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
        public async Task<Datos<IEnumerable<AnteproyectoPresupuestarioCABDTO>>> AnteproyectoPresupuestarioCAB(int pagina, int cantidadRegistros, string? terminoDeBusqueda, int? numeroVersion, int? ejercicio, string? estado)
        {
            _logger.LogInformation("Inicio de Proceso de Actualizar Objeto Version Anteproyecto ");
            int saltarRegistros = (pagina - 1) * cantidadRegistros;

            string query = @" SELECT  
                                    va.NUMERO_VERSION AS numeroVersion,  
                                    r.valor_alfanumerico AS estado, 
                                    va.ejercicio AS ejercicio,  
                                    va.CODIGO_VERSION AS codigoVersion,
                                    va.es_ley AS esLey
                                FROM 
                                    versiones_anteproyectos va 
                                JOIN 
                                    referencias r ON r.valor_decimal = va.ESTADO  
                                JOIN 
                                    tipo_referencias tr ON tr.codigo_tipo_referencia = r.codigo_tipo_referencia 
                                WHERE 
                                    r.dominio_referencia LIKE 'ESTADO_VERSION%' 
                                    AND va.version_consolidado = 1";


            if (!string.IsNullOrEmpty(terminoDeBusqueda))
            {
                query += @"
                                   AND (va.NUMERO_VERSION LIKE '%' + @terminoDeBusqueda + '%'
                                        OR va.CODIGO_VERSION LIKE '%' + @terminoDeBusqueda + '%'
                                        OR r.valor_alfanumerico LIKE '%' + @terminoDeBusqueda + '%'
                                        OR va.ejercicio LIKE '%' + @terminoDeBusqueda + '%'                                                                                                                     
                                       )";
            }
            else
            {
                query += @"";

                if (ejercicio != 0 && ejercicio != null)
                { query += " and va.ejercicio LIKE '%' + @parametroEjercicio + '%'"; }
                if (numeroVersion != 0 && numeroVersion != null)//
                { query += " and va.NUMERO_VERSION LIKE '%' + @parametroCodigoVersion + '%'"; }

                if (!string.IsNullOrEmpty(estado))
                { query += " and r.valor_alfanumerico LIKE '%' + @parametroEstado + '%'"; }
                
            }               

            query += @" order by va.ejercicio DESC, va.NUMERO_VERSION desc OFFSET @saltarRegistros ROWS";
            query += @" FETCH NEXT @cantidadRegistros ROWS ONLY";

            
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    
                    var parametros = new
                    {
                        //usuario = user,
                        pagina,
                        saltarRegistros = saltarRegistros,
                        cantidadRegistros,
                        parametroCodigoVersion= numeroVersion.ToString(),
                        parametroEstado=estado,
                        parametroEjercicio=ejercicio.ToString(),
                        terminoDeBusqueda =terminoDeBusqueda

                    };
                    string queryCantidadTotalREgistros = @"SELECT  COUNT(	va.NUMERO_VERSION) FROM versiones_anteproyectos va 
    JOIN referencias r on r.valor_decimal=va.ESTADO 	
	JOIN tipo_referencias tr ON tr.codigo_tipo_referencia= r.codigo_tipo_referencia 
	WHERE r.dominio_referencia like 'ESTADO_VERSION%' and va.version_consolidado=1";

                    if (!string.IsNullOrEmpty(terminoDeBusqueda))
                    {
                        queryCantidadTotalREgistros += @"
                                   AND (va.NUMERO_VERSION LIKE '%' + @terminoDeBusqueda + '%'
                                        OR va.CODIGO_VERSION LIKE '%' + @terminoDeBusqueda + '%'
                                        OR r.valor_alfanumerico LIKE '%' + @terminoDeBusqueda + '%'
                                        OR va.ejercicio LIKE '%' + @terminoDeBusqueda + '%'                                                                                                                     
                                       )";
                    }
                    else
                    {
                        queryCantidadTotalREgistros += @"";

                        if (ejercicio != 0 && ejercicio != null)//
                        { queryCantidadTotalREgistros += " and va.ejercicio LIKE '%' + @parametroEjercicio + '%'"; }
                        if (!string.IsNullOrEmpty(estado))
                        { queryCantidadTotalREgistros += " and r.valor_alfanumerico LIKE '%' + @parametroEstado + '%'"; }
                        if (numeroVersion != 0 && numeroVersion != null)//
                        { queryCantidadTotalREgistros += " and va.NUMERO_VERSION LIKE '%' + @parametroCodigoVersion + '%'"; }

                    }


                    var resultado = await connection.QueryAsync<AnteproyectoPresupuestarioCABDTO>(query, parametros);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);

                    var listado = new Datos<IEnumerable<AnteproyectoPresupuestarioCABDTO>>
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
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Onjeto Version Anteproyecto de versiones_anteproyectos");
            }


        }
        public async Task<int> AbrirVersionAnteproyecto(int version, int user)
        {

            _logger.LogInformation("Inicio de Proceso de abrir version");
            var Cantidad = 0;

            // Validar si la version esta en estado cerrado.
           
            int usuario;

            using (var connection = this._conexion.CreateSqlConnection())
            {
                string queryFechaCierre = "select c.fecha_cierre from version_fecha_cierre c where c.activo=1";
                string fecha = "";

                var fechaCierre = await connection.QueryAsync<DateTime>(queryFechaCierre);
                if (fechaCierre != null && fechaCierre.Count() != 0)

                {
                    string fechaFormateada = fechaCierre.First().ToString("yyyy-MM-dd HH:mm:ss");

                    fecha = " and FORMAT(GETDATE(), 'yyyy-MM-dd HH:mm:ss') > '" + fechaFormateada + "'";
                }


                string queryAbierta = @"select Top 1 ESTADO as Estado, 
                                    CODIGO_VERSION as CodigoVersion, 
                                    CODIGO_CIRCUNSCRIPCION as CodigoCircunscripcion, 
                                    EJERCICIO  as Ejercicio,
                                    USUARIO_INSERTO as UsuarioInserto,
                                    FECHA_INSERTO as Fecha, 
                                    ES_LEY as EsLey
                                    from versiones_anteproyectos where ESTADO=1 and version_consolidado=1 and es_ley=0 " + //fecha+ "" +
                                    "and (select Top 1  ES_LEY as EsLey from versiones_anteproyectos " +
                                     "where ESTADO=1  order by versiones_anteproyectos.EJERCICIO desc, CODIGO_VERSION desc)=0 order by versiones_anteproyectos.EJERCICIO desc, CODIGO_VERSION desc";

                string query = @"select Top 1 ESTADO as Estado, 
                                    CODIGO_VERSION as CodigoVersion, 
                                    CODIGO_CIRCUNSCRIPCION as CodigoCircunscripcion, 
                                    EJERCICIO  as Ejercicio,
                                    USUARIO_INSERTO as UsuarioInserto,
                                    FECHA_INSERTO as Fecha, 
                                    ES_LEY as EsLey
                                    from versiones_anteproyectos where ESTADO=2 and version_consolidado=1 and es_ley=0 "
                                    /*+fecha*/ + "and (select Top 1  ES_LEY as EsLey from versiones_anteproyectos " +
                                    "where ESTADO=2 and version_consolidado=1 order by versiones_anteproyectos.EJERCICIO desc, CODIGO_VERSION desc)=0 order by versiones_anteproyectos.EJERCICIO desc, CODIGO_VERSION desc";


                var resultadoAbierta = await connection.QueryAsync<VersionAnteproyectoDTO>(queryAbierta);

                if (resultadoAbierta.Count() == 0)
                {
                    var resultado = await connection.QueryAsync<VersionAnteproyectoDTO>(query);


                    if (resultado != null && resultado.Count() != 0)
                    {
                        var resultadoValores = resultado.First();

                        if (resultadoValores.Estado == 2 && resultadoValores.EsLey == 0)
                        {
                            //string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                            //                  WHERE cedula_identidad =@UsuarioUltimaModificacion";

                            //usuario = await connection.QueryFirstOrDefaultAsync<int>(queryUsuario, new { UsuarioUltimaModificacion = user });
                            string queryUltimoValorCodigo = @"SELECT ISNULL(MAX(CODIGO_VERSION),0) FROM versiones_anteproyectos";
                            int ultimoValorCodigodetalle = await connection.ExecuteScalarAsync<int>(queryUltimoValorCodigo);
                            int nuevoCodigoCodigodetalle = ultimoValorCodigodetalle + 1;
                            int valorEsley = 0;

                            //Insertar version
                            string queryAbrir = @"insert into  versiones_anteproyectos  
                                  (ESTADO, CODIGO_VERSION, NUMERO_VERSION, CODIGO_CIRCUNSCRIPCION, EJERCICIO,USUARIO_INSERTO,FECHA_INSERTO,version_consolidado, ES_LEY)
                                  values ( 1, " +
                                                      nuevoCodigoCodigodetalle.ToString() + "," +
                                                      nuevoCodigoCodigodetalle.ToString() + "," +
                                                      resultadoValores.CodigoCircunscripcion.ToString() + "," +
                                                      resultadoValores.Ejercicio.ToString() + "," +
                                                      user + ", GETDATE(), 1" + "," + valorEsley.ToString() + ")";

                            var versionAbrir = await connection.QueryAsync<VersionAnteproyectoDTO>(queryAbrir);


                            //Insertar VERSIONES_ANTEPROYECTOS_OBJETO

                            var sourceQuery1 = "SELECT   CODIGO_VERSION as CodigoVersion, " +
                                    " Codigo_anteproyecto_objeto as CodigoAnteproyectoObjeto, " +
                                    " Codigo_Circunscripcion_Origen as CodigoCircunscripcionOrigen, " +
                                    " Codigo_Configuracion_Presupuestaria as CodigoConfiguracionPresupuestaria , " +
                                    " Codigo_Fuente_Financiamiento as CodigoFuenteFinanciamiento," +
                                    " Codigo_Organismo_Financiador as CodigoOrganismoFinanciador," +
                                    " Evento as Evento," +
                                    " Presupuesto_Inicial as PresupuestoInicial," +
                                    " Modificaciones as Modificaciones," +
                                    " Monto_Planificado as MontoPlanificado " +
                                    " FROM VERSIONES_ANTEPROYECTOS_OBJETOS " +
                                    " where CODIGO_VERSION= " + resultadoValores.CodigoVersion.ToString() +
                                    " order by CODIGO_VERSION, CODIGO_ANTEPROYECTO_OBJETO asc ";

                            var sourceRecords1 = await connection.QueryAsync<VersionAnteproyectoObjeto>(sourceQuery1);

                            string queryUltimoValorVERSIONESANTEPROYECTOSOBJETOS = @"SELECT ISNULL(MAX(CODIGO_ANTEPROYECTO_OBJETO),0) FROM VERSIONES_ANTEPROYECTOS_OBJETOS";
                            int UltimoValorVERSIONESANTEPROYECTOSOBJETO = await connection.ExecuteScalarAsync<int>(queryUltimoValorVERSIONESANTEPROYECTOSOBJETOS);


                            // Modificar y preparar los registros para insertar
                            var destinationRecords1 = new List<VersionAnteproyectoObjeto>();
                            foreach (var record in sourceRecords1)
                            {
                                UltimoValorVERSIONESANTEPROYECTOSOBJETO = UltimoValorVERSIONESANTEPROYECTOSOBJETO + 1;

                                //// Realizar modificaciones en el registro
                                var modifiedRecord = new VersionAnteproyectoObjeto
                                {
                                    CodigoVersion = nuevoCodigoCodigodetalle,
                                    CodigoAnteproyectoObjeto = UltimoValorVERSIONESANTEPROYECTOSOBJETO,
                                    CodigoConfiguracionPresupuestaria = record.CodigoConfiguracionPresupuestaria,
                                    CodigoCircunscripcionOrigen = record.CodigoCircunscripcionOrigen,
                                    CodigoFuenteFinanciamiento = record.CodigoFuenteFinanciamiento,
                                    CodigoOrganismoFinanciador = record.CodigoOrganismoFinanciador,
                                    Evento = record.Evento,
                                    PresupuestoInicial = record.PresupuestoInicial,
                                    Modificaciones = record.Modificaciones,
                                    MontoPlanificado = record.MontoPlanificado,

                                };

                                string QueryCCR = @" select codigo_centro_responsabilidad from configuracion_presupuestaria where codigo_configuracion_presupuestaria = " + record.CodigoConfiguracionPresupuestaria.ToString();
                                string QueryMateria = @" select codigo_materia from configuracion_presupuestaria where codigo_configuracion_presupuestaria = " + record.CodigoConfiguracionPresupuestaria.ToString();
                                int CodigoCCR = await connection.ExecuteScalarAsync<int>(QueryCCR);
                                int CodigoMateria = await connection.ExecuteScalarAsync<int>(QueryMateria);



                                // Insertar registros en VERSIONES_ANTEPROYECTOS_OBJETOS

                                var insertQuery1 = "insert into  VERSIONES_ANTEPROYECTOS_OBJETOS   (CODIGO_VERSION , " +
                                        " Codigo_anteproyecto_objeto , " +
                                        " Codigo_Circunscripcion_Origen , " +
                                        " Codigo_Configuracion_Presupuestaria, " +
                                        " Codigo_Fuente_Financiamiento," +
                                        " Codigo_Organismo_Financiador," +
                                        " Evento," +
                                        " Presupuesto_Inicial," +
                                        " Modificaciones," +
                                        " Monto_Planificado )" +
                            "values (@CodigoVersion, " +
                                    "@CodigoAnteproyectoObjeto," +
                                    "@CodigoCircunscripcionOrigen," +
                                    "@CodigoConfiguracionPresupuestaria," +
                                    "@CodigoFuenteFinanciamiento," +
                                    "@CodigoOrganismoFinanciador," +
                                    "@Evento," +
                                    "@PresupuestoInicial," +
                                    "@Modificaciones," +
                                    "@MontoPlanificado)";

                                await connection.ExecuteAsync(insertQuery1, modifiedRecord);


                                var sourceQuery = "SELECT   CODIGO_VERSION as CodigoVersion, " +
                                    " Codigo_anteproyecto_objeto as CodigoAnteproyectoObjeto, " +
                                    " Codigo_anteproyecto_bien as CodigoAnteproyectoBien, " +
                                    " Numero_bien as NumeroBien , " +
                                    " Descripcion_bien as Descripcion," +
                                    " Cantidad as Cantidad," +
                                    " Valor_unitario as CostoUnitario," +
                                    " Unidad_medida as UnidadMedida," +
                                    " Fundamentacion as fundamentacion," +
                                    " Seleccionado as Seleccionado, " +
                                    " Fecha_inserto as FechaInserto, " +
                                    " Usuario_inserto as UsuarioInserto" +
                                    " FROM VERSIONES_ANTEPROYECTOS_BIENES " +
                                    " where CODIGO_VERSION= " + resultadoValores.CodigoVersion.ToString() + "and Codigo_anteproyecto_objeto = " + record.CodigoAnteproyectoObjeto +
                                    " order by CODIGO_VERSION, CODIGO_ANTEPROYECTO_OBJETO, CODIGO_ANTEPROYECTO_BIEN asc ";

                                var sourceRecords = await connection.QueryAsync<VersionAnteproyectoBien>(sourceQuery);

                                string queryUltimoValorVERSIONESANTEPROYECTOSBIENES = @"SELECT ISNULL(MAX(CODIGO_ANTEPROYECTO_BIEN),0) FROM VERSIONES_ANTEPROYECTOS_BIENES";
                                int UltimoValorVERSIONESANTEPROYECTOSBIENES = await connection.ExecuteScalarAsync<int>(queryUltimoValorVERSIONESANTEPROYECTOSBIENES);
                                int nuevoUltimoValorVERSIONESANTEPROYECTOSBIENES = ultimoValorCodigodetalle;


                                // Modificar y preparar los registros para insertar
                                var destinationRecords = new List<VersionAnteproyectoBien>();
                                foreach (var record1 in sourceRecords)
                                {

                                    UltimoValorVERSIONESANTEPROYECTOSBIENES = UltimoValorVERSIONESANTEPROYECTOSBIENES + 1;

                                    //// Realizar modificaciones en el registro
                                    var modifiedRecord1 = new VersionAnteproyectoBien
                                    {
                                        CodigoVersion = nuevoCodigoCodigodetalle,
                                        CodigoAnteproyectoObjeto = UltimoValorVERSIONESANTEPROYECTOSOBJETO,
                                        Ejercicio = record1.Ejercicio,
                                        CodigoCircunscripcion = record1.CodigoCircunscripcion,
                                        NumeroBien = record1.NumeroBien,
                                        Descripcion = record1.Descripcion,
                                        CostoUnitario = record1.CostoUnitario,
                                        Cantidad = record1.Cantidad,
                                        UnidadMedida = record1.UnidadMedida,
                                        fundamentacion = record1.fundamentacion,
                                        Seleccionado = record1.Seleccionado,
                                        CodigoAnteproyectoBien = UltimoValorVERSIONESANTEPROYECTOSBIENES,
                                        UsuarioInserto = user,
                                        FechaInserto = DateTime.Today,
                                        FechaModificacion = DateTime.Today,
                                        codigoCentroResponsabilidad = CodigoCCR,
                                        codigoMateria = CodigoMateria,
                                        UsuarioModificacion = record1.UsuarioModificacion
                                    };


                                    // Insertar registros en VERSIONES_ANTEPROYECTOS_BIENES
                                    var insertQuery = "insert into  VERSIONES_ANTEPROYECTOS_BIENES  (Codigo_version," +
                                        "Codigo_anteproyecto_objeto," +
                                        "Codigo_anteproyecto_bien," +
                                        "Numero_bien," +
                                        "Descripcion_bien," +
                                        "Cantidad," +
                                        "Valor_unitario," +
                                        "Unidad_medida, " +
                                        "Fundamentacion," +
                                        "Seleccionado," +
                                        "Fecha_inserto," +
                                        "Usuario_inserto)" +
                                "values (@CodigoVersion, " +
                                        "@CodigoAnteproyectoObjeto," +
                                        "@CodigoAnteproyectoBien," +
                                        "@NumeroBien," +
                                        "@Descripcion," +
                                        "@Cantidad," +
                                        "@CostoUnitario," +
                                        "@UnidadMedida," +
                                        "@fundamentacion," +
                                        "@Seleccionado," +
                                        "GETDATE()," +
                                        "@UsuarioInserto)";

                                    await connection.ExecuteAsync(insertQuery, modifiedRecord1);



                                }


                                //Insertar en versiones_anteproyectos_conf_bienes
                                string sourceQueryConfBienes = @" SELECT
                                                                    codigo_anteproyecto_bien AS CodigoAnteproyectoBien,
                                                                    codigo_anteproyecto_objeto AS CodigoAnteproyectoObjeto,
                                                                    codigo_version as CodigoVersion,
                                                                    numero_bien as NumeroBien,
                                                                    descripcion_bien as Descripcion,
                                                                    cantidad as Cantidad,
                                                                    valor_unitario as CostoUnitario,
                                                                    usuario_inserto as UsuarioInserto,
                                                                    fecha_inserto as FechaInserto,
                                                                    usuario_modificacion as UsuarioModificacion,
                                                                    fecha_modificacion as FechaModificacion,
                                                                    unidad_medida as UnidadMedida,
                                                                    fundamentacion as Fundamentacion,
                                                                    seleccionado as Seleccionado,
                                                                    codigo_centro_responsabilidad as CodigoCentroResponsabilidad,
                                                                    codigo_materia as CodigoMateria
                                                                FROM
                                                                    versiones_anteproyectos_conf_bienes
                                                                WHERE codigo_anteproyecto_objeto = " + record.CodigoAnteproyectoObjeto.ToString();
                                var sourceRecordsConfBienes = await connection.QueryAsync<VersionAnteproyectoBien>(sourceQueryConfBienes);

                                string queryUltimoValorConfBienes = @"SELECT ISNULL(MAX(CODIGO_ANTEPROYECTO_BIEN),0) FROM versiones_anteproyectos_conf_bienes";
                                int UltimoValorConfBienes = await connection.ExecuteScalarAsync<int>(queryUltimoValorConfBienes);
                                int nuevoUltimoValorConfBienes = UltimoValorConfBienes;

                                // Modificar y preparar los registros para insertar

                                foreach (var record2 in sourceRecordsConfBienes)
                                {

                                    nuevoUltimoValorConfBienes = nuevoUltimoValorConfBienes + 1;

                                    //// Realizar modificaciones en el registro
                                    var modifiedRecord2 = new VersionAnteproyectoBien
                                    {
                                        CodigoVersion = nuevoCodigoCodigodetalle,
                                        CodigoAnteproyectoObjeto = UltimoValorVERSIONESANTEPROYECTOSOBJETO,
                                        Ejercicio = record2.Ejercicio,
                                        CodigoCircunscripcion = record2.CodigoCircunscripcion,
                                        NumeroBien = record2.NumeroBien,
                                        Descripcion = record2.Descripcion,
                                        CostoUnitario = record2.CostoUnitario,
                                        Cantidad = record2.Cantidad,
                                        UnidadMedida = record2.UnidadMedida,
                                        fundamentacion = record2.fundamentacion,
                                        Seleccionado = record2.Seleccionado,
                                        CodigoAnteproyectoBien = nuevoUltimoValorConfBienes,
                                        UsuarioInserto = user,
                                        FechaInserto = DateTime.Today,
                                        FechaModificacion = DateTime.Today,
                                        codigoCentroResponsabilidad = CodigoCCR,
                                        codigoMateria = CodigoMateria,
                                        UsuarioModificacion = record2.UsuarioModificacion
                                    };



                                    string QueryBienesConf = @"INSERT INTO versiones_anteproyectos_conf_bienes 
                        (codigo_anteproyecto_objeto, codigo_anteproyecto_bien, codigo_version, numero_bien, 
                        descripcion_bien, cantidad, valor_unitario, usuario_inserto, fecha_inserto, usuario_modificacion, 
                        fecha_modificacion, unidad_medida, fundamentacion, seleccionado, codigo_centro_responsabilidad, codigo_materia) 
                        VALUES (@CodigoAnteproyectoObjeto, @CodigoAnteproyectoBien, @CodigoVersion, @NumeroBien, @Descripcion, 
                        @Cantidad, @CostoUnitario, @UsuarioInserto, @FechaInserto, @UsuarioModificacion, @FechaModificacion,
                        @UnidadMedida, @Fundamentacion, @Seleccionado, @codigoCentroResponsabilidad, @codigoMateria)";


                                    await connection.ExecuteAsync(QueryBienesConf, modifiedRecord2);

                                }



                                Cantidad = 1;

                            }
                        }
                    }

                }
            }
             
            return Cantidad;
        }

        public async Task<int> EliminarObjetodeAnteproyectoyBienes(int version, int codigoVersionAnteproyecto, int codigoConfigPresupuestaria)
        {
            _logger.LogInformation("Inicio del Proceso de eliminar objeto de la version de anteproyecto de objetos");

            string queryEliminarBienesConf = @"DELETE FROM versiones_anteproyectos_conf_bienes
                                       WHERE CODIGO_VERSION = @codigoVersion
                                       AND CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto";

            string queryActualizarBienes = @"DELETE from VERSIONES_ANTEPROYECTOS_BIENES
                                                WHERE CODIGO_VERSION = @codigoVersion
                                                AND CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto";

            string queryEliminarObjetos = @"DELETE FROM VERSIONES_ANTEPROYECTOS_OBJETOS
                                              WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto
	                                          AND CODIGO_VERSION = @codigoVersion
	                                          AND CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria";

            string queryContarObjetosConfigPresu = @"select count(CODIGO_ANTEPROYECTO_OBJETO) FROM VERSIONES_ANTEPROYECTOS_OBJETOS
                                              WHERE CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria";

            string queryEliminarConfiguracion = @"DELETE FROM Configuracion_Presupuestaria
                                              WHERE  CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria";

            string queryEliminarVERSIONES_CONTRATOS = @" DELETE FROM VERSIONES_CONTRATOS where CODIGO_VERSION = @codigoVersion 
                                                      and CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto";
            string queryEliminarVERSIONES_ANTEPROYECTO_PLANIFICACION = @" DELETE FROM VERSIONES_ANTEPROYECTO_PLANIFICACION where CODIGO_VERSION = @codigoVersion 
                                                      and CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto";
           
            var parametros = new DynamicParameters();
            parametros.Add("@codigoVersionAnteproyecto", codigoVersionAnteproyecto);
            parametros.Add("@codigoVersion", version);
            parametros.Add("@codigoConfigPresupuestaria", codigoConfigPresupuestaria);

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var eliminarBienesConfig = await connection.ExecuteAsync(queryEliminarBienesConf, parametros);

                    var eliminarBienes = await connection.ExecuteAsync(queryActualizarBienes, parametros);                  
                
                    var eliminarContrato = await connection.ExecuteAsync(queryEliminarVERSIONES_CONTRATOS, parametros);

                    var eliminarPlanificacion = await connection.ExecuteAsync(queryEliminarVERSIONES_ANTEPROYECTO_PLANIFICACION, parametros);

                    var eliminarObjetos = await connection.ExecuteAsync(queryEliminarObjetos, parametros);


                    _logger.LogInformation("Fin del Proceso de eliminar objeto de la version de anteproyecto de objetos");

                    return eliminarObjetos;
                }
            }
            catch (Exception ex)
            {
                throw new RegistrosParaSolicitudException("Ocurrió un error al eliminar registros de la tabla de Anteproyecto Bienes y Objetos" + "||-->" + ex.Message + "<--||");
            }
        }

        #endregion

        #region DESARROLLO DE LA INCIDENCIA CSJ-158

        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> ObtenerCentroResponsabilidadReporte(int cedula)
        {

            string query = @"select 
	                        vlcrc.codigo_centro_responsabilidad as Codigo,
	                      CONCAT( vlcrc.codigo_materia ,' - ' ,vlcrc.descripcion_centro_responsabilidad)  as Descripcion
                        from vListaCentrosResponsabilidadPorCircunscripcion vlcrc
                        JOIN vListaUsuariosPorCentrosResponsabilidad u on u.codigo_circunscripcion=vlcrc.codigo_circunscripcion
                        where u.cedula_identidad in (@CedulaUsuario)";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string queryCantidadTotalREgistros = @"  SELECT Count(	vlcrc.codigo_centro_responsabilidad) 
from vListaCentrosResponsabilidadPorCircunscripcion vlcrc
JOIN vListaUsuariosPorCentrosResponsabilidad u on u.codigo_circunscripcion=vlcrc.codigo_circunscripcion
where u.cedula_identidad in (@CedulaUsuario)";

                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query, new { CedulaUsuario = cedula.ToString() } );


                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, new { CedulaUsuario = cedula.ToString() });
                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gast");
            }
        }
        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCentroResponsabilidad(string codigoCircunscripcion)
        {   

            string query = @" select ccr.codigo_centro_responsabilidad as Codigo,
	                               ccr.descripcion as Descripcion

                            from  administracion_pj.dbo.catalogo_centros_responsabilidad ccr
                            where ccr.codigo_circunscripcion = @codigoCircuncripcion";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {      
                    string queryCantidadTotalREgistros = @"  SELECT count(ccr.codigo_centro_responsabilidad) 
                           from administracion_pj.dbo.catalogo_centros_responsabilidad ccr
                            where ccr.codigo_circunscripcion = @codigoCircuncripcion";          
                                       
                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query, new { codigoCircuncripcion = int.Parse(codigoCircunscripcion) }); 
                    

                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, new { codigoCircuncripcion = int.Parse(codigoCircunscripcion) }); 
                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };           
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gast");
            }
        }
        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGDependenciaCircuscripcion(string user)
        {
            string query = @" select 	vcr.nombre_circunscripcion as Descripcion, vcr.codigo_circunscripcion as Codigo
            from vListaUsuariosPorCentrosResponsabilidad vucr
            join vListaCentrosResponsabilidadPorCircunscripcion vcr  
            on vcr.codigo_centro_responsabilidad=vucr.codigo_centro_responsabilidad
            and vucr.cedula_identidad = @usuario group by vcr.nombre_circunscripcion , vcr.codigo_circunscripcion order by vcr.nombre_circunscripcion desc ";


            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string usuario;
                    string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad =@UsuarioUltimaModificacion";

                    usuario = await connection.QueryFirstOrDefaultAsync<string>(queryUsuario, new { UsuarioUltimaModificacion = user });

                    var parametros = new
                    {
                        usuario = user,

                    };
                    string queryCantidadTotalREgistros = @"SELECT COUNT(DISTINCT vcr.nombre_circunscripcion) 
                             from vListaUsuariosPorCentrosResponsabilidad vucr
                             join vListaCentrosResponsabilidadPorCircunscripcion vcr  
                             on vcr.codigo_centro_responsabilidad=vucr.codigo_centro_responsabilidad
                             and vucr.cedula_identidad = @usuario ";

                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query, parametros);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);

                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gast");
            }


        }
        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGMateria(string user)
        {    
            string query = @" select distinct vucr.codigo_materia as Codigo, vucr.descripcion_materia as Descripcion
                             from vListaUsuariosPorCentrosResponsabilidad vucr
                             join usuarios_poder_judicial upj
                             on upj.codigo_usuario = vucr.codigo_usuario";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
          
                    string queryCantidadTotalREgistros = @"SELECT  COUNT(DISTINCT vucr.descripcion_materia)                           
                             from vListaUsuariosPorCentrosResponsabilidad vucr
                             join usuarios_poder_judicial upj
                             on upj.codigo_usuario = vucr.codigo_usuario";
                  
                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros);
                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

               
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gast");
            }

        }
        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarCircuscripcion(string user)
        {
            string query = @" select codigo_circunscripcion as Codigo,nombre_circunscripcion  as Descripcion
                                from circunscripciones 
                                where codigo_circunscripcion not in (-1,0) ";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {

                    string queryCantidadTotalREgistros = @"SELECT  COUNT(DISTINCT codigo_circunscripcion)                           
                              from circunscripciones 
                                where codigo_circunscripcion not in (-1,0) ";

                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros);
                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };


                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener los datos de Circunscripciones");
            }

        }

        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGDepartamento(string codigoCentroResponsabilidad)
        {
            string query = @" select 	distinct 
		                            vcr.codigo_departamento as Codigo,
		                            vcr.descripcion_departamento as Descripcion

                            from vListaCentrosResponsabilidadPorCircunscripcion vcr  
                            where vcr.codigo_centro_responsabilidad = @codigoCentro ";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {                                 
                    var parametros = new
                    {
                        codigoCentro = int.Parse(codigoCentroResponsabilidad)

                    };                  

                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query, parametros);
                    
                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = 0
                    };
                    
                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }

        }
        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOG(int ejercicio)
        {
            string query = @" SELECT DISTINCT
                                 vlb.numero_objeto_gasto+'-'+ vlb.descripcion_objeto_gasto  as Descripcion,
                                 vlb.codigo_objeto_gasto as Codigo

                            from vListaBienesPrioritarios vlb  
                            where vlb.ejercicio in (@ejercicio - 1,0)
                            And vlb.activo = 1 
                            order by Descripcion asc ";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {                            
                    var parametros = new
                    {
                        ejercicio = ejercicio,

                    };
                    string queryCantidadTotalREgistros = @"SELECT SUM(Counts) AS TotalCount
                                        FROM (
                                            SELECT COUNT(DISTINCT(vlb.numero_objeto_gasto + '-' + vlb.descripcion_objeto_gasto)) AS Counts
                                            FROM vListaBienesPrioritarios vlb
                                            WHERE vlb.origen = 'BIEN'
                                              AND vlb.ejercicio = (@ejercicio - 1) 
                                              AND vlb.activo = 1    
                                            UNION ALL    
                                            SELECT COUNT(DISTINCT(vlb.numero_objeto_gasto + '-' + vlb.descripcion_objeto_gasto)) AS Counts
                                            FROM vListaBienesPrioritarios vlb
                                            WHERE vlb.origen = 'SERVICIO'
                                              AND vlb.activo = 1
                                        ) AS SubQuery";

                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query, parametros);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);
                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener los datos del Objeto de Gasto");
            }


        }
        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCodigoOrganismoFinanciador()
        {
            string query = @"select codigo_organismo_financiador as Codigo, numero_organismo_financiador+'-'+descrip_organismo_financiador as Descripcion
            from organismo_financiador 
            where codigo_organismo_financiador not in(-1)
            order by numero_organismo_financiador+descrip_organismo_financiador asc ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {                    
                    string queryCantidadTotalREgistros = @"select Count(numero_organismo_financiador+'-'+descrip_organismo_financiador)
                                from organismo_financiador";

                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros);
                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }


        }
        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCodigoFuenteFinanciamiento()
        {
            string query = @"select codigo_fuente_financiamiento as Codigo, numero_fuente_financiamiento +'-'+ descrip_fuente_financiamiento as Descripcion
            from fuente_financiamiento 
            where codigo_fuente_financiamiento not in(-1)
            order by numero_fuente_financiamiento +'-'+ descrip_fuente_financiamiento asc";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string queryCantidadTotalREgistros = @"select Count(numero_fuente_financiamiento +'-'+ descrip_fuente_financiamiento)
                                 from fuente_financiamiento";
                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros);
                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }


        }
        public async Task<Datos<IEnumerable<ClaseCentroResponsabilidadDTO>>> DatosOGCentroResponsabilidad(int codigoCentroResponsabilidad)
        {
            string query = @" select vcr.descripcion_centro_responsabilidad as CentroResponsabilidad, 
                                vcr.numero_programa + '-' + vcr.descripcion_programa as Programa, 
                                vcr.numero_actividad + '-' + vcr.descripcion_actividad as Actividad,
                                vcr.numero_departamento as Departamento, 
                                vcr.numero_tipo_presupuesto as NumeroPresupuesto, 
                                vcr.descripcion_tipo_presupuesto as TipoPresupuesto, 
                                vcr.codigo_centro_responsabilidad as CodigoCentroResponsabilidad, 
                                vcr.codigo_programa as CodigoPrograma, 
                                vcr.codigo_actividad as CodigoActividad, 
                                vcr.codigo_departamento as CodigoDepartamento, 
                                vcr.codigo_tipo_presupuesto as CodigoTipoPresupuesto

                              from vListaCentrosResponsabilidadPorCircunscripcion vcr where
                                vcr.codigo_centro_responsabilidad = @CodigoCentroResponsabilidad

                                group by vcr.descripcion_centro_responsabilidad,
                                vcr.numero_programa + '-' + vcr.descripcion_programa , 
                                vcr.numero_actividad + '-' + vcr.descripcion_actividad ,
                                vcr.numero_departamento , 
                                vcr.numero_tipo_presupuesto , 
                                vcr.descripcion_tipo_presupuesto,
                                vcr.codigo_centro_responsabilidad,
                                vcr.codigo_programa, 
                                vcr.codigo_actividad, 
                                vcr.codigo_departamento,
                                vcr.codigo_tipo_presupuesto ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new
                    {
                        CodigoCentroResponsabilidad = codigoCentroResponsabilidad,

                    };
                    string queryCantidadTotalREgistros = @"SELECT COUNT(DISTINCT(vcr.descripcion_centro_responsabilidad) )                                
                              from vListaCentrosResponsabilidadPorCircunscripcion vcr where
                                vcr.codigo_centro_responsabilidad = @CodigoCentroResponsabilidad";

                    var resultado = await connection.QueryAsync<ClaseCentroResponsabilidadDTO>(query, parametros);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);
                    var listado = new Datos<IEnumerable<ClaseCentroResponsabilidadDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }
            
        }
        public async Task<Datos<IEnumerable<ClaseDatosOGGrupoSubGrupoDTO>>> DatosOGGrupoSubGrupo(int codigoObjetoGasto, int ejercicio)
        {        
            string query = @" select distinct
                                (vlb.numero_grupo+'-'+vlb.descripcion_grupo)  as Grupo, 
                                (vlb.numero_subgrupo+'-'+vlb.descripcion_subgrupo) as SubGrupo, 
                                vlb.codigo_grupo as CodigoGrupo,
                                vlb.codigo_subgrupo as CodigoSubgrupo 
                                from 	vListaBienesPrioritarios vlb                                     
                                where vlb.codigo_objeto_gasto = @codigo
                                and vlb.ejercicio in (@Ejercicio - 1,0)
 ";
            string queryCantidadTotalRegistros = @"select 
                        COUNT(DISTINCT(vlb.numero_grupo + '-' + vlb.descripcion_grupo)) AS Counts
                        FROM vListaBienesPrioritarios vlb 
                        where vlb.codigo_objeto_gasto = @codigo
                        and vlb.ejercicio in (@Ejercicio - 1,0)";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new
                    {
                        codigo = codigoObjetoGasto,
                        ejercicio= ejercicio

                    };

                    var resultado = await connection.QueryAsync<ClaseDatosOGGrupoSubGrupoDTO>(query, parametros);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);
                    var listado = new Datos<IEnumerable<ClaseDatosOGGrupoSubGrupoDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }

        }
        public async Task<Datos<IEnumerable<ClaseDatosOGBienesprioritariosDTO>>> DatosOGBienesprioritarios(int version,int codigoAnteproyectoObjeto)
        {
            string query = @"SELECT numero_bien as Codigo, 
      		                     descripcion_bien as Descripcion ,
      		                     valor_unitario as MontoUnitario ,
     		                     cantidad ,  
                                 valor_unitario* cantidad as MontoTotal ,
                                 Fundamentacion 
                                  FROM versiones_anteproyectos_bienes 
                                  where CODIGO_VERSION=@version
                                  and CODIGO_ANTEPROYECTO_OBJETO=@codigo ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new
                    {
                        codigo = codigoAnteproyectoObjeto,
                        version = version

                    };
                    string queryCantidadTotalREgistros = @"SELECT Count(numero_bien)  
                                  FROM versiones_anteproyectos_bienes 
                                  where 	CODIGO_VERSION=@version
                                  and CODIGO_ANTEPROYECTO_OBJETO=@codigo ";


                    var resultado = await connection.QueryAsync<ClaseDatosOGBienesprioritariosDTO>(query, parametros);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);
                    var listado = new Datos<IEnumerable<ClaseDatosOGBienesprioritariosDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }

        }
        public async Task<CantidadTotalGenericaDTO> DatosOGBienesMonto(int version, int codigoAnteproyectoObjeto)
        {
                            string query = @"SELECT   
                    SUM(valor_unitario * cantidad) AS MontoTotal,  Sum(cantidad) as CantidadTotal
                FROM versiones_anteproyectos_bienes 
                WHERE CODIGO_VERSION = @version
                AND CODIGO_ANTEPROYECTO_OBJETO = @codigo";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new
                    {
                        codigo = codigoAnteproyectoObjeto,
                        version = version

                    };
                    

                    var resultado = await connection.QueryFirstOrDefaultAsync<CantidadTotalGenericaDTO>(query, parametros);
                   

                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }

        }
        public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> BuscarBienesPrioritarios(int codigoCatalogo)
        {
            string query = @" SELECT numero_catalogo as codigo, descripcion_catalogo as descripcion
                                     FROM vListaBienesPrioritarios 
  			                              WHERE codigo_catalogo = @codigo  ";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new
                    {
                        codigo = codigoCatalogo,

                    };
                    string queryCantidadTotalREgistros = @"SELECT COUNT(numero_catalogo)
                                     FROM vListaBienesPrioritarios 
  			                              WHERE codigo_catalogo = @codigo ";

                    var resultado = await connection.QueryAsync<ClaseGenericaCodigoDescripDTO>(query, parametros);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);
                    var listado = new Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }

        }
        public async Task<ClaseLlavePrimaria> AgregarConfiguracionPresupuestaria(ClaseConfiguracionPresupuestaria configuracion)
        {
            ClaseLlavePrimaria ClaseLlavePrimaria = new ClaseLlavePrimaria();
            if (configuracion.CodigoAnteproyectoObjeto != 0)
            {
                string queryVersionAntProyObjActualizar = @" update  versiones_anteproyectos_objetos  set (
                                                CODIGO_ANTEPROYECTO_OBJETO
                                              , CODIGO_VERSION
                                              , CODIGO_CONFIGURACION_PRESUPUESTARIA
                                              , CODIGO_FUENTE_FINANCIAMIENTO
                                              , CODIGO_ORGANISMO_FINANCIADOR
                                              , EVENTO
                                              , PRESUPUESTO_INICIAL
                                              , MODIFICACIONES
                                              , MONTO_PLANIFICADO
                                              )
                                         values (@CodigoAnteproyectoObjeto
                                              , @CodigoVersion
                                              , @CodigoConfiguracionPresupuestaria
                                              , @CodigoFuenteFinanciamiento
                                              , @CodigoOrganismoFinanciador
                                              , @Evento
                                              , @PresupuestoInicial
                                              , @Modificaciones                                              
                                               ,@MontoPlanificado
                                              )";
                var parametroObjetoActualizar = new
                {
                    CodigoAnteproyectoObjeto = configuracion.CodigoAnteproyectoObjeto,
                    CodigoObjetoGasto = configuracion.CodigoObjetoGasto,
                    CodigoPrograma = configuracion.CodigoPrograma,
                    CodigoActividad = configuracion.CodigoActividad,
                    CodigoTipoPresupuesto = configuracion.CodigoTipoPresupuesto,
                    CodigoDepartamento = configuracion.CodigoDepartamento,
                    CodigoGrupo = configuracion.CodigoGrupo,
                    CodigoCentroResponsabilidad = configuracion.CodigoCentroResponsabilidad,
                    CodigoMateria = configuracion.CodigoMateria,
                    CodigoSubGrupo = configuracion.CodigoSubGrupo,
                    CodigoVersion = configuracion.CodigoVersion,
                    CodigoConfiguracionPresupuestaria = configuracion.CodigoConfiguracionPresupuestaria,
                    CodigoFuenteFinanciamiento = configuracion.CodigoFuenteFinanciamiento,
                    CodigoOrganismoFinanciador = configuracion.CodigoOrganismoFinanciador,
                    Evento = configuracion.Evento,
                    PresupuestoInicial = configuracion.PresupuestoInicial,
                    Modificaciones = configuracion.Modificaciones,
                    CodigoCircunscripcionOrigen = configuracion.CodigoConfiguracionPresupuestaria,
                    Ejercicio = configuracion.Ejercicio,
                    MontoPlanificado = configuracion.PresupuestoInicial - configuracion.Modificaciones,
                    CodigoSubPrograma = configuracion.CodigoSubPrograma,
                };


                string queryConfigPresuActualizar = @" update  configuracion_presupuestaria  set  ( 
                                   CODIGO_CONFIGURACION_PRESUPUESTARIA,
                                   CODIGO_OBJETO_GASTO
                                  ,CODIGO_PROGRAMA
                                  ,CODIGO_ACTIVIDAD
                                  ,CODIGO_TIPO_PRESUPUESTO
                                  ,CODIGO_DEPARTAMENTO
                                  ,GRUPO
                                  ,CODIGO_CENTRO_RESPONSABILIDAD
                                  ,CODIGO_MATERIA
                                  ,subgrupo)
	                                 values (  @CodigoConfiguracionPresupuestaria,
                                            @CodigoObjetoGasto, 
                                            @CodigoPrograma, 
                                            @CodigoActividad ,
                                            @CodigoTipoPresupuesto,
                                            @CodigoDepartamento, 
                                            @CodigoGrupo, 
                                            @CodigoCentroResponsabilidad	,
                                            @CodigoMateria,
                                            @CodigoSubGrupo)";

                try
                {
                    using (var connection = this._conexion.CreateSqlConnection())
                    {
                        var resultado4 = await connection.ExecuteAsync(queryVersionAntProyObjActualizar, parametroObjetoActualizar);
                    }

                }
                catch (Exception ex)
                {
                    throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
                }

                ClaseLlavePrimaria.CodigoAnteproyectoObjeto = configuracion.CodigoAnteproyectoObjeto;
                ClaseLlavePrimaria.CodigoVersion = configuracion.CodigoVersion;
                ClaseLlavePrimaria.CodigoConfiguracionPresupuestaria = configuracion.CodigoConfiguracionPresupuestaria;
            }
                if (configuracion.CodigoAnteproyectoBien != 0) 
            {
                string queryVersionAntProyBienesActualizar = @" update  versiones_anteproyectos_bienes  set (
                                                CODIGO_ANTEPROYECTO_BIEN
                                              , CODIGO_ANTEPROYECTO_OBJETO
                                              , CODIGO_VERSION
                                              , NUMERO_BIEN
                                              , DESCRIPCION_BIEN
                                              , CANTIDAD
                                              , VALOR_UNITARIO
                                              , USUARIO_INSERTO
                                              , FECHA_INSERTO
                                              , USUARIO_MODIFICACION
                                              , FECHA_MODIFICACION
                                              , UNIDAD_MEDIDA
                                              , FUNDAMENTACION
                                              )
                                         values (@CodigoAnteproyectoBien
                                              , @CodigoAnteproyectoObjeto
                                              , @CodigoVersion
                                              , @NumeroBien
                                              , @DescripcionBien
                                              , @Cantidad
                                              , @ValorUnitario
                                              , @UsuarioInserto
                                              , GETDATE()                                              
                                              , @UsuarioInserto
                                              , GETDATE() 
                                              , @UnidadMedida
                                              , @Fundamentacion
                                              )";



                try
                {
                    var parametroBien = new
                    {
                        CodigoAnteproyectoBien = configuracion.CodigoAnteproyectoBien,
                        CodigoAnteproyectoObjeto = configuracion.CodigoAnteproyectoObjeto,
                        CodigoObjetoGasto = configuracion.CodigoObjetoGasto,
                        CodigoVersion = configuracion.CodigoVersion,
                        NumeroBien = configuracion.NumeroBien,
                        DescripcionBien = configuracion.DescripcionBien,
                        Cantidad = configuracion.Cantidad,
                        ValorUnitario = configuracion.ValorUnitario,
                        UsuarioInserto = configuracion.UsuarioInserto,
                        UnidadMedida = configuracion.UnidadMedida,
                        Fundamentacion = configuracion.Fundamentacion

                    };

                    using (var connection = this._conexion.CreateSqlConnection())
                    {
                        var resultado5 = await connection.ExecuteAsync(queryVersionAntProyBienesActualizar, parametroBien);
                    }

                }
                catch (Exception ex)
                {
                    throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
                }

                ClaseLlavePrimaria.CodigoAnteproyectoObjeto = configuracion.CodigoAnteproyectoObjeto;
                ClaseLlavePrimaria.CodigoVersion = configuracion.CodigoVersion;
                ClaseLlavePrimaria.CodigoConfiguracionPresupuestaria = configuracion.CodigoConfiguracionPresupuestaria;

            }

            else if (configuracion.CodigoAnteproyectoBien == 0)
            {
                

                string queryConfigPresu = @" insert into configuracion_presupuestaria ( 
                                   CODIGO_CONFIGURACION_PRESUPUESTARIA,
                                   CODIGO_OBJETO_GASTO
                                  ,CODIGO_PROGRAMA
                                  ,CODIGO_ACTIVIDAD
                                  ,CODIGO_TIPO_PRESUPUESTO
                                  ,CODIGO_DEPARTAMENTO
                                  ,GRUPO
                                  ,CODIGO_CENTRO_RESPONSABILIDAD
                                  ,CODIGO_MATERIA
                                  ,subgrupo)
	                                 values (  @CodigoConfiguracionPresupuestaria,
                                            @CodigoObjetoGasto, 
                                            @CodigoPrograma, 
                                            @CodigoActividad ,
                                            @CodigoTipoPresupuesto,
                                            @CodigoDepartamento, 
                                            @CodigoGrupo, 
                                            @CodigoCentroResponsabilidad	,
                                            @CodigoMateria,
                                            @CodigoSubGrupo)";

                string queryVersionAntProyObj = @" insert into versiones_anteproyectos_objetos (
                                                CODIGO_ANTEPROYECTO_OBJETO
                                              , CODIGO_VERSION
                                              , CODIGO_CONFIGURACION_PRESUPUESTARIA
                                              , CODIGO_FUENTE_FINANCIAMIENTO
                                              , CODIGO_ORGANISMO_FINANCIADOR
                                              , EVENTO
                                              , PRESUPUESTO_INICIAL
                                              , MODIFICACIONES
                                              , MONTO_PLANIFICADO
                                              )
                                         values (@CodigoAnteproyectoObjeto
                                              , @CodigoVersion
                                              , @CodigoConfiguracionPresupuestaria
                                              , @CodigoFuenteFinanciamiento
                                              , @CodigoOrganismoFinanciador
                                              , @Evento
                                              , @PresupuestoInicial
                                              , @Modificaciones                                              
                                               ,@MontoPlanificado
                                              )";

                string queryModificaciones = @"select dbo.monto_reprogramado (@Ejercicio,@CodigoTipoPresupuesto) as monto_reprogramado 
                                                from presupuesto_gastos where ejercicio= @Ejercicio
                                                and codigo_departamento=@CodigoDepartamento 
                                                and codigo_tipo_presupuesto=@CodigoTipoPresupuesto
                                                and codigo_objeto_gasto=@CodigoObjetoGasto
                                                and codigo_fuente_financiamiento=@CodigoFuenteFinanciamiento
                                                and codigo_organismo_financiador=@CodigoOrganismoFinanciador
                                                and codigo_subprograma= @CodigoSubPrograma";

                string queryVersionAntProyBienes = @" insert into versiones_anteproyectos_bienes (
                                                CODIGO_ANTEPROYECTO_BIEN
                                              , CODIGO_ANTEPROYECTO_OBJETO
                                              , CODIGO_VERSION
                                              , NUMERO_BIEN
                                              , DESCRIPCION_BIEN
                                              , CANTIDAD
                                              , VALOR_UNITARIO
                                              , USUARIO_INSERTO
                                              , FECHA_INSERTO
                                              , USUARIO_MODIFICACION
                                              , FECHA_MODIFICACION
                                              , UNIDAD_MEDIDA
                                              , FUNDAMENTACION
                                              )
                                         values (@CodigoAnteproyectoBien
                                              , @CodigoAnteproyectoObjeto
                                              , @CodigoVersion
                                              , @NumeroBien
                                              , @DescripcionBien
                                              , @Cantidad
                                              , @ValorUnitario
                                              , @UsuarioInserto
                                              , GETDATE()                                              
                                              , @UsuarioInserto
                                              , GETDATE() 
                                              , @UnidadMedida
                                              , @Fundamentacion
                                              )";




                try
                {
                    using (var connection = this._conexion.CreateSqlConnection())
                    {
                        string usuario;
                        string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad =@UsuarioUltimaModificacion"
                        ;

                        usuario = await connection.QueryFirstOrDefaultAsync<string>(queryUsuario, new { UsuarioUltimaModificacion = configuracion.UsuarioInserto });

                        string queryCodigoActividad = @"select codigo_actividad from vListaCentrosResponsabilidadPorCircunscripcion where codigo_centro_responsabilidad = @CodigoCentroResponsabilidad and codigo_materia = @CodigoMateria";
                        int CodActividad = await connection.QueryFirstOrDefaultAsync<int>(queryCodigoActividad, new { CodigoCentroResponsabilidad = configuracion.CodigoCentroResponsabilidad, CodigoMateria = configuracion.CodigoMateria });

                        var parametroMonto = new
                        {
                            CodigoObjetoGasto = configuracion.CodigoObjetoGasto,
                            CodigoDepartamento = configuracion.CodigoDepartamento,
                            CodigoFuenteFinanciamiento = configuracion.CodigoFuenteFinanciamiento,
                            CodigoOrganismoFinanciador = configuracion.CodigoOrganismoFinanciador,
                            CodigoTipoPresupuesto = configuracion.CodigoTipoPresupuesto,
                            Ejercicio = configuracion.Ejercicio,
                            CodigoSubPrograma = CodActividad

                        };
                        string queryPresupuestoInicial = @" select monto_presupuesto as MontoPresupuestoInicial
                                from presupuesto_gastos
                                where ejercicio = @Ejercicio
                                and codigo_departamento = @CodigoDepartamento
                                and codigo_tipo_presupuesto = @CodigoTipoPresupuesto
                                and codigo_objeto_gasto = @CodigoObjetoGasto
                                and codigo_fuente_financiamiento = @CodigoFuenteFinanciamiento
                                and codigo_organismo_financiador=@CodigoOrganismoFinanciador
                                and codigo_subprograma= @CodigoSubPrograma";


                        Int64 montoPresupuestoInicial = await connection.QueryFirstOrDefaultAsync<Int64>(queryPresupuestoInicial, parametroMonto);
                        Int64 montoModificaciones = await connection.QueryFirstOrDefaultAsync<Int64>(queryModificaciones, parametroMonto);

                        string queryUltimoValor = @"select  max(CODIGO_CONFIGURACION_PRESUPUESTARIA) from configuracion_presupuestaria";
                        int UltimoValor = await connection.ExecuteScalarAsync<int>(queryUltimoValor);
                        int nuevoUltimoValor = UltimoValor + 1;

                        string queryCodigo = @"select  max(codigo_anteproyecto_objeto) from versiones_anteproyectos_objetos 
                                           where CODIGO_VERSION = " + configuracion.CodigoVersion.ToString() + "and " +
                                               "CODIGO_CONFIGURACION_PRESUPUESTARIA = " + configuracion.CodigoConfiguracionPresupuestaria.ToString();
                        int UltimoValorCodigo = await connection.ExecuteScalarAsync<int>(queryCodigo);
                        int nuevoCodigo = UltimoValorCodigo + 1;
                        var parametroObjeto = new
                        {
                            CodigoAnteproyectoObjeto = nuevoCodigo,
                            CodigoObjetoGasto = configuracion.CodigoObjetoGasto,
                            CodigoPrograma = configuracion.CodigoPrograma,
                            CodigoActividad = CodActividad,//configuracion.CodigoActividad,
                            CodigoTipoPresupuesto = configuracion.CodigoTipoPresupuesto,
                            CodigoDepartamento = configuracion.CodigoDepartamento,
                            CodigoGrupo = configuracion.CodigoGrupo,
                            CodigoCentroResponsabilidad = configuracion.CodigoCentroResponsabilidad,
                            CodigoMateria = configuracion.CodigoMateria,
                            CodigoSubGrupo = configuracion.CodigoSubGrupo,
                            CodigoVersion = configuracion.CodigoVersion,
                            CodigoConfiguracionPresupuestaria = nuevoUltimoValor,
                            CodigoFuenteFinanciamiento = configuracion.CodigoFuenteFinanciamiento,
                            CodigoOrganismoFinanciador = configuracion.CodigoOrganismoFinanciador,
                            Evento = configuracion.Evento,
                            PresupuestoInicial = montoPresupuestoInicial,
                            Modificaciones = montoModificaciones,
                            CodigoCircunscripcionOrigen = configuracion.CodigoCircunscripcionOrigen,
                            Ejercicio = configuracion.Ejercicio,
                            MontoPlanificado = montoPresupuestoInicial - montoModificaciones,
                            CodigoSubPrograma = CodActividad
                        };

                        string queryUltimoBien = @"select  max(CODIGO_ANTEPROYECTO_BIEN) from versiones_anteproyectos_bienes where CODIGO_ANTEPROYECTO_OBJETO= " + nuevoCodigo;
                        int UltimoValorBien = await connection.ExecuteScalarAsync<int>(queryUltimoBien);
                        int nuevoUltimoValorBien = UltimoValorBien + 1;


                        var parametroBien = new
                        {
                            CodigoAnteproyectoBien = nuevoUltimoValorBien,
                            CodigoAnteproyectoObjeto = nuevoCodigo,
                            CodigoObjetoGasto = configuracion.CodigoObjetoGasto,
                            CodigoVersion = configuracion.CodigoVersion,
                            NumeroBien = configuracion.NumeroBien,
                            DescripcionBien = configuracion.DescripcionBien,
                            Cantidad = configuracion.Cantidad,
                            ValorUnitario = configuracion.ValorUnitario,
                            UsuarioInserto = configuracion.UsuarioInserto,
                            UnidadMedida = configuracion.UnidadMedida,
                            Fundamentacion = configuracion.Fundamentacion

                        };


                        ClaseLlavePrimaria.CodigoAnteproyectoObjeto = nuevoCodigo;
                        ClaseLlavePrimaria.CodigoVersion = configuracion.CodigoVersion;
                        ClaseLlavePrimaria.CodigoConfiguracionPresupuestaria = nuevoUltimoValor;//configuracion.CodigoConfiguracionPresupuestaria;
                        if (configuracion.CodigoConfiguracionPresupuestaria == 0)
                        {
                            var resultado1 = await connection.ExecuteAsync(queryConfigPresu, parametroObjeto);
                        }
                        if (configuracion.CodigoAnteproyectoObjeto == 0)
                        {
                            var resultado2 = await connection.ExecuteAsync(queryVersionAntProyObj, parametroObjeto);
                        }
                        if (configuracion.CodigoAnteproyectoBien == 0)
                        {
                            var resultado3 = await connection.ExecuteAsync(queryVersionAntProyBienes, parametroBien);
                        }


                        _logger.LogInformation("Fin de Proceso de Actualizar Estado Version Anteproyecto");

                        //return ClaseLlavePrimaria;
                    }
                }
                catch (Exception ex)
                {
                    throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
                }
            }

            return ClaseLlavePrimaria;

        }
        public async Task<Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>> BuscarBien(int codigoObjetoGasto, int ejercicio,int pagina, int cantidadRegistros, string MontoUnitario, string TerminoDeBusqueda, string Codigo, string Descripcion)
        {
            var filtrosAdicionales = string.Empty;
            int saltarRegistros = (pagina - 1) * cantidadRegistros;
            if (
               !string.IsNullOrEmpty(Codigo) ||
               !string.IsNullOrEmpty(Descripcion) ||
               !string.IsNullOrEmpty(MontoUnitario) ||

               !string.IsNullOrEmpty(TerminoDeBusqueda))
            {
                // Se proporcionaron parámetros de búsqueda, agregar filtros adicionales
                if (!string.IsNullOrEmpty(TerminoDeBusqueda))
                {
                    filtrosAdicionales += @"
                   
                       And (codigo_catalogo LIKE '%' + @terminoDeBusqueda + '%'                       
                        OR descripcion_catalogo LIKE '%' + @terminoDeBusqueda + '%'                       
                        OR COALESCE(CONVERT(BIGINT, valor_unitario), 0) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '')  + '%'
                                          
                    )";
                }
                else
                {
                    filtrosAdicionales += @"                                        
                    AND (@parametroCatalogo IS NULL OR codigo_catalogo LIKE '%' + @parametroCatalogo + '%') 
                    AND (@parametroDescripcionCatalogo IS NULL OR descripcion_catalogo LIKE '%' + @parametroDescripcionCatalogo + '%')                     
                    AND(@parametroMontoUnitario IS NULL  OR  COALESCE(CONVERT(BIGINT, valor_unitario), 0) LIKE '%' + REPLACE(@parametroMontoUnitario, '.', '')  + '%')";
                }
            }

            string query = $@"
                            SELECT DISTINCT
                                codigo_catalogo AS Codigo,
                                descripcion_catalogo AS Descripcion,
                                CONVERT(INT, valor_unitario) AS MontoUnitario,
                                vlb.codigo_unidad_medida AS CodigoUnidadMedida,
                                vlb.descripcion_unidad_medida AS UnidadMedida,
                                vlb.numero_bien AS NumeroBien
                            FROM 
                                vListaBienesPrioritarios vlb
                            WHERE 
                                vlb.ejercicio IN (@ejercicio-1,0)
                                AND vlb.codigo_objeto_gasto = @codigoObjetoGasto
                                {filtrosAdicionales}";

            // Query para obtener los datos paginados
            string queryConPaginacion = query + @"
                                        ORDER BY codigo_catalogo ASC
                                        OFFSET @saltarRegistros ROWS
                                        FETCH NEXT @cantidadRegistros ROWS ONLY";

            //Cantidad de Registros existentes.
            string queryCantidadTotalRegistros = $@"SELECT COUNT(*) FROM ({query}) AS COUNT";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@terminoDeBusqueda", TerminoDeBusqueda);
                    parametros.Add("@saltarRegistros", saltarRegistros);
                    parametros.Add("@cantidadRegistros", cantidadRegistros);
                    parametros.Add("@codigoObjetoGasto", codigoObjetoGasto);
                    parametros.Add("@parametroCatalogo", Codigo);
                    parametros.Add("@parametroDescripcionCatalogo", Descripcion);
                    parametros.Add("@parametroMontoUnitario", MontoUnitario);
                    parametros.Add("@ejercicio", ejercicio);

                    var resultado = await connection.QueryAsync<VersionAnteproyectoBienSoloDTO>(queryConPaginacion, parametros);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);
                    var listado = new Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }

        }
        public async Task<Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>> BuscarBienesGrilla(int version, int codigoAnteproyectoObjeto)
        {
            string query = @" SELECT  	numero_bien as Codigo,
      		                    descripcion_bien as Descripcion ,
      		                    valor_unitario as MontoUnitario , 
                                cantidad as Cantidad, 
                                (valor_unitario* cantidad) as MontoTotal , 
                                fundamentacion 
                                  FROM versiones_anteproyectos_bienes
                                where 	CODIGO_VERSION=@version
                                and CODIGO_ANTEPROYECTO_OBJETO=@codigoAnteproyectoObjeto";

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new
                    {
                        version = version,
                        codigoAnteproyectoObjeto= codigoAnteproyectoObjeto

                    };
                    string queryCantidadTotalREgistros = @"SELECT COUNT(numero_bien)                                
                                  FROM versiones_anteproyectos_bienes
                                where 	CODIGO_VERSION=@version
                                and CODIGO_ANTEPROYECTO_OBJETO=@codigoAnteproyectoObjeto";

                    var resultado = await connection.QueryAsync<VersionAnteproyectoBienSoloDTO>(query, parametros);
                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros, parametros);
                    var listado = new Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }

        }
        public async Task<int> EliminarBienAnteproyecto(int codigoAnteProyectoBien,int codigoObjetoGasto)
        {
          
            string query = @" Delete versiones_anteproyectos_bienes where CODIGO_ANTEPROYECTO_BIEN = @CodigoAnteProyectoBien and codigo_objeto_gasto= @codigoObjetoGasto";
           
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new
                    {
                        codigoObjetoGasto = codigoObjetoGasto,
                        CodigoAnteProyectoBien = codigoAnteProyectoBien,                     

                    };
                    

                    var resultado = await connection.ExecuteScalarAsync<int>(query, parametros);
                   

                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al actualizar los datos de Objeto Gasto");
            }

        }
        public async Task<Datos<IEnumerable<VersionesAnteproyectosDTO>>> ObtenerVersiones()
        {
            _logger.LogInformation("Inicio del Proceso de Obtener Versiones Anteproyectos");
            // Definir la consulta SQL para obtener los valores
            string query = @"select CODIGO_VERSION as CodigoVersion, NUMERO_VERSION as NumeroVersion from VERSIONES_ANTEPROYECTOS where version_consolidado=1";

            using (var connection = this._conexion.CreateSqlConnection())
            {
                string queryCantidadTotalREgistros = @"SELECT COUNT(CODIGO_VERSION)
                                     from VERSIONES_ANTEPROYECTOS where version_consolidado=1";


                var resultado = await connection.QueryAsync<VersionesAnteproyectosDTO>(query);
                var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalREgistros);
                var listado = new Datos<IEnumerable<VersionesAnteproyectosDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalTegistros
                };

                return listado;


               
            }

        }

        public async Task<int> ObtenerVersionAbierta()
        {
            _logger.LogInformation("Inicio del Proceso de Obtener Versiones Anteproyectos");
            // Definir la consulta SQL para obtener los valores
            string query = @"select max(NUMERO_VERSION) as NumeroVersion from VERSIONES_ANTEPROYECTOS where version_consolidado=1 and estado=1";

            using (var connection = this._conexion.CreateSqlConnection())
            {
               


                var resultado = await connection.ExecuteScalarAsync<int>(query);
                
             

                return resultado;



            }

        }



        #endregion

        #region DESARROLLO DE LA INCIDENCIA CSJ-ARCHIVOS

        public async Task<string> PersistirArchivo( int codigoVersion, int usuarioInserto, string dominio, string referencia, string nombre, string extension)
        {
            string result = "";

            try
            {
                //Obtenemos el Token para la gestion
                var token = string.Empty;
                if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Trim();                 
                }

                var request = new HttpRequestMessage(HttpMethod.Post, _gestionArchivos.BaseUrl + _gestionArchivos.PersistirArchivoUrl);
                request.Headers.Add("Authorization", token);  
                var content = new StringContent("{\"referencia\":\"" + referencia + "\"}", null, "application/json");
                request.Content = content;
                string resultado;

                // Enviar la solicitud
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                resultado = await response.Content.ReadAsStringAsync();


                string input = resultado;
                string startPhrase = "\"referencia\":\"";
                string endPhrase = "\",\"nombreOriginal\":";

                try
                {
                    result = ExtractSubstring(input, startPhrase, endPhrase);                    
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }


                if (!string.IsNullOrEmpty(resultado))
                {
                    using (var connection = this._conexion.CreateSqlConnection())
                    {
                        string fileName = nombre;
                        string fileExtension = extension;

                        //Archivos Adjuntos
                        string queryUltimoValor = @"select  max(Codigo_archivo) from ARCHIVOS_ADJUNTOS";
                        int UltimoValor = await connection.ExecuteScalarAsync<int>(queryUltimoValor);
                        int nuevoUltimoValor = UltimoValor + 1;
                        int Codigo_archivo = nuevoUltimoValor;

                        string queryArchivo = @" insert into ARCHIVOS_ADJUNTOS (
                                                codigo_archivo
                                              , referencia
                                              , dominio
                                              , nombre_archivo
                                              , extension                                              
                                              )
                                         values (" + Codigo_archivo + " ,'"
                                                       + result + "' , '"
                                                       + dominio + "' ,'"
                                                       + fileName + "', '"
                                                       + fileExtension + "') ";

                        var resultado1 = await connection.ExecuteAsync(queryArchivo);

                        string usuario;
                        string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad =@UsuarioUltimaModificacion"
                        ;

                        usuario = await connection.QueryFirstOrDefaultAsync<string>(queryUsuario, new { UsuarioUltimaModificacion = usuarioInserto });


                        if (codigoVersion != 0)
                        {
                            //Versiones Adjuntos
                            string queryUltimoValor1 = @"select  max(Codigo_version_adjunto) from VERSIONES_ADJUNTOS";
                            int UltimoValor1 = await connection.ExecuteScalarAsync<int>(queryUltimoValor1);
                            int nuevoUltimoValor1 = UltimoValor1 + 1;
                            int Codigo_version_adjunto = nuevoUltimoValor1;

                            string queryArchivo1 = @" insert into VERSIONES_ADJUNTOS (
                                                Codigo_version_adjunto
                                              , codigo_archivo
                                              , codigo_version
                                              , usuario_inserto
                                              , fecha_inserto                                                                                          
                                              )
                                         values (" + Codigo_version_adjunto + " , "
                                                       + Codigo_archivo + " , "
                                                       + codigoVersion + " ,"
                                                       + usuario + " , GETDATE()) ";

                            var resultado2 = await connection.ExecuteAsync(queryArchivo1);
                        }
                    }
                }       

            return resultado;
            }
            catch (Exception ex)
            {                
                return "Error";
            }
        }

        static string ExtractSubstring(string input, string startPhrase, string endPhrase)
        {
            int startIndex = input.IndexOf(startPhrase);
            if (startIndex == -1)
            {
                throw new ArgumentException("La frase de inicio no se encuentra en la cadena.");
            }

            startIndex += startPhrase.Length; // Mover el índice al final de la frase de inicio

            int endIndex = input.IndexOf(endPhrase, startIndex);
            if (endIndex == -1)
            {
                throw new ArgumentException("La frase de fin no se encuentra en la cadena después de la frase de inicio.");
            }

            return input.Substring(startIndex, endIndex - startIndex).Trim();


        }

        public async Task<string> PersistirArchivoAnterior(IFormFile archivo, int codigoVersion,int usuarioInserto,string dominio)
        {
            string result = "";
            string result1 = "";

            //Obtenemos el Token para la gestion
            var token = string.Empty;
            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Trim();
            }

            var requestL = new HttpRequestMessage(HttpMethod.Post, _gestionArchivos.BaseUrl + _gestionArchivos.LoginUrl);
            var contentL = new StringContent($"{{\"usuario\": \"{_gestionArchivos.Username}\",\"clave\": \"{_gestionArchivos.Password}\"}}", null, "application/json");
            requestL.Content = contentL;
            var responseL = await _httpClient.SendAsync(requestL);
            responseL.EnsureSuccessStatusCode();
            string var = await responseL.Content.ReadAsStringAsync();

            string input = var;
            string startPhrase = "\"bearerToken\": \"";
            string endPhrase = "\", \"refreshToken\":";

            try
            {
                result = ExtractSubstring(input, startPhrase, endPhrase);
                Console.WriteLine($"La subcadena es: '{result}'");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            // Define el usuarioSesion
            var request = new HttpRequestMessage(HttpMethod.Post, _gestionArchivos.BaseUrl + _gestionArchivos.PersistirArchivoAnteriorUrl);

            request.Headers.Add("Authorization", token);

            var content = new MultipartFormDataContent();

            string resultado;
            using (var stream = archivo.OpenReadStream())
            {
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);
                content.Add(fileContent, "file", archivo.FileName);
                request.Content = content;

                // Enviar la solicitud
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Leer y devolver la respuesta
               resultado = await response.Content.ReadAsStringAsync();
            }

            string input1 = resultado;
            string start = "data\":\"";
            string endP = "\"}";

            try
            {
                result1 = ExtractSubstring(input1, start, endP);
                Console.WriteLine($"La subcadena es: '{result1}'");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }


            if (!string.IsNullOrEmpty(resultado))
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string fileName = System.IO.Path.GetFileName(archivo.FileName);
                    string fileExtension = System.IO.Path.GetExtension(archivo.FileName);
                   
                    
                    

                    //Archivos Adjuntos
                    string queryUltimoValor = @"select  max(Codigo_archivo) from ARCHIVOS_ADJUNTOS";
                    int UltimoValor = await connection.ExecuteScalarAsync<int>(queryUltimoValor);
                    int nuevoUltimoValor = UltimoValor + 1;
                    int Codigo_archivo = nuevoUltimoValor;

                    string queryArchivo = @" insert into ARCHIVOS_ADJUNTOS (
                                                codigo_archivo
                                              , referencia
                                              , dominio
                                              , nombre_archivo
                                              , extension                                              
                                              )
                                         values (" + Codigo_archivo + " ,'" 
                                                   + result1 + "' , '" 
                                                   + dominio + "' ,'" 
                                                   + fileName + "', '" 
                                                   + fileExtension + "') ";

                    var resultado1 = await connection.ExecuteAsync(queryArchivo);

                    //Versiones Adjuntos
                    string queryUltimoValor1 = @"select  max(Codigo_version_adjunto) from VERSIONES_ADJUNTOS";
                    int UltimoValor1 = await connection.ExecuteScalarAsync<int>(queryUltimoValor1);
                    int nuevoUltimoValor1 = UltimoValor1 + 1;
                    int Codigo_version_adjunto = nuevoUltimoValor1;

                    string usuario;
                    string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad =@UsuarioUltimaModificacion"
                    ;

                    usuario = await connection.QueryFirstOrDefaultAsync<string>(queryUsuario, new { UsuarioUltimaModificacion = usuarioInserto });



                    string queryArchivo1 = @" insert into VERSIONES_ADJUNTOS (
                                                Codigo_version_adjunto
                                              , codigo_archivo
                                              , codigo_version
                                              , usuario_inserto
                                              , fecha_inserto                                                                                          
                                              )
                                         values (" + Codigo_version_adjunto + " , "
                                                   + Codigo_archivo + " , "
                                                   + codigoVersion + " ,"
                                                   + usuario + " , GETDATE()) ";

                    var resultado2 = await connection.ExecuteAsync(queryArchivo1);

                }

            }
            Console.WriteLine(resultado);

            return resultado;
           
        }

        public async Task<ClaseBajarArchivo> BajarArchivoAnterior(int codigoVersion,  string usuarioInserto,  string dominio)
        {

            string result = "";

            //Obtenemos el Token para la gestion
            var token = string.Empty;
            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Trim();
            }


            var requestL = new HttpRequestMessage(HttpMethod.Post, _gestionArchivos.BaseUrl + _gestionArchivos.LoginUrl);
            var contentL = new StringContent($"{{\"usuario\": \"{_gestionArchivos.Username}\",\"clave\": \"{_gestionArchivos.Password}\"}}", null, "application/json");
            requestL.Content = contentL;
            var responseL = await _httpClient.SendAsync(requestL);
            responseL.EnsureSuccessStatusCode();
            string var = await responseL.Content.ReadAsStringAsync();     

            string input = var;
            string startPhrase = "\"bearerToken\": \"";
            string endPhrase = "\", \"refreshToken\":";

            try
            {
                result = ExtractSubstring(input, startPhrase, endPhrase);             
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            // Define el usuarioSesion
            string referencia = "";
            string nombre = "";
            //Obtener ultima referencia de usuario

            using (var connection = this._conexion.CreateSqlConnection())
            {
                //Versiones Adjuntos
                string usuario;
                string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad =@UsuarioUltimaModificacion"
                ;

                usuario = await connection.QueryFirstOrDefaultAsync<string>(queryUsuario, new { UsuarioUltimaModificacion = usuarioInserto });
                string completar = "";
                if (dominio == "ADJUNTO_ANTEPROYECTO_PRESUPUESTO_CIRCUNSCRIPCION")
                {
                    completar = "VA.CODIGO_VERSION=@codigoVersion and AA.dominio= 'ADJUNTO_ANTEPROYECTO_PRESUPUESTO_CIRCUNSCRIPCION'";
                }

                if (dominio == "FORMATO_IMPORTACION_SOLICITUDES")
                {
                    completar = "AA.CODIGO_ARCHIVO= VA.CODIGO_ARCHIVO and AA.dominio= 'FORMATO_IMPORTACION_SOLICITUDES'";
                }


                string queryUltimoReferencia = @"select top 1 AA.referencia, AA.CODIGO_ARCHIVO from ARCHIVOS_ADJUNTOS  AA 
                                           join VERSIONES_ADJUNTOS VA on AA.CODIGO_ARCHIVO= VA.CODIGO_ARCHIVO 
                                           where " + completar + " and VA.USUARIO_INSERTO= " + usuario + " order by AA.CODIGO_ARCHIVO desc"; 
                
                referencia = await connection.ExecuteScalarAsync<string>(queryUltimoReferencia, new { codigoVersion = codigoVersion });

                string queryUltimoNombre = @"select top 1 AA.nombre_archivo from ARCHIVOS_ADJUNTOS  AA 
                                           join VERSIONES_ADJUNTOS VA on AA.CODIGO_ARCHIVO= VA.CODIGO_ARCHIVO 
                                           where " + completar + " and VA.USUARIO_INSERTO= " + usuario + " order by AA.CODIGO_ARCHIVO desc";
                                         

                nombre = await connection.ExecuteScalarAsync<string>(queryUltimoNombre, new { codigoVersion = codigoVersion });
            }

            var request = new HttpRequestMessage(HttpMethod.Get, _gestionArchivos.BaseUrl + _gestionArchivos.BajarArchivoUrl + referencia);
            request.Headers.Add("Authorization", token);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadAsByteArrayAsync();         

            return new ClaseBajarArchivo
            {
                archivo = resultado,
                nombre = nombre
            };
        }

        #endregion

        #region DESARROLLO DE LA INCIDENCIA CSJ-312

        public async Task<int> NotificacionesCierre(int codigoVersion, string usuarioEjecucion, int codigoCircunscripcion)
        {

            var resultado = 0;
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    #region Registro y Cierre de fecha Cierre

                    string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad = @UsuarioEjecucion
                    ";

                    var parametros = new DynamicParameters();
                    parametros.Add("@UsuarioEjecucion", usuarioEjecucion);

                    var codigoUsuario = await connection.ExecuteScalarAsync<int>(queryUsuario, parametros);

                    int anioActual = DateTime.Now.Year + 1;
                    string descripcion = "Fecha de cierre del ejercicio " + (anioActual + 1).ToString();

                    var parametrosFecha = new DynamicParameters();
                    parametrosFecha.Add("@UsuarioInserto", codigoUsuario);
                    parametrosFecha.Add("@Ejercicio", anioActual + 1);
                    parametrosFecha.Add("@Descripcion", descripcion);

                    string nombreProcedimiento = "sp_RegistrarFechaCierreCompleto";
                    await connection.ExecuteAsync(nombreProcedimiento, parametrosFecha, commandType: CommandType.StoredProcedure);

                    #endregion Registro y Cierre de fecha Cierre                        
                }

                using (var connection = this._conexion.CreateSqlConnectionCapital())
                {

                    //Registro Notificaciones ECU
                    #region Registro Notificaciones

                    string queryNotificacionesMensaje = @"SELECT  
                                                ref.descripcion_larga AS mensaje                                                   
                                                FROM referencias ref 
                                                JOIN tipo_referencias tipo_ref ON tipo_ref.codigo_tipo_referencia = ref.codigo_tipo_referencia 
                                                WHERE tipo_ref.dominio_tipo_referencia = 'NOTIFICACION_RECEPCION_ANTEPROYECTO'
                                                    AND ref.valor_decimal = 0 ";
                    string queryNotificacionesTipoNotificacion = @"SELECT                                                
                                                tipo_ref.descripcion_tipo_referencia as tipoNotificacion 
                                                FROM referencias ref 
                                                JOIN tipo_referencias tipo_ref ON tipo_ref.codigo_tipo_referencia = ref.codigo_tipo_referencia 
                                                WHERE tipo_ref.dominio_tipo_referencia = 'NOTIFICACION_RECEPCION_ANTEPROYECTO'
                                                    AND ref.valor_decimal = 0 ";

                    string mensaje = await connection.ExecuteScalarAsync<string>(queryNotificacionesMensaje);
                    string TipoNotificacion = await connection.ExecuteScalarAsync<string>(queryNotificacionesTipoNotificacion);

                    string queryCodigoUsuarioNotificaciones = @"SELECT ref.valor_decimal AS CodUsuario 
                                                                FROM referencias ref 
                                                                JOIN tipo_referencias tipo_ref
                                                                    ON tipo_ref.codigo_tipo_referencia = ref.codigo_tipo_referencia 
                                                                WHERE tipo_ref.dominio_tipo_referencia = 'NOTIFICACION_RECEPCION_ANTEPROYECTO' AND ref.valor_decimal <> 0 ";

                    var CodigoUsuarioRecepcion = await connection.QueryAsync<CodigoUsuario>(queryCodigoUsuarioNotificaciones);
                    string variableUsuario = "";
                    if (CodigoUsuarioRecepcion != null && CodigoUsuarioRecepcion.Any())
                    {
                        foreach (var CodigoUsuario in CodigoUsuarioRecepcion)
                        {

                            variableUsuario = CodigoUsuario.CodUsuario.ToString();
                            string queryUltimoValorCodigo = @"SELECT ISNULL(MAX(codigo_notificaciones_enviadas),0) FROM notificaciones_consultas";
                            int ultimoValor = await connection.ExecuteScalarAsync<int>(queryUltimoValorCodigo);
                            int nuevoCodigo = ultimoValor + 1;

                            string insertarNotificaciones = @" INSERT INTO notificaciones_consultas 
                    (codigo_notificaciones_enviadas, fecha_recepcion, codigo_usuario_recepcion, mensaje, codigo_circunscripción_enviado, tipo_notificacion, leido)           
                    VALUES (@CodigoNotificaciones, GETDATE(), @CodigoUsuarioRecepcion, @Mensaje ,@CodigoCircunscripciónEnviado ,@TipoNotificacion ,0)";

                            var parametrosNotificaciones = new DynamicParameters();
                            parametrosNotificaciones.Add("@CodigoNotificaciones", nuevoCodigo);
                            parametrosNotificaciones.Add("@CodigoUsuarioRecepcion", variableUsuario);
                            parametrosNotificaciones.Add("@Mensaje", mensaje);
                            parametrosNotificaciones.Add("@CodigoCircunscripciónEnviado", codigoCircunscripcion);
                            parametrosNotificaciones.Add("@TipoNotificacion", TipoNotificacion);

                            if (mensaje != "" && TipoNotificacion != "" && variableUsuario != "" && codigoCircunscripcion != 0 && codigoVersion != 0)
                            {
                                int InsertarNotificaciones = await connection.ExecuteScalarAsync<int>(insertarNotificaciones, parametrosNotificaciones);
                            }

                        }
                    }
                    #endregion Registro Notificaciones
                }

            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error a registrar las notificaciones y fecha cierre." + "||-->" + ex.Message + "<--||");
            }
            return resultado;
        }

        public async Task<int> SincronizarDBCapital(int codigoVersion, int codigoCircunscripcion, string usuarioEjecucion, int codigoTarea)
        {
            _logger.LogInformation("Inicio de Proceso de Sincronizar la Base de datos de Capital ");

            #region QUERYS SELECT 

            string selectVA = @"
                SELECT 
                    codigo_version as CodigoVersionOrigen,
                    numero_version AS NumeroVersion,
                    codigo_circunscripcion AS CodigoCircunscripcion,
                    ejercicio AS Ejercicio,
                    estado AS Estado,
                    usuario_inserto AS UsuarioInserto,
                    fecha_inserto AS FechaInserto,
                    usuario_modificacion AS UsuarioModificacion,
                    fecha_modificacion AS FechaModificacion,
                    version_consolidado AS VersionConsolidado,
                    es_ley AS EsLey
                FROM versiones_anteproyectos 
                WHERE
                    codigo_version = @CodigoVersion ";

            string selectVAO = @"
            SELECT 
                codigo_version as CodigoVersionOrigen,
                codigo_anteproyecto_objeto AS CodigoAOOrigen,
                codigo_version AS CodigoVersion,
                codigo_configuracion_presupuestaria AS CodigoCCPOrigen,
                codigo_fuente_financiamiento AS CodigoFuenteFinanciamiento,
                codigo_organismo_financiador AS CodigoOrganismoFinanciador,
                evento AS Evento,
                presupuesto_inicial AS PresupuestoInicial,
                Modificaciones AS Modificaciones,
                monto_planificado AS MontoPlanificado,
                codigo_circunscripcion_origen AS CodigoCircunscripcionOrigen
            FROM 
                versiones_anteproyectos_objetos
            WHERE 
                codigo_version = @CodigoVersion ";

            string selectConfig = @"
            SELECT 
                codigo_configuracion_presupuestaria AS CodigoCCPOrigen,
                codigo_objeto_gasto AS CodigoObjetoGasto,
                codigo_programa AS CodigoPrograma,
                codigo_actividad AS CodigoActividad,
                codigo_tipo_presupuesto AS CodigoTipoPresupuesto,
                codigo_departamento AS CodigoDepartamento,
                grupo AS Grupo,
                codigo_centro_responsabilidad AS CodigoCentroResponsabilidad,
                codigo_materia AS CodigoMateria,
                subgrupo AS Subgrupo,
                ejercicio AS Ejercicio
            FROM 
                configuracion_presupuestaria ";

            string selectVAB = @"
            SELECT 
                codigo_version as CodigoVersionOrigen,
                codigo_anteproyecto_bien AS CodigoABOrigen,
                codigo_anteproyecto_objeto AS CodigoAOOrigen,
                codigo_version AS CodigoVersion,
                numero_bien AS NumeroBien,
                descripcion_bien AS DescripcionBien,
                cantidad AS Cantidad,
                valor_unitario AS ValorUnitario,
                usuario_inserto AS UsuarioInserto,
                fecha_inserto AS FechaInserto,
                usuario_modificacion AS UsuarioModificacion,
                fecha_modificacion AS FechaModificacion,
                unidad_medida AS UnidadMedida,
                fundamentacion AS Fundamentacion,
                seleccionado AS Seleccionado
            FROM 
                versiones_anteproyectos_bienes
            WHERE 
                codigo_version = @CodigoVersion ";

            string selectVAP = @"
            SELECT 
                codigo_version as CodigoVersionOrigen,
                codigo_anteproyecto_planificacon AS CodigoAPOrigen,
                codigo_anteproyecto_objeto AS CodigoAOOrigen,
                codigo_version AS CodigoVersion,
                mes AS Mes,
                monto AS Monto,
                usuario_inserto AS UsuarioInserto,
                fecha_inserto AS FechaInserto,
                usuario_modificacion AS UsuarioModificacion,
                fecha_modificacion AS FechaModificacion
            FROM 
                versiones_anteproyecto_planificacion
            WHERE 
                codigo_version = @CodigoVersion ";

            string selectVC = @"
            SELECT 
                codigo_version as CodigoVersionOrigen,
                codigo_anteproyecto_contrato AS CodigoACOrigen,
                codigo_anteproyecto_objeto AS CodigoAOOrigen,
                codigo_version AS CodigoVersion,
                codigo_contrato AS CodigoContrato,
                usuario_inserto AS UsuarioInserto,
                fecha_inserto AS FechaInserto,
                usuario_modificacion AS UsuarioModificacion,
                fecha_modificacion AS FechaModificacion,
                monto_contrato AS MontoContrato
            FROM 
                versiones_contratos
            WHERE 
                codigo_version = @CodigoVersion ";

            string selectVACB = @"
            SELECT 
	            codigo_anteproyecto_bien AS CodigoABOrigen,
                codigo_anteproyecto_objeto AS CodigoAOOrigen,
                codigo_version as CodigoVersionOrigen,
                numero_bien as NumeroBien,
                descripcion_bien as DescripcionBien,
                cantidad as Cantidad,
                valor_unitario as ValorUnitario,
                usuario_inserto as UsuarioInserto,
                fecha_inserto as FechaInserto,
                usuario_modificacion as UsuarioModificacion,
                fecha_modificacion as FechaModificacion,
                unidad_medida as UnidadMedida,
                fundamentacion as Fundamentacion,
                seleccionado as Seleccionado,
                codigo_centro_responsabilidad as CodigoCentroResponsabilidad,
                codigo_materia as CodigoMateria
            FROM 
                versiones_anteproyectos_conf_bienes
            WHERE codigo_version = @CodigoVersion ";

            #endregion

            string estadoEjecucion = "Inicio";

            string queryTareasLotes = @"
            INSERT INTO tareas_lotes
            (codigo_tarea, codigo_version, codigo_circunscripcion, descripcion, estado, fecha_inicio, usuario_ejecucion, fecha_finalizacion)
            OUTPUT inserted.codigo_tarea
            VALUES ((SELECT ISNULL(MAX(codigo_tarea), 0) + 1 FROM tareas_lotes), @CodigoVersion, @CodigoCircunscripcion, @Descripcion, @Estado, GETDATE(), @UsuarioEjecucion, GETDATE())
            ";

            string queryUpdateTareasLotes = @"
            UPDATE tareas_lotes
            SET descripcion = @Descripcion, estado = @Estado, fecha_finalizacion = GETDATE()
            WHERE codigo_tarea = @CodigoTarea
            ";

            string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                              WHERE cedula_identidad =@UsuarioEjecucion
            ";

            int codigoCircunscripcionOrigen;
            VersionAnteproyectoSync versionAnteproyecto;
            List<VersionesAnteproyectosObjetosSync> versionAnteproyectoObjeto;
            List<VersionesAnteproyectosBienesSync> versionAnteproyectoBienes;
            List<VersionesAnteproyectoPlanificacionSync> versionAnteproyectoPlanificacion;
            List<VersionesContratosSync> versionContratos;
            List<ConfiguracionPresupuestariaSync> configuracionPresupuestaria;
            List<VersionesAnteproyectosConfBienesSync> versionAnteproyectoConfigBienes;

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@UsuarioEjecucion", usuarioEjecucion);

                    estadoEjecucion = "Ejecucion de queryUsuario";
                    var codigoUsuario = await connection.ExecuteScalarAsync<int>(queryUsuario, parametros);

                    if (codigoTarea == 0)
                    {
                        parametros = new DynamicParameters();
                        parametros.Add("@CodigoVersion", codigoVersion);
                        parametros.Add("@CodigoCircunscripcion", codigoCircunscripcion);
                        parametros.Add("@Descripcion", "El proceso se está ejecutando.");
                        parametros.Add("@Estado", 2);
                        parametros.Add("@UsuarioEjecucion", codigoUsuario);

                        estadoEjecucion = "Ejecucion de queryTareasLotes";
                        codigoTarea = await connection.ExecuteScalarAsync<int>(queryTareasLotes, parametros);
                    }

                    parametros = new DynamicParameters();
                    parametros.Add("@CodigoVersion", codigoVersion);

                    estadoEjecucion = "Ejecucion de selectVA";
                    versionAnteproyecto = await connection.QuerySingleAsync<VersionAnteproyectoSync>(selectVA, parametros);

                    codigoCircunscripcionOrigen = versionAnteproyecto.CodigoCircunscripcion;
                    versionAnteproyecto.CodigoCircunscripcion = 1;

                    estadoEjecucion = "Ejecucion de selectVAO";
                    versionAnteproyectoObjeto = (await connection.QueryAsync<VersionesAnteproyectosObjetosSync>(selectVAO, parametros)).ToList();

                    versionAnteproyectoBienes = new List<VersionesAnteproyectosBienesSync>();

                    string codigosVao = versionAnteproyectoObjeto.First().CodigoAOOrigen.ToString();
                    string codigosConfig = versionAnteproyectoObjeto.First().CodigoCCPOrigen.ToString();
                    if (versionAnteproyectoObjeto.Count > 1)
                    {
                        foreach (VersionesAnteproyectosObjetosSync vao in versionAnteproyectoObjeto)
                        {
                            codigosVao += (", " + vao.CodigoAOOrigen.ToString());
                            codigosConfig += (", " + vao.CodigoCCPOrigen.ToString());
                        }
                    }

                    codigosVao = ("AND codigo_anteproyecto_objeto IN (" + codigosVao + ") ORDER BY codigo_configuracion_presupuestaria ");
                    codigosConfig = ("WHERE codigo_configuracion_presupuestaria IN (" + codigosConfig + ") ORDER BY codigo_configuracion_presupuestaria ");
                    selectConfig += codigosConfig;

                    estadoEjecucion = "Ejecucion de selectVAB";
                    versionAnteproyectoBienes = (await connection.QueryAsync<VersionesAnteproyectosBienesSync>(selectVAB, parametros)).ToList();
                    versionAnteproyectoPlanificacion = (await connection.QueryAsync<VersionesAnteproyectoPlanificacionSync>(selectVAP, parametros)).ToList();
                    versionContratos = (await connection.QueryAsync<VersionesContratosSync>(selectVC, parametros)).ToList();
                    configuracionPresupuestaria = (await connection.QueryAsync<ConfiguracionPresupuestariaSync>(selectConfig, parametros)).ToList();
                    versionAnteproyectoConfigBienes = (await connection.QueryAsync<VersionesAnteproyectosConfBienesSync>(selectVACB, parametros)).ToList();

                }



            }
            catch (Exception ex)
            {
                /*try
                {
                    using (var connection = this._conexion.CreateSqlConnection())
                    {
                        var parametros = new DynamicParameters();
                        parametros.Add("@Descripcion", "Error al obtener los datos de las tablas de versiones.");
                        parametros.Add("@Estado", 3);
                        parametros.Add("@CodigoTarea", codigoTarea);

                        await connection.QuerySingleAsync<VersionAnteproyectoSync>(queryUpdateTareasLotes, parametros);
                    }
                }
                catch (Exception e)
                {
                    throw new GeneracionSolicitudesException("Error al actualizar la tabla de tareas_lotes." + "||-->" + e.Message + "<--||");
                }*/
                throw new GeneracionSolicitudesException("Ocurrio un error al obtener los datos de las tablas de versiones en " + estadoEjecucion + "." + "||-->" + ex.Message + "<--||");
            }

            #region QUERYS INSERT

            // VERSION ANTEPROYECTO
            // Para el insert de Version Anteproyecto
            var insertVA = @"
            INSERT INTO  versiones_anteproyectos  
            (codigo_version, numero_version, codigo_circunscripcion, ejercicio, estado, usuario_inserto, fecha_inserto, usuario_modificacion, fecha_modificacion, version_consolidado, es_ley)
            OUTPUT inserted.codigo_version as CodigoVersion
            VALUES ((SELECT ISNULL(MAX(codigo_version), 0) + 1 FROM versiones_anteproyectos), (SELECT ISNULL(MAX(numero_version), 0) + 1 FROM versiones_anteproyectos), @CodigoCircunscripcion, @Ejercicio, 2, @UsuarioInserto, @FechaInserto, @UsuarioModificacion, @FechaModificacion, @VersionConsolidado, @EsLey)
            ";

            // Valida si ya existe una Version Consolidada del Anteproyecto
            string validacionVA = @"SELECT COUNT(*) 
                FROM versiones_anteproyectos 
                WHERE ejercicio = @Ejercicio 
                    AND version_consolidado = 1 ";

            // Obtiene el codigo de la version consolidada
            string obtenerVersionConsolidada = @"SELECT codigo_version 
                FROM versiones_anteproyectos 
                WHERE ejercicio = @Ejercicio 
                    AND version_consolidado = 1 ";


            // CONFIGURACION PRESUPUESTARIA
            // Para el insert/update de Configuracion Presupuestaria
            var insertConfig = @"INSERT INTO configuracion_presupuestaria 
            (codigo_configuracion_presupuestaria, codigo_objeto_gasto, codigo_programa, codigo_actividad, codigo_tipo_presupuesto, codigo_departamento, grupo, codigo_centro_responsabilidad, codigo_materia, subgrupo, ejercicio) 
            OUTPUT inserted.codigo_configuracion_presupuestaria as CodigoConfiguracionPresupuestaria
            VALUES ((SELECT ISNULL(MAX(codigo_configuracion_presupuestaria), 0) + 1 FROM configuracion_presupuestaria), @CodigoObjetoGasto, @CodigoPrograma, @CodigoActividad, @CodigoTipoPresupuesto, @CodigoDepartamento, @Grupo, @CodigoCentroResponsabilidad, @CodigoMateria, @Subgrupo, @Ejercicio)
            ";


            // VERSIONES ANTEPROYECTO OBJETO
            // Para el insert/update de Versiones Anteproyectos Objetos
            var insertVAO = $@"INSERT INTO versiones_anteproyectos_objetos 
            (codigo_anteproyecto_objeto, codigo_version, codigo_configuracion_presupuestaria, codigo_fuente_financiamiento, codigo_organismo_financiador, evento, presupuesto_inicial, Modificaciones, monto_planificado, codigo_circunscripcion_origen)
            OUTPUT inserted.codigo_anteproyecto_objeto as CodigoAnteproyectoObjeto
            VALUES (
            (SELECT ISNULL(MAX(codigo_anteproyecto_objeto), 0) + 1 FROM versiones_anteproyectos_objetos),
            @CodigoVersion, @CodigoConfiguracionPresupuestaria, @CodigoFuenteFinanciamiento, @CodigoOrganismoFinanciador, @Evento, @PresupuestoInicial, @Modificaciones, @MontoPlanificado, {codigoCircunscripcionOrigen})
            ";


            // VERSIONES ANTEPROYECTOS BIENES
            // Para el insert/update de Versiones Anteproyectos Bienes

            var insertVAB = @"INSERT INTO versiones_anteproyectos_bienes 
            (codigo_anteproyecto_bien, codigo_anteproyecto_objeto, codigo_version, numero_bien, descripcion_bien, cantidad, valor_unitario, usuario_inserto, fecha_inserto, usuario_modificacion, fecha_modificacion, unidad_medida, fundamentacion, seleccionado)
            OUTPUT inserted.codigo_anteproyecto_bien as CodigoAnteproyectoBien
            VALUES (
            (SELECT ISNULL(MAX(codigo_anteproyecto_bien), 0) + 1 FROM versiones_anteproyectos_bienes), 
            @CodigoAnteproyectoObjeto, @CodigoVersion, @NumeroBien, @DescripcionBien, @Cantidad, @ValorUnitario, @UsuarioInserto, @FechaInserto, @UsuarioModificacion, @FechaModificacion, @UnidadMedida, @Fundamentacion, @Seleccionado)
            ";


            // VERSIONES ANTEPROYECTOS PLANIFICACION
            // Para el insert/update de Versiones Anteproyectos Planificacion

            var insertVAP = @"INSERT INTO versiones_anteproyecto_planificacion 
            (codigo_anteproyecto_planificacon, codigo_anteproyecto_objeto, codigo_version, mes, monto, usuario_inserto, fecha_inserto, usuario_modificacion, fecha_modificacion)
            VALUES (
            (SELECT ISNULL(MAX(codigo_anteproyecto_planificacon), 0) + 1 FROM versiones_anteproyecto_planificacion), 
            @CodigoAnteproyectoObjeto, @CodigoVersion, @Mes, @Monto, @UsuarioInserto, @FechaInserto, @UsuarioModificacion, @FechaModificacion)
            ";


            // VERSIONES CONTRATOS
            // Para el insert/update de Versiones Contratos
            var insertVC = @"INSERT INTO versiones_contratos 
            (codigo_anteproyecto_contrato, codigo_anteproyecto_objeto, codigo_version, codigo_contrato, usuario_inserto, fecha_inserto, usuario_modificacion, fecha_modificacion, monto_contrato) 
            VALUES (
            (SELECT ISNULL(MAX(codigo_anteproyecto_contrato ), 0) + 1 FROM versiones_contratos), 
            @CodigoAnteproyectoObjeto, @CodigoVersion, @CodigoContrato, @UsuarioInserto, @FechaInserto, @UsuarioModificacion, @FechaModificacion, @MontoContrato)
            ";

            // VERSIONES ANTEPROYECTOS CONFIGURACION BIENES
            // Para el insert/update de Versiones Configuracion Bienes
            var insertVACB = @"INSERT INTO versiones_anteproyectos_conf_bienes 
            (codigo_anteproyecto_objeto, codigo_anteproyecto_bien, codigo_version, numero_bien, descripcion_bien, cantidad, valor_unitario, usuario_inserto, fecha_inserto, 
            usuario_modificacion, fecha_modificacion, unidad_medida, fundamentacion, seleccionado, codigo_centro_responsabilidad, codigo_materia) 
            VALUES (
            @CodigoAnteproyectoObjeto, @CodigoAnteproyectoBien, @CodigoVersion, @NumeroBien, @DescripcionBien, @Cantidad, @ValorUnitario, @UsuarioInserto, @FechaInserto, 
            @UsuarioModificacion, @FechaModificacion, @UnidadMedida, @Fundamentacion, @Seleccionado, @CodigoCentroResponsabilidad, @CodigoMateria)
            ";

            #endregion

            var resultadoValidacion = 0;
            var resultadoUpdateVA = 0;
            var resultadoUpdateConfig = 0;
            var resultadoUpdateVAO = 0;
            var resultadoUpdateVAB = 0; 
            var resultadoUpdateVAP = 0; 
            var resultadoUpdateVC = 0;
            var resultadoUpdateVACB = 0;

            using (var connection = this._conexion.CreateSqlConnectionCapital())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int codigoVersionNuevo = 0;
                        resultadoValidacion = await connection.ExecuteScalarAsync<int>(validacionVA, versionAnteproyecto, transaction);
                        if (resultadoValidacion == 0)
                        {
                            versionAnteproyecto.VersionConsolidado = 1;
                            codigoVersionNuevo = await connection.ExecuteScalarAsync<int>(insertVA, versionAnteproyecto, transaction);
                        }
                        else
                        {
                            codigoVersionNuevo = await connection.ExecuteScalarAsync<int>(obtenerVersionConsolidada, versionAnteproyecto, transaction);
                        }
                        resultadoValidacion = 0;

                        //INSERT CONFIGURACION PRESUPUESTARIA
                        foreach (ConfiguracionPresupuestariaSync config in configuracionPresupuestaria)
                        {
                            config.CodigoConfiguracionPresupuestaria = await connection.ExecuteScalarAsync<int>(insertConfig, config, transaction);
                        }

                        //INSERT OBJETOS
                        foreach (VersionesAnteproyectosObjetosSync vao in versionAnteproyectoObjeto)
                        {
                            vao.CodigoVersion = codigoVersionNuevo;
                            foreach (ConfiguracionPresupuestariaSync config in configuracionPresupuestaria)
                            {
                                if (vao.CodigoCCPOrigen == config.CodigoCCPOrigen)
                                {
                                    vao.CodigoConfiguracionPresupuestaria = config.CodigoConfiguracionPresupuestaria;
                                }
                            }
                            vao.CodigoAnteproyectoObjeto = await connection.ExecuteScalarAsync<int>(insertVAO, vao, transaction);
                        }

                        //INSERT BIENES
                        foreach (VersionesAnteproyectosBienesSync vab in versionAnteproyectoBienes)
                        {
                            vab.CodigoVersion = codigoVersionNuevo;
                            foreach (VersionesAnteproyectosObjetosSync vao in versionAnteproyectoObjeto)
                            {
                                if (vab.CodigoAOOrigen == vao.CodigoAOOrigen)
                                {
                                    vab.CodigoAnteproyectoObjeto = vao.CodigoAnteproyectoObjeto;
                                    break;
                                }
                            }
                            vab.CodigoAnteproyectoBien = await connection.ExecuteScalarAsync<int>(insertVAB, vab, transaction);
                        }

                        //INSERT PLANIFICACION
                        foreach (VersionesAnteproyectoPlanificacionSync vap in versionAnteproyectoPlanificacion)
                        {
                            vap.CodigoVersion = codigoVersionNuevo;
                            foreach (VersionesAnteproyectosObjetosSync vao in versionAnteproyectoObjeto)
                            {
                                if (vap.CodigoAOOrigen == vao.CodigoAOOrigen)
                                {
                                    vap.CodigoAnteproyectoObjeto = vao.CodigoAnteproyectoObjeto;
                                    break;
                                }
                            }
                        }
                        resultadoUpdateVAP = await connection.ExecuteAsync(insertVAP, versionAnteproyectoPlanificacion, transaction);

                        //INSERT CONTRATOS
                        foreach (VersionesContratosSync vc in versionContratos)
                        {
                            vc.CodigoVersion = codigoVersionNuevo;
                            //vc.CodigoAnteproyectoObjeto = resultadoUpdateVAO.Find(x => x.CodigoAOOrigen == vc.CodigoAOOrigen).CodigoAnteproyectoObjeto;
                            foreach (VersionesAnteproyectosObjetosSync vao in versionAnteproyectoObjeto)
                            {
                                if (vc.CodigoAOOrigen == vao.CodigoAOOrigen)
                                {
                                    vc.CodigoAnteproyectoObjeto = vao.CodigoAnteproyectoObjeto;
                                    break;
                                }
                            }
                        }
                        resultadoUpdateVC = await connection.ExecuteAsync(insertVC, versionContratos, transaction);

                        //INSERT CONFIG BIENES
                        foreach (VersionesAnteproyectosConfBienesSync vacb in versionAnteproyectoConfigBienes)
                        {
                            vacb.CodigoVersion = codigoVersionNuevo;
                            foreach (VersionesAnteproyectosObjetosSync vao in versionAnteproyectoObjeto)
                            {
                                if (vacb.CodigoAOOrigen == vao.CodigoAOOrigen)
                                {
                                    vacb.CodigoAnteproyectoObjeto = vao.CodigoAnteproyectoObjeto;
                                    break;
                                }
                            }
                            foreach (VersionesAnteproyectosBienesSync vab in versionAnteproyectoBienes)
                            {
                                if (vacb.CodigoABOrigen == vab.CodigoABOrigen)
                                {
                                    vacb.CodigoAnteproyectoBien = vab.CodigoAnteproyectoBien;
                                    break;
                                }
                            }
                        }
                        resultadoUpdateVACB = await connection.ExecuteAsync(insertVACB, versionAnteproyectoConfigBienes, transaction);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        try
                        {
                            using (var connection2 = this._conexion.CreateSqlConnection())
                            {
                                var parametros = new DynamicParameters();
                                parametros.Add("@Descripcion", "Error al insertar en la base de datos de la capital.");
                                parametros.Add("@Estado", 3);
                                parametros.Add("@CodigoTarea", codigoTarea);

                                await connection2.ExecuteScalarAsync<int>(queryUpdateTareasLotes, parametros);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new GeneracionSolicitudesException("Error al actualizar la tabla de tareas_lotes." + "||-->" + e.Message + "<--||");
                        }

                        throw new GeneracionSolicitudesException("Ocurrio un error al insertar en la base de datos de la capital." + "||-->" + ex.Message + "<--||");
                    }
                }
            }

            try
            {
                using (var connection2 = this._conexion.CreateSqlConnection())
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@Descripcion", "El proceso se ejecutó correctamente.");
                    parametros.Add("@Estado", 1);
                    parametros.Add("@CodigoTarea", codigoTarea);

                    await connection2.ExecuteScalarAsync<int>(queryUpdateTareasLotes, parametros);
                }
            }
            catch (Exception e)
            {
                throw new GeneracionSolicitudesException("Error al actualizar la tabla de tareas_lotes." + "||-->" + e.Message + "<--||");
            }

            _logger.LogInformation("Se finalizó Proceso de Sincronización de las tablas de Versiones en la Base de datos de Capital.");

            return resultadoUpdateVA;
        }

        public async Task<ClaseBajarArchivo> BajarArchivo(int codigoVersion, string usuarioInserto, string dominio)
        {          
            string referencia = "";
            string nombre = "";
            //Obtener ultima referencia de usuario
            try
            {

                //Obtenemos el Token para la gestion
                var token = string.Empty;
                if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Trim();
                }

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    //Versiones Adjuntos
                    string usuario;
                    string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad 
                                                  WHERE cedula_identidad =@UsuarioUltimaModificacion"
                    ;
                    string queryUltimoReferencia = "";
                    string queryUltimoNombre = "";
                    usuario = await connection.QueryFirstOrDefaultAsync<string>(queryUsuario, new { UsuarioUltimaModificacion = usuarioInserto });
                    string completar = "";
                    if (dominio == "ADJUNTO_ANTEPROYECTO_PRESUPUESTO_CIRCUNSCRIPCION")
                    {
                        completar = "VA.CODIGO_VERSION=@codigoVersion and AA.dominio= 'ADJUNTO_ANTEPROYECTO_PRESUPUESTO_CIRCUNSCRIPCION'";
                        queryUltimoReferencia = @"select top 1 AA.referencia, AA.CODIGO_ARCHIVO from ARCHIVOS_ADJUNTOS  AA 
                                               join VERSIONES_ADJUNTOS VA on AA.CODIGO_ARCHIVO= VA.CODIGO_ARCHIVO 
                                               where " + completar + " and VA.USUARIO_INSERTO= " + usuario + " order by AA.CODIGO_ARCHIVO desc";

                        queryUltimoNombre = @"select top 1 AA.nombre_archivo+AA.extension from ARCHIVOS_ADJUNTOS  AA 
                                               join VERSIONES_ADJUNTOS VA on AA.CODIGO_ARCHIVO= VA.CODIGO_ARCHIVO 
                                               where " + completar + " and VA.USUARIO_INSERTO= " + usuario + " order by AA.CODIGO_ARCHIVO desc";
                    }

                    if (dominio == "FORMATO_IMPORTACION_SOLICITUDES")
                    {
                        completar = "AA.dominio= 'FORMATO_IMPORTACION_SOLICITUDES'";
                        queryUltimoReferencia = @"select top 1 AA.referencia, AA.CODIGO_ARCHIVO from ARCHIVOS_ADJUNTOS  AA                                           
                                               where " + completar + " order by AA.CODIGO_ARCHIVO desc";
                        queryUltimoNombre = @"select top 1 AA.nombre_archivo+AA.extension from ARCHIVOS_ADJUNTOS  AA 
                                           
                                               where " + completar + " order by AA.CODIGO_ARCHIVO desc";
                    }

                    referencia = await connection.ExecuteScalarAsync<string>(queryUltimoReferencia, new { codigoVersion = codigoVersion });

                    nombre = await connection.ExecuteScalarAsync<string>(queryUltimoNombre, new { codigoVersion = codigoVersion });
                }

                var request = new HttpRequestMessage(HttpMethod.Get, _gestionArchivos.BaseUrl + _gestionArchivos.BajarArchivoUrl + referencia);
                request.Headers.Add("Authorization", token);
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var resultado = await response.Content.ReadAsByteArrayAsync();

                ClaseBajarArchivo ClaseBajarArchivo = new ClaseBajarArchivo();

                ClaseBajarArchivo.archivo = resultado;
                ClaseBajarArchivo.nombre = nombre;
                return ClaseBajarArchivo;
            }
            catch (Exception ex)
            {
                ClaseBajarArchivo ClaseBajarArchivo = new ClaseBajarArchivo();
                return ClaseBajarArchivo;
            }
        }

        #endregion


    }
}

