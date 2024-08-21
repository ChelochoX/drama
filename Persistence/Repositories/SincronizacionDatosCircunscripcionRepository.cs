using Application.Services.Interfaces.IRepository;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Entities.Request;
using Domain.Entities.SincronizacionDatosCircunscripcion;
using Domain.Exceptions.SincronizacionDatosCircunscripcion;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Persistence.Repositories;

public class SincronizacionDatosCircunscripcionRepository : ISincronizacionDatosCircunscripcionRepository
{
    private readonly DbConnections _conexion;
    private readonly ILogger<SincronizacionDatosCircunscripcionRepository> _logger;

    public SincronizacionDatosCircunscripcionRepository(ILogger<SincronizacionDatosCircunscripcionRepository> logger, DbConnections conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<Datos<IEnumerable<SincronizacionDatosDTO>>> ObtenerDatosOrganismoFinanciador(SincronizacionDatosRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener tareas por lotes");

        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
        var query = string.Empty;
        var filtrosAdicionales = string.Empty;
        // Tratamos de identificar si el dato que viene es una fecha

        try
        {
           if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
           request.CodigoTarea.HasValue ||
           request.CodigoVersion.HasValue ||
           request.CodigoCircunscripcion.HasValue ||
           !string.IsNullOrEmpty(request.DescripcionCircunscripcion) ||
           !string.IsNullOrEmpty(request.DescripcionTarea) ||
           !string.IsNullOrEmpty(request.Estado) ||
           !string.IsNullOrEmpty(request.FechaInicio) ||
           !string.IsNullOrEmpty(request.UsuarioEjecucion) ||
           !string.IsNullOrEmpty(request.FechaFinalizacion))
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    filtrosAdicionales += @"
                        AND (CONVERT(NVARCHAR(MAX), tl.codigo_tarea) LIKE '%' + @TerminoDeBusqueda + '%'                       
                        OR CONVERT(NVARCHAR(MAX), tl.codigo_version) LIKE '%' + @TerminoDeBusqueda + '%'
                        OR c.nombre_circunscripcion LIKE '%' + @TerminoDeBusqueda + '%'
                        OR tl.descripcion LIKE '%' + @TerminoDeBusqueda + '%'
                        OR r.descripcion_referencia LIKE '%' + @TerminoDeBusqueda + '%'
                        OR u.username LIKE '%' + @TerminoDeBusqueda + '%'
                        OR ( CONVERT(VARCHAR, tl.fecha_inicio, 120) LIKE '%' + @TerminoDeBusqueda + '%') 
                        OR ( CONVERT(VARCHAR, tl.fecha_finalizacion, 120)  LIKE '%' + @TerminoDeBusqueda + '%' ) 
                        OR CONVERT(NVARCHAR(MAX), tl.codigo_circunscripcion) LIKE '%' + @TerminoDeBusqueda + '%')";
                }
                else
                {
                    filtrosAdicionales += @"
                        AND (@CodigoTarea IS NULL OR CONVERT(NVARCHAR(MAX), tl.codigo_tarea) LIKE '%' + @CodigoTarea + '%')
                        AND (@CodigoVersion IS NULL OR CONVERT(NVARCHAR(MAX), tl.codigo_version) LIKE '%' + @CodigoVersion + '%')
                        AND (@CodigoCircunscripcion IS NULL OR CONVERT(NVARCHAR(MAX), tl.codigo_circunscripcion) LIKE '%' + @CodigoCircunscripcion + '%')
                        AND (@DescripcionCircunscripcion IS NULL OR c.nombre_circunscripcion LIKE '%' + @DescripcionCircunscripcion + '%')
                        AND (@DescripcionTarea IS NULL OR tl.descripcion LIKE '%' + @DescripcionTarea + '%')
                        AND (@Estado IS NULL OR r.descripcion_referencia LIKE '%' + @Estado + '%')                        
                        AND (@UsuarioEjecucion IS NULL OR u.username LIKE '%' + @UsuarioEjecucion + '%')
                        AND (@FechaInicio IS NULL OR CONVERT(VARCHAR, tl.fecha_inicio, 120) LIKE '%' + @FechaInicio + '%') 
                        AND (@FechaFinalizacion IS NULL OR CONVERT(VARCHAR, tl.fecha_finalizacion, 120)  LIKE '%' + @FechaFinalizacion + '%' ) 
                     ";
                }
            }

