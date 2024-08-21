using Application.Services.Interfaces.IRepository;
using Dapper;
using DocumentFormat.OpenXml.Office2016.Excel;
using Domain.Entities.VersionadoAnteproyecto;
using Microsoft.Extensions.Logging;
using Domain.Entities.Request;
using Domain.DTOs;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;

namespace Persistence.Repositories;

public class VersionadoAnteproyectoRepository: IVersionadoAnteproyectoRepository
{
    private readonly DbConnections _conexion;
    private readonly ILogger<VersionadoAnteproyectoRepository> _logger;

    public VersionadoAnteproyectoRepository(ILogger<VersionadoAnteproyectoRepository> logger, DbConnections conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<Datos<IEnumerable<VersionadoAnteproyectoDTO>>> ListarVersionadodeAnteproyecto(VersionadoAnteproyectoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el listado de versionado de anteproyecto");
        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
        var filtrosAdicionales = string.Empty;
        var filtrohaving1= string.Empty;
        var filtrohaving2 = string.Empty;
        try
        {
            if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                !string.IsNullOrEmpty(request.NumeroOBG) ||
                !string.IsNullOrEmpty(request.NumeroFF) ||
                !string.IsNullOrEmpty(request.NumeroOF) ||
                !string.IsNullOrEmpty(request.PresupuestoVigente) ||
                !string.IsNullOrEmpty(request.ProyectoPresupuesto) ||
                !string.IsNullOrEmpty(request.Diferencia) ||
                 !string.IsNullOrEmpty(request.Porcentaje) ||
                !string.IsNullOrEmpty(request.Fundamentacion)
       
          )
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    filtrosAdicionales += @"
                        HAVING ( (CONVERT(NVARCHAR(MAX), vap.numero_og) LIKE '%' + @TerminoDeBusqueda + '%'                       
                        OR CONVERT(NVARCHAR(MAX), vap.numero_ff) LIKE '%' + @TerminoDeBusqueda + '%'
                        OR vap.fundamentacion LIKE '%' + @TerminoDeBusqueda + '%'
                        OR CONVERT(NVARCHAR(MAX), vap.numero_of) LIKE '%' + @TerminoDeBusqueda + '%'
                    ) ";

                    filtrohaving1 = @"
                        OR CONVERT(NVARCHAR(MAX), ROUND(
                            CASE 
                                WHEN SUM(vap.presupuesto_vigente) <> 0 THEN ISNULL((CAST(SUM(vap.diferencia) AS FLOAT) / CAST(SUM(vap.presupuesto_vigente) AS FLOAT) * 100), 0) 
                                ELSE 0 
                            END, 0
                        )) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
                        OR CONVERT(NVARCHAR(MAX), SUM(vap.presupuesto_vigente)) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
                        OR CONVERT(NVARCHAR(MAX), SUM(vap.proyecto_presupuesto)) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
                        OR CONVERT(NVARCHAR(MAX), SUM(vap.diferencia)) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
                    ) ";

                    filtrohaving2 = @"
                        OR CONVERT(NVARCHAR(MAX), ROUND(
                            CASE 
                                WHEN SUM(vap.presupuesto_vigente) <> 0 THEN ISNULL((CAST(SUM(vap.diferencia) AS FLOAT) / CAST(SUM(vap.presupuesto_vigente) AS FLOAT) * 100), 0) 
                                ELSE 0 
                            END, 0
                        )) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
                        OR CONVERT(NVARCHAR(MAX), SUM(vap.presupuesto_vigente)) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
                        OR CONVERT(NVARCHAR(MAX), SUM(vap.proyecto_presupuesto)) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
     
                    ) ";

                }
                else
                {
                    filtrosAdicionales += @"
                        HAVING(
                        (@NumeroOG IS NULL OR CONVERT(NVARCHAR(MAX),  vap.numero_og) LIKE '%' + @NumeroOG + '%')
                        AND (@NumeroFF IS NULL OR CONVERT(NVARCHAR(MAX), vap.numero_ff) LIKE '%' + @NumeroFF + '%')
                        AND (@NumeroOF IS NULL OR CONVERT(NVARCHAR(MAX), vap.numero_of) LIKE '%' + @NumeroOF + '%')                        
                        AND (@Fundamentacion IS NULL OR vap.fundamentacion LIKE '%' + @Fundamentacion + '%')
                     ";

                    filtrohaving1 = @" 
                        AND (@Porcentaje IS NULL OR CONVERT(NVARCHAR(MAX), ROUND(
                        CASE
                                        WHEN SUM(vap.presupuesto_vigente) <> 0 THEN ISNULL((CAST(SUM(vap.diferencia) AS FLOAT) / CAST(SUM(vap.presupuesto_vigente) AS FLOAT) * 100), 0)
                                        ELSE 0
                                    END, 0
                        )) LIKE '%' + REPLACE(@Porcentaje, '.', '') + '%' )
                                AND (@PresupuestoVigente IS NULL OR CONVERT(NVARCHAR(MAX), SUM(vap.presupuesto_vigente)) LIKE '%' + REPLACE(@PresupuestoVigente, '.', '') + '%')
                                AND (@ProyectoPresupuesto IS NULL OR CONVERT(NVARCHAR(MAX), SUM(vap.proyecto_presupuesto)) LIKE '%' + REPLACE(@ProyectoPresupuesto, '.', '') + '%')
                                AND (@Diferencia IS NULL OR CONVERT(NVARCHAR(MAX), SUM(vap.diferencia)) LIKE '%' + REPLACE(@Diferencia, '.', '') + '%')
                    ) ";

                    filtrohaving2 = @"
                        AND (@Porcentaje IS NULL OR CONVERT(NVARCHAR(MAX), ROUND(
                            CASE
                                WHEN SUM(vap.presupuesto_vigente) <> 0 THEN ISNULL((CAST(SUM(vap.diferencia) AS FLOAT) / CAST(SUM(vap.presupuesto_vigente) AS FLOAT) * 100), 0)
                                ELSE 0
                            END, 0
                        )) LIKE '%' + REPLACE(@Porcentaje, '.', '') + '%')
                        AND (@PresupuestoVigente IS NULL OR CONVERT(NVARCHAR(MAX), SUM(vap.presupuesto_vigente)) LIKE '%' + REPLACE(@PresupuestoVigente, '.', '') + '%')
                        AND (@ProyectoPresupuesto IS NULL OR CONVERT(NVARCHAR(MAX), SUM(vap.proyecto_presupuesto)) LIKE '%' + REPLACE(@ProyectoPresupuesto, '.', '') + '%')
                        AND (@Diferencia IS NULL OR CONVERT(NVARCHAR(MAX), SUM(vap.diferencia)) LIKE '%' + REPLACE(@Diferencia, '.', '') + '%')
                    ) ";

                }
            }



            var query = $@"
            SELECT  
                COALESCE(v1.numero_og, v2.numero_og) AS NumeroOBG, 
                COALESCE(v1.numero_ff, v2.numero_ff) AS NumeroFF, 
                COALESCE(v1.numero_of, v2.numero_of) AS NumeroOF, 
                COALESCE(v1.fundamentacion, v2.fundamentacion) AS Fundamentacion, 
                COALESCE(v1.presupuesto_vigente, v2.presupuesto_vigente) AS PresupuestoVigente,  
                ISNULL(v1.proyecto_presupuesto1, 0) AS ProyectoPresupuesto1,  
                 ISNULL(v1.diferencia1, 0) AS Diferencia1,  
                ISNULL(V1.Porcentaje1, 0)  AS Porcentaje1, 
                ISNULL(v2.proyecto_presupuesto2, 0) AS ProyectoPresupuesto2,  
                ISNULL((ISNULL(v2.proyecto_presupuesto2, 0) - ISNULL(v1.proyecto_presupuesto1, 0)), 0) AS DiferenciaVersiones, 
                ISNULL(V2.Porcentaje2, 0)  AS Porcentaje2
            FROM (
                SELECT  
                    vap.EJERCICIO, 
                    vap.NUMERO_VERSION,
                    vap.numero_og, 
                    vap.numero_ff,
                    vap.numero_of, 
                    vap.fundamentacion, 
                    SUM(vap.presupuesto_vigente) AS presupuesto_vigente, 
                    SUM(vap.proyecto_presupuesto) AS proyecto_presupuesto1, 
                    SUM(vap.diferencia) AS Diferencia1, 
                    vap.CODIGO_OBJETO_GASTO,  
                    vap.CODIGO_FUENTE_FINANCIAMIENTO,
                    vap.CODIGO_ORGANISMO_FINANCIADOR,
                    ROUND(
                        CASE 
                            WHEN SUM(vap.presupuesto_vigente) <> 0 
                            THEN ISNULL((CAST(SUM(vap.diferencia) AS FLOAT) / CAST(SUM(vap.presupuesto_vigente) AS FLOAT) * 100), 0) 
                            ELSE 0 
                        END, 
                        0
                    ) AS Porcentaje1
                FROM vlistaAnteproyectoPresupuestarioCircunscripcion vap 
                WHERE vap.NUMERO_VERSION = @version1 
             and vap.EJERCICIO = @ejercicio
               
            GROUP BY  
                    vap.EJERCICIO, 
                    vap.NUMERO_VERSION, 
                    vap.numero_og, 
                    vap.numero_ff, 
                    vap.numero_of, 
                    vap.fundamentacion, 
                    vap.CODIGO_OBJETO_GASTO,  
                    vap.CODIGO_FUENTE_FINANCIAMIENTO, 
                    vap.CODIGO_ORGANISMO_FINANCIADOR 
            {filtrosAdicionales}  
            {filtrohaving1}
            ) AS v1  
            FULL OUTER JOIN (
                SELECT  
                    vap.EJERCICIO, 
                    vap.NUMERO_VERSION, 
                    vap.numero_og, 
                    vap.numero_ff,
                    vap.numero_of, 
                    vap.fundamentacion, 
                    SUM(vap.presupuesto_vigente) AS presupuesto_vigente, 
                    SUM(vap.proyecto_presupuesto) AS proyecto_presupuesto2, 
                    SUM(vap.diferencia) AS Diferencia2, 
                    vap.CODIGO_OBJETO_GASTO,  
                    vap.CODIGO_FUENTE_FINANCIAMIENTO, 
                    vap.CODIGO_ORGANISMO_FINANCIADOR,
                    ROUND(
                        CASE 
                            WHEN SUM(vap.presupuesto_vigente) <> 0 
                            THEN ISNULL((CAST(SUM(vap.diferencia) AS FLOAT) / CAST(SUM(vap.presupuesto_vigente) AS FLOAT) * 100), 0) 
                            ELSE 0 
                        END, 
                        0
            ) AS Porcentaje2
                FROM vlistaAnteproyectoPresupuestarioCircunscripcion vap 
                where vap.NUMERO_VERSION = @version2                                           
             and vap.EJERCICIO = @ejercicio 
            GROUP BY  
                    vap.EJERCICIO, 
                    vap.NUMERO_VERSION, 
                    vap.numero_og, 
                    vap.numero_ff, 
                    vap.numero_of, 
                    vap.fundamentacion, 
                    vap.CODIGO_OBJETO_GASTO,  
                    vap.CODIGO_FUENTE_FINANCIAMIENTO, 
                    vap.CODIGO_ORGANISMO_FINANCIADOR 
            {filtrosAdicionales}
            {filtrohaving2}
            ) AS v2  
            ON v2.CODIGO_OBJETO_GASTO = v1.CODIGO_OBJETO_GASTO 
            AND v2.CODIGO_FUENTE_FINANCIAMIENTO = v1.CODIGO_FUENTE_FINANCIAMIENTO 
            AND v2.CODIGO_ORGANISMO_FINANCIADOR = v1.CODIGO_ORGANISMO_FINANCIADOR 
            AND (v1.fundamentacion = v2.fundamentacion OR v1.fundamentacion IS NULL OR v2.fundamentacion IS NULL)
            ";

            string queryresultado = query + $@" order by COALESCE(v1.numero_og, v2.numero_og), COALESCE(v1.numero_ff, v2.numero_ff) ,
                                COALESCE(v1.numero_of, v2.numero_of)    OFFSET {saltarRegistros} ROWS FETCH NEXT {request.CantidadRegistros} ROWS ONLY";

            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@ejercicio", request.Ejercicio);
            parametros.Add("@version1", request.NumeroVersion1);
            parametros.Add("@version2", request.NumeroVersion2);
            parametros.Add("@NumeroOG", request.NumeroOBG);
            parametros.Add("@NumeroOF", request.NumeroOF);

            parametros.Add("@NumeroFF", request.NumeroFF);
            parametros.Add("@Fundamentacion", request.Fundamentacion);
            parametros.Add("@PresupuestoVigente", request.PresupuestoVigente);
            parametros.Add("@TerminoDeBusqueda", request.TerminoDeBusqueda);

            parametros.Add("@PresupuestoVigente", request.PresupuestoVigente);
            parametros.Add("@ProyectoPresupuesto", request.ProyectoPresupuesto);
            parametros.Add("@Diferencia", request.Diferencia);
            parametros.Add("@Porcentaje", request.Porcentaje);
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<VersionadoAnteproyectoDTO>(queryresultado, parametros);
                string queryCantidadTotalRegistros = @"SELECT COUNT(*) FROM (" + query + ") AS COUNT";
                var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);
                var listado = new Datos<IEnumerable<VersionadoAnteproyectoDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalRegistros
                };


                

                _logger.LogInformation("Fin de Proceso de obtener el listado de versionado de anteproyecto");
               
                return listado;
            }
        }
        catch (Exception ex)
        {
            throw new VersionadoAnteproyectoException("Ocurrió un error inesperado al obtener el listado de versionado de anteproyecto" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<IEnumerable<VersionesAnteproyectoDTO>> ObtenerVersionesporEjercicio(int ejercicio)
    {
        _logger.LogInformation("Inicio de Proceso de obtener las versiones por ejercicio");

        try
        {
            var query = $@"
                            select CODIGO_VERSION as Version 
                            from VERSIONES_ANTEPROYECTOS
                            where EJERCICIO = @ejercicio";

            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@ejercicio", ejercicio);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<VersionesAnteproyectoDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de obtener las versiones por ejercicio");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new VersionadoAnteproyectoException("Ocurrió un error inesperado al obtener las versiones que corresponden al ejercicio" + "||-->" + ex.Message + "<--||");
        }
    }
}