            query = $@"
                SELECT 
                    tl.codigo_tarea as CodigoTarea, 
                    tl.codigo_version as CodigoVersion, 
                    tl.codigo_circunscripcion as CodigoCircunscripcion, 
                    c.nombre_circunscripcion as DescripcionCircunscripcion, 
                    tl.descripcion as DescripcionTarea, 
                    r.descripcion_referencia as Estado, 
                    tl.fecha_inicio as FechaInicio, 
                    u.username as UsuarioEjecucion, 
                    tl.fecha_finalizacion as FechaFinalizacion
                FROM tareas_lotes tl 
                JOIN circunscripciones c ON c.codigo_circunscripcion=tl.codigo_circunscripcion  
                JOIN referencias r ON r.valor_decimal=tl.estado   
                JOIN tipo_referencias tr ON tr.codigo_tipo_referencia= r.codigo_tipo_referencia   
                JOIN usuarios_poder_judicial u ON tl.usuario_ejecucion = u.codigo_usuario

                WHERE tr.dominio_tipo_referencia LIKE 'ESTADO_TAREAS_POR_LOTE'
                AND (tl.codigo_circunscripcion <> 1 
                AND tl.codigo_circunscripcion = @paramCodigoCircunscripcion)
                {filtrosAdicionales}
                
                UNION
                
                SELECT 
                    tl.codigo_tarea as CodigoTarea, 
                    tl.codigo_version as CodigoVersion, 
                    tl.codigo_circunscripcion as CodigoCircunscripcion, 
                    c.nombre_circunscripcion as DescripcionCircunscripcion, 
                    tl.descripcion as DescripcionTarea, 
                    r.descripcion_referencia as Estado, 
                    tl.fecha_inicio as FechaInicio, 
                    u.username as UsuarioEjecucion, 
                    tl.fecha_finalizacion as FechaFinalizacion
                FROM tareas_lotes tl 
                JOIN circunscripciones c ON c.codigo_circunscripcion=tl.codigo_circunscripcion 
                JOIN referencias r ON r.valor_decimal=tl.estado   
                JOIN tipo_referencias tr ON tr.codigo_tipo_referencia= r.codigo_tipo_referencia   
                JOIN usuarios_poder_judicial u ON tl.usuario_ejecucion = u.codigo_usuario
                WHERE tr.dominio_tipo_referencia LIKE 'ESTADO_TAREAS_POR_LOTE'
                AND (tl.codigo_circunscripcion = 1 
                AND tl.codigo_circunscripcion IN (SELECT codigo_circunscripcion FROM circunscripciones))
                {filtrosAdicionales}
        
                ORDER BY tl.codigo_tarea 
                OFFSET {saltarRegistros} ROWS FETCH NEXT {request.CantidadRegistros} ROWS ONLY";

            var queryCantidadTotalRegistros = $@"
                    SELECT COUNT(*) AS TotalRegistros 
                    FROM (
                        SELECT 
                            tl.codigo_tarea as CodigoTarea, 
                            tl.codigo_version as CodigoVersion, 
                            tl.codigo_circunscripcion as CodigoCircunscripcion, 
                            c.nombre_circunscripcion as DescripcionCircunscripcion, 
                            tl.descripcion as DescripcionTarea, 
                            r.descripcion_referencia as Estado, 
                            tl.fecha_inicio as FechaInicio, 
                            u.username as UsuarioEjecucion, 
                            tl.fecha_finalizacion as FechaFinalizacion
                        FROM tareas_lotes tl 
                        JOIN circunscripciones c ON c.codigo_circunscripcion=tl.codigo_circunscripcion  
                        JOIN referencias r ON r.valor_decimal=tl.estado   
                        JOIN tipo_referencias tr ON tr.codigo_tipo_referencia= r.codigo_tipo_referencia   
                        JOIN usuarios_poder_judicial u ON tl.usuario_ejecucion = u.codigo_usuario
                        WHERE tr.dominio_tipo_referencia LIKE 'ESTADO_TAREAS_POR_LOTE'
                        AND (tl.codigo_circunscripcion <> 1 
                        AND tl.codigo_circunscripcion = @paramCodigoCircunscripcion)
                        {filtrosAdicionales}
            
                        UNION
            
                        SELECT 
                            tl.codigo_tarea as CodigoTarea, 
                            tl.codigo_version as CodigoVersion, 
                            tl.codigo_circunscripcion as CodigoCircunscripcion, 
                            c.nombre_circunscripcion as DescripcionCircunscripcion, 
                            tl.descripcion as DescripcionTarea, 
                            r.descripcion_referencia as Estado, 
                            tl.fecha_inicio as FechaInicio, 
                            u.username as UsuarioEjecucion, 
                            tl.fecha_finalizacion as FechaFinalizacion
                        FROM tareas_lotes tl 
                        JOIN circunscripciones c ON c.codigo_circunscripcion=tl.codigo_circunscripcion 
                        JOIN referencias r ON r.valor_decimal=tl.estado   
                        JOIN tipo_referencias tr ON tr.codigo_tipo_referencia= r.codigo_tipo_referencia   
                        JOIN usuarios_poder_judicial u ON tl.usuario_ejecucion = u.codigo_usuario
                        WHERE tr.dominio_tipo_referencia LIKE 'ESTADO_TAREAS_POR_LOTE'
                        AND (tl.codigo_circunscripcion = 1 
                        AND tl.codigo_circunscripcion IN (SELECT codigo_circunscripcion FROM circunscripciones))
                        {filtrosAdicionales}
                    ) AS Total";


            var terminoDeBusqueda = request.TerminoDeBusqueda ?? string.Empty;
            if (int.TryParse(terminoDeBusqueda, out int numericSearchTerm))
            {
                terminoDeBusqueda = numericSearchTerm.ToString();
            }

            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@CantidadRegistros", request.CantidadRegistros);
            parametros.Add("@paramCodigoCircunscripcion", request.ParamCodigoCircunscripcion);
            parametros.Add("@TerminoDeBusqueda", terminoDeBusqueda);

            parametros.Add("@CodigoTarea", request.CodigoTarea?.ToString());
            parametros.Add("@CodigoVersion", request.CodigoVersion?.ToString());
            parametros.Add("@CodigoCircunscripcion", request.CodigoCircunscripcion?.ToString());
            parametros.Add("@DescripcionCircunscripcion", request.DescripcionCircunscripcion);
            parametros.Add("@DescripcionTarea", request.DescripcionTarea);
            parametros.Add("@Estado", request.Estado);

            string fechaInicioParaSQL =null;
            string fechaFinParaSQL = null;
         
            fechaInicioParaSQL = request.FechaInicio;

            parametros.Add("@FechaInicio", fechaInicioParaSQL);
            parametros.Add("@UsuarioEjecucion", request.UsuarioEjecucion);

            //}
            fechaFinParaSQL =  request.FechaFinalizacion ;
            parametros.Add("@FechaFinalizacion", fechaFinParaSQL);


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QueryAsync<SincronizacionDatosDTO>(query, parametros);

                var response = new Datos<IEnumerable<SincronizacionDatosDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalRegistros
                };

                _logger.LogInformation("Fin de Proceso de obtener tareas por lotes");
                return response;
            }
        }

        catch (Exception ex)
        {
            throw new SincronizacionDatosException("Ocurrió un error inesperado de tareas por lote de la sincronizacion de datos" + "||-->" + ex.Message + "<--||");
        }
    }


}
