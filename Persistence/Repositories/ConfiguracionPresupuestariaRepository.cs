using Application.Services.Interfaces.IRepository;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;
using Domain.Entities.Request;
using Domain.Entities.Response;
using Domain.Exceptions.ImportarArchivoSIPOIExcepcions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DatosparaConfiguracionPresupuestariaDTO = Domain.Entities.ConfiguracionPresyupuestaria.DatosparaConfiguracionPresupuestariaDTO;

namespace Persistence.Repositories;

public class ConfiguracionPresupuestariaRepository: IConfiguracionPresupuestariaRepository
{
    private readonly DbConnections _conexion;
    private readonly ILogger<ConfiguracionPresupuestariaRepository> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ConfiguracionPresupuestariaRepository(DbConnections conexion, ILogger<ConfiguracionPresupuestariaRepository> logger, IHttpContextAccessor httpContextAccessor)
    {
        _conexion = conexion;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<Datos<IEnumerable<ConfiguracionPresupuestariaPorObjetosGastosDTO>>> ObtenerObjetosdeGastosparalaConfiguracionPresupuestaria(ConfiguracionPresupuestariaPorObjetosGastosRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de la Copnfiguracion Presupuestaria por Objeto de Gastos");

        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
        var query = string.Empty;
        var filtrosAdicionales = string.Empty;
        var filtrosAdicionalesMontos = string.Empty;
        string paginacion = $"ORDER BY t1.ObjetoGasto Asc OFFSET {saltarRegistros} ROWS FETCH NEXT {request.CantidadRegistros} ROWS ONLY";

        try
        {

            if (!string.IsNullOrEmpty(request.Poi) ||
                !string.IsNullOrEmpty(request.CentroResponsabilidad) ||
                !string.IsNullOrEmpty(request.Departamento) ||
                !string.IsNullOrEmpty(request.DescripcionMateria) ||
                !string.IsNullOrEmpty(request.ObjetoGasto) ||
                !string.IsNullOrEmpty(request.MontoTotal) ||
                !string.IsNullOrEmpty(request.Cantidad) ||
                !string.IsNullOrEmpty(request.TerminoDeBusqueda))
            {
                // Se proporcionaron parámetros de búsqueda, agregar filtros adicionales
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    filtrosAdicionales += @"
                    where (
                        t1.poi LIKE '%' + @terminoDeBusqueda + '%'                       
                        OR t1.CodigoCentroResposabilidad LIKE '%' + @terminoDeBusqueda + '%'
                        OR t1.DescripcionMateria LIKE '%' + @terminoDeBusqueda + '%'
                        OR t1.Departamento LIKE '%' + @terminoDeBusqueda + '%'
                        OR t1.ObjetoGasto LIKE '%' + @terminoDeBusqueda + '%'
                        OR COALESCE(CONVERT(BIGINT, t2.CantidadSolicitud), 0) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '')  + '%'
                        OR COALESCE(CONVERT(BIGINT, t2.MontoSolicitud), 0) LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '') + '%'                   
                    )";
                }
                else
                {                  

                    filtrosAdicionales += @"
                    where (@poi IS NULL OR t1.poi LIKE '%' + @poi + '%')                       
                    AND (@parametroCentroResponsabilidad IS NULL OR t1.CodigoCentroResposabilidad LIKE '%' + @parametroCentroResponsabilidad + '%') 
                    AND (@parametroDescripcionMateria IS NULL OR t1.DescripcionMateria LIKE '%' + @parametroDescripcionMateria + '%') 
                    AND (@parametroDepartamento IS NULL OR t1.Departamento LIKE '%' + @parametroDepartamento + '%') 
                    AND (@parametroObjetoGasto IS NULL OR t1.ObjetoGasto LIKE '%' + @parametroObjetoGasto + '%')
                    AND(@parametroCantidad IS NULL OR  COALESCE(CONVERT(BIGINT, t2.CantidadSolicitud), 0) LIKE '%' + REPLACE(@parametroCantidad, '.', '') + '%')
                    AND(@parametroMontoTotal IS NULL  OR  COALESCE(CONVERT(BIGINT, t2.MontoSolicitud), 0) LIKE '%' + REPLACE(@parametroMontoTotal, '.', '')  + '%')";
                }
            }

               query = $@"
                       WITH t1 AS (
                        SELECT     
                            s.ejercicio AS POI,            
                            v.codigo_centro_responsabilidad as CodigoCentroResposabilidad,
                            v.codigo_materia as CodigoMateria,
                            v.descripcion_materia as DescripcionMateria,
                            og.codigo_objeto_gasto as CodigoObjetoGasto,
                            v.descripcion_centro_responsabilidad AS CentroResponsabilidad,  
                            v.numero_departamento as Departamento,  
                            (og.numero_objeto_gasto +'-'+ og.descrip_objeto_gasto) AS ObjetoGasto
          
                        FROM   
                            solicitudes_bienes_circunscripcion s  
                        JOIN 
                            vListaCentrosResponsabilidadPorCircunscripcion v ON v.codigo_centro_responsabilidad = s.codigo_centro_responsabilidad
                           and v.codigo_materia = s.codigo_materia and s.codigo_circunscripcion = v.codigo_circunscripcion  
                        JOIN 
                            solicitudes_objetos_detalle sod ON sod.codigo_solicitud = s.codigo_solicitud  
                        JOIN 
                            objeto_gasto og ON sod.codigo_objeto_gasto = og.codigo_objeto_gasto                          

                        WHERE s.ejercicio = @parametroPOI                              
    
                        GROUP BY  
                            og.codigo_objeto_gasto, 
                            s.ejercicio,    
                            v.descripcion_centro_responsabilidad,  
                            v.descripcion_materia,
                            v.numero_departamento, 
                            og.numero_objeto_gasto,   
                            og.descrip_objeto_gasto, 
                            v.codigo_centro_responsabilidad,
                            v.codigo_materia
                    )
                    SELECT 
                        t1.POI,
                        t1.CodigoCentroResposabilidad,
                        t1.CentroResponsabilidad,
                        t1.DescripcionMateria,
                        t1.CodigoMateria,
                        t1.Departamento,
                        t1.CodigoObjetoGasto,
                        t1.ObjetoGasto,
                        t3.""OF"",
                        T3.CodigoOrganismoFinanciador,
                        T3.FF,
                        T3.CodigoFuenteFinanciamiento,
                        t2.CantidadSolicitud,
                        t2.MontoSolicitud,
                        t2.CodigoCircunscripcion,
                        t3.CantidadConfiguracion,
                        t3.MontoConfiguracion
       
                    FROM 
                        t1
                    JOIN
                        (
                            SELECT 
                                s.codigo_centro_responsabilidad as CodigoCentroResposabilidad,
                                s.codigo_materia as CodigoMateria,
                                sod.codigo_objeto_gasto as CodigoObjetoGasto,                                    
                                COALESCE(SUM(CONVERT(BIGINT, bd.cantidad)),0) as CantidadSolicitud,
                                COALESCE(SUM(CONVERT(BIGINT, bd.cantidad) * bd.costo_unitario),0) as MontoSolicitud,
                                s.codigo_circunscripcion as CodigoCircunscripcion
                            FROM 
                                solicitudes_bienes_circunscripcion s  
                            JOIN 
                                solicitudes_objetos_detalle sod ON sod.codigo_solicitud = s.codigo_solicitud
                            JOIN 
                                solicitudes_objetos_bienes_detalle bd ON bd.codigo_solicitud = s.codigo_solicitud AND bd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto 
                            WHERE
                                sod.estado = 2
                                and s.ejercicio = @parametroPOI  
                                
                            GROUP BY 
                                s.codigo_circunscripcion,
                                s.codigo_centro_responsabilidad,
                                s.codigo_materia,
                                sod.codigo_objeto_gasto
               
                        ) as t2 on t2.CodigoObjetoGasto = t1.CodigoObjetoGasto and t2.CodigoCentroResposabilidad = t1.CodigoCentroResposabilidad and t1.CodigoMateria=t2.CodigoMateria
                    LEFT JOIN 
                        (
                      SELECT 
                                cp.codigo_centro_responsabilidad as CodigoCentroResposabilidad, 
                                cp.codigo_materia as CodigoMateria,
                                cp.codigo_objeto_gasto as CodigoObjetoGasto,                                  
                                COALESCE(SUM(CONVERT(BIGINT, vab.CANTIDAD)), 0) AS CantidadConfiguracion,
                                COALESCE(SUM(CONVERT(DECIMAL(18,2), vab.VALOR_UNITARIO) * CONVERT(BIGINT, vab.CANTIDAD)), 0) AS MontoConfiguracion,
			                    ff.codigo_fuente_financiamiento   as CodigoFuenteFinanciamiento,
                                ff.numero_fuente_financiamiento as FF,  
                                org.numero_organismo_financiador as ""OF"",
                                org.codigo_organismo_financiador as CodigoOrganismoFinanciador
                            FROM 
                                VERSIONES_ANTEPROYECTOS_OBJETOS vao
                            JOIN 
                               versiones_anteproyectos_conf_bienes vab ON vab.CODIGO_VERSION = vao.CODIGO_VERSION AND vab.CODIGO_ANTEPROYECTO_OBJETO = vao.CODIGO_ANTEPROYECTO_OBJETO AND vab.SELECCIONADO = 1
                            JOIN 
                                CONFIGURACION_PRESUPUESTARIA cp ON cp.CODIGO_CONFIGURACION_PRESUPUESTARIA = vao.codigo_configuracion_presupuestaria
                                and vab.codigo_centro_responsabilidad = cp.CODIGO_CENTRO_RESPONSABILIDAD and vab.codigo_materia = cp.CODIGO_MATERIA
                            JOIN VERSIONES_ANTEPROYECTOS va on va.CODIGO_VERSION = vao.CODIGO_VERSION
		                    JOIN 
                            fuente_financiamiento ff ON ff.codigo_fuente_financiamiento = vao.codigo_fuente_financiamiento 
                         JOIN 
                            organismo_financiador org ON org.codigo_organismo_financiador = vao.codigo_organismo_financiador
                            WHERE  
                                va.EJERCICIO = @parametroPOI 
                                and va.version_consolidado=0
                                and va.es_ley=0
                                 
                            GROUP BY
                                cp.codigo_centro_responsabilidad,
                                cp.codigo_materia,
                                cp.codigo_objeto_gasto,
			                    ff.codigo_fuente_financiamiento,
			                    ff.numero_fuente_financiamiento,
			                    org.numero_organismo_financiador,
			                    org.codigo_organismo_financiador
                        ) as t3 on t3.CodigoObjetoGasto = t2.CodigoObjetoGasto 
                       and t3.CodigoCentroResposabilidad = t2.CodigoCentroResposabilidad and t3.CodigoMateria=t2.CodigoMateria
                              {filtrosAdicionales} {paginacion};
		                 ";

            //var paginacion = $"ORDER BY t1.ObjetoGasto Asc OFFSET {saltarRegistros} ROWS FETCH NEXT {request.CantidadRegistros} ROWS ONLY";

            //query = string.Format(query, filtrosAdicionales, paginacion);
                      

         var   queryCantidadTotalRegistros = $@"
                        WITH t1 AS (
                        SELECT     
                            s.ejercicio AS POI,            
                            v.codigo_centro_responsabilidad AS CodigoCentroResposabilidad,
                            v.codigo_materia AS CodigoMateria,
                            v.descripcion_materia AS DescripcionMateria,
                            og.codigo_objeto_gasto AS CodigoObjetoGasto,
                            v.descripcion_centro_responsabilidad AS CentroResponsabilidad,  
                            v.numero_departamento AS Departamento,  
                            (og.numero_objeto_gasto +'-'+ og.descrip_objeto_gasto) AS ObjetoGasto
                        FROM   
                            solicitudes_bienes_circunscripcion s  
                        JOIN 
                            vListaCentrosResponsabilidadPorCircunscripcion v ON v.codigo_centro_responsabilidad = s.codigo_centro_responsabilidad
                            AND v.codigo_materia = s.codigo_materia AND s.codigo_circunscripcion = v.codigo_circunscripcion  
                        JOIN 
                            solicitudes_objetos_detalle sod ON sod.codigo_solicitud = s.codigo_solicitud  
                        JOIN 
                            objeto_gasto og ON sod.codigo_objeto_gasto = og.codigo_objeto_gasto                          
                        WHERE 
                            s.ejercicio = @parametroPOI
                        GROUP BY  
                            og.codigo_objeto_gasto, 
                            s.ejercicio,    
                            v.descripcion_centro_responsabilidad,  
                            v.descripcion_materia,
                            v.numero_departamento, 
                            og.numero_objeto_gasto,   
                            og.descrip_objeto_gasto, 
                            v.codigo_centro_responsabilidad,
                            v.codigo_materia
                    ),
                    t2 AS (
                        SELECT 
                            s.codigo_centro_responsabilidad AS CodigoCentroResposabilidad,
                            s.codigo_materia AS CodigoMateria,
                            sod.codigo_objeto_gasto AS CodigoObjetoGasto,                                    
                            COALESCE(SUM(CONVERT(BIGINT, bd.cantidad)), 0) AS CantidadSolicitud,
                            COALESCE(SUM(CONVERT(BIGINT, bd.cantidad) * bd.costo_unitario), 0) AS MontoSolicitud,
                            s.codigo_circunscripcion AS CodigoCircunscripcion
                        FROM 
                            solicitudes_bienes_circunscripcion s  
                        JOIN 
                            solicitudes_objetos_detalle sod ON sod.codigo_solicitud = s.codigo_solicitud
                        JOIN 
                            solicitudes_objetos_bienes_detalle bd ON bd.codigo_solicitud = s.codigo_solicitud 
                            AND bd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto 
                        WHERE
                            sod.estado = 2
                            and s.ejercicio = @parametroPOI  
                        GROUP BY 
                            s.codigo_circunscripcion,
                            s.codigo_centro_responsabilidad,
                            s.codigo_materia,
                            sod.codigo_objeto_gasto
                    ),
                    t3 AS (
                        SELECT 
                            cp.codigo_centro_responsabilidad AS CodigoCentroResposabilidad, 
                            cp.codigo_materia AS CodigoMateria,
                            cp.codigo_objeto_gasto AS CodigoObjetoGasto,                                  
                            COALESCE(SUM(CONVERT(BIGINT, vab.CANTIDAD)), 0) AS CantidadConfiguracion,
                            COALESCE(SUM(CONVERT(DECIMAL(18,2), vab.VALOR_UNITARIO) * CONVERT(BIGINT, vab.CANTIDAD)), 0) AS MontoConfiguracion,
                            ff.codigo_fuente_financiamiento AS CodigoFuenteFinanciamiento,
                            ff.numero_fuente_financiamiento AS FF,  
                            org.numero_organismo_financiador AS ""OF"",
                            org.codigo_organismo_financiador AS CodigoOrganismoFinanciador
                        FROM 
                            VERSIONES_ANTEPROYECTOS_OBJETOS vao
                        JOIN 
                            versiones_anteproyectos_conf_bienes vab ON vab.CODIGO_VERSION = vao.CODIGO_VERSION 
                            AND vab.CODIGO_ANTEPROYECTO_OBJETO = vao.CODIGO_ANTEPROYECTO_OBJETO 
                            AND vab.SELECCIONADO = 1
                        JOIN 
                            CONFIGURACION_PRESUPUESTARIA cp ON cp.CODIGO_CONFIGURACION_PRESUPUESTARIA = vao.codigo_configuracion_presupuestaria
                            AND vab.codigo_centro_responsabilidad = cp.CODIGO_CENTRO_RESPONSABILIDAD 
                            AND vab.codigo_materia = cp.CODIGO_MATERIA
                        JOIN 
                            VERSIONES_ANTEPROYECTOS va ON va.CODIGO_VERSION = vao.CODIGO_VERSION
                        JOIN 
                            fuente_financiamiento ff ON ff.codigo_fuente_financiamiento = vao.codigo_fuente_financiamiento 
                        JOIN 
                            organismo_financiador org ON org.codigo_organismo_financiador = vao.codigo_organismo_financiador
                        WHERE  
                            va.EJERCICIO = @parametroPOI 
                            AND va.version_consolidado = 0
                            AND va.es_ley = 0
                        GROUP BY
                            cp.codigo_centro_responsabilidad,
                            cp.codigo_materia,
                            cp.codigo_objeto_gasto,
                            ff.codigo_fuente_financiamiento,
                            ff.numero_fuente_financiamiento,
                            org.numero_organismo_financiador,
                            org.codigo_organismo_financiador
                    )
                    SELECT COUNT(*)
                    FROM (
                        SELECT 
                            t1.POI,
                            t1.CodigoCentroResposabilidad,
                            t1.CentroResponsabilidad,
                            t1.DescripcionMateria,
                            t1.CodigoMateria,
                            t1.Departamento,
                            t1.CodigoObjetoGasto,
                            t1.ObjetoGasto,
                            t3.""OF"",
                            t3.CodigoOrganismoFinanciador,
                            t3.FF,
                            t3.CodigoFuenteFinanciamiento,
                            t2.CantidadSolicitud,
                            t2.MontoSolicitud,
                            t2.CodigoCircunscripcion,
                            t3.CantidadConfiguracion,
                            t3.MontoConfiguracion
                        FROM 
                            t1
                        JOIN 
                            t2 ON t2.CodigoObjetoGasto = t1.CodigoObjetoGasto 
                            AND t2.CodigoCentroResposabilidad = t1.CodigoCentroResposabilidad 
                            AND t1.CodigoMateria = t2.CodigoMateria
                        LEFT JOIN 
                            t3 ON t3.CodigoObjetoGasto = t2.CodigoObjetoGasto 
                            AND t3.CodigoCentroResposabilidad = t2.CodigoCentroResposabilidad 
                            AND t3.CodigoMateria = t2.CodigoMateria
                       {filtrosAdicionales} 
                    ) AS TotalRegistros ";

            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@poi", request.Poi);
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);
            parametros.Add("@terminoDeBusqueda", request.TerminoDeBusqueda);

            // Añadir los parámetros correspondientes de búsqueda              
            parametros.Add("@parametroPOI", request.ParametroPoi);
            parametros.Add("@parametroCentroResponsabilidad", request.CentroResponsabilidad);
            parametros.Add("@parametroDescripcionMateria", request.DescripcionMateria);
            parametros.Add("@parametroDepartamento", request.Departamento);
            parametros.Add("@parametroObjetoGasto", request.ObjetoGasto);
            parametros.Add("@parametroCantidad", request.Cantidad);
            parametros.Add("@parametroMontoTotal", request.MontoTotal);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);         
            parametros.Add("@saltarRegistros", saltarRegistros);

            //Obtenemos datos de la Configuracion del Objeto que ya tenga algo de configuracion
            string stringConfiguracion = @"
                                             select 
                                             vr.CODIGO_VERSION as CodigoVersion,
                                             cn.CODIGO_CONFIGURACION_PRESUPUESTARIA as CodigoConfigPresupuestaria

                                             from VERSIONES_ANTEPROYECTOS vr JOIN CONFIGURACION_PRESUPUESTARIA cn 
                                             on vr.ejercicio = cn.ejercicio
                                             join VERSIONES_ANTEPROYECTOS_OBJETOS vao 
                                             on vr.CODIGO_VERSION = vao.CODIGO_VERSION
                                             and vao.CODIGO_CONFIGURACION_PRESUPUESTARIA = cn.CODIGO_CONFIGURACION_PRESUPUESTARIA

                                            WHERE vr.CODIGO_CIRCUNSCRIPCION = @codigoCircunscripcion
                                            AND cn.CODIGO_OBJETO_GASTO = @codigoObjetoGasto                                          
                                            AND VR.EJERCICIO = @ejercicio
                                            AND VR.version_consolidado = 0
                                            AND CN.codigo_centro_responsabilidad = @codigoCentro
                                            AND CN.codigo_materia = @codigoMateria";

            //Obtenemos Montos que corresponden a la configuracion
            string stringMontoConfiguracion = @"
                                           SELECT
                                            COALESCE(SUM(CONVERT(BIGINT, vab.CANTIDAD)), 0) AS CantidadConfiguracion,
                                            COALESCE(SUM(CONVERT(DECIMAL(18,2), vab.VALOR_UNITARIO) * CONVERT(BIGINT, vab.CANTIDAD)), 0) AS MontoConfiguracion
                                        FROM 
                                            VERSIONES_ANTEPROYECTOS_OBJETOS vao
                                        JOIN 
                                            VERSIONES_ANTEPROYECTOS_CONF_BIENES vab ON vab.CODIGO_VERSION = vao.CODIGO_VERSION AND vab.CODIGO_ANTEPROYECTO_OBJETO = vao.CODIGO_ANTEPROYECTO_OBJETO AND vab.SELECCIONADO = 1
                                        JOIN 
                                            CONFIGURACION_PRESUPUESTARIA cp ON cp.CODIGO_CONFIGURACION_PRESUPUESTARIA = vao.codigo_configuracion_presupuestaria
			                            join VERSIONES_ANTEPROYECTOS va on va.CODIGO_VERSION = vao.CODIGO_VERSION
		                            where 
			                            va.EJERCICIO = @ejercicioMonto 
			                            AND VAO.CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ
			                            AND VAO.CODIGO_VERSION = @versionanteproyecto
                                        AND vab.codigo_centro_responsabilidad = @codigoCentro
	                                    AND vab.codigo_materia = @codigoMateria
			                            AND VAO.CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoconfigpres
 
                                        GROUP BY
                                            cp.codigo_centro_responsabilidad,
                                            cp.codigo_materia,
                                            cp.codigo_objeto_gasto,
			                                vao.codigo_fuente_financiamiento,
			                                vao.codigo_organismo_financiador";

            //Obtenemos montos que corresponden a la solicitud       
            string stringMontoSolicitud = @"
                                        select 
		                                    isnull(SUM(CONVERT(BIGINT, sol.cantidad)),0) AS CantidadSolicitud,
                                            isnull(SUM(CONVERT(BIGINT, sol.cantidad) * CONVERT(DECIMAL(18), sol.costo_unitario)),0) AS TotalSolicitud   
	                                    from solicitudes_objetos_bienes_detalle sol
	                                    join solicitudes_objetos_detalle obj 
	                                    on sol.codigo_solicitud = obj.codigo_solicitud
	                                    and sol.codigo_solicitud_objeto = obj.codigo_solicitud_objeto
	                                    join solicitudes_bienes_circunscripcion cab on
	                                    cab.codigo_solicitud = obj.codigo_solicitud  

	                                    where obj.estado = 2
	                                    and obj.codigo_objeto_gasto = @codigoOBG
	                                    and cab.codigo_centro_responsabilidad = @codigo_centro_responsabilidad
	                                    and cab.codigo_materia =  @codigo_materia
                                        and cab.ejercicio = @ejercicio";                        

            //Obtenemos el Codigo Anteproyecto de la configuracion
            string stringAnteproyectoObjeto = @"
                                         SELECT COALESCE(sum(VAO.CODIGO_ANTEPROYECTO_OBJETO),0)
                                            FROM VERSIONES_ANTEPROYECTOS_OBJETOS VAO
                                            WHERE
	                                            VAO.CODIGO_VERSION = @codigoVersionAnteproyecto
                                            AND VAO.CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria
                                            AND VAO.CODIGO_FUENTE_FINANCIAMIENTO = @codigoFF
                                            AND VAO.CODIGO_ORGANISMO_FINANCIADOR = @codigoOF";


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QueryAsync<dynamic>(query, parametros);

                var listado = new List<ConfiguracionPresupuestariaPorObjetosGastosDTO>();
                string objetoGastoActual = string.Empty;
                int codigoMateriaActual = 0;
                int codigoCentrodeResponsabilidadActual = 0;

                var configuracion = new Configuracion();

                foreach (var item in resultado)
                {
                    var poi = item.POI.ToString();
                    var departamento = item.Departamento.ToString();
                    var codigoCentrodeResponsabilidad = item.CodigoCentroResposabilidad;
                    var descripcionMateria = item.DescripcionMateria;
                    var centroResponsabilidad = item.CentroResponsabilidad.ToString();
                    var codigoObjetoGasto = item.CodigoObjetoGasto;
                    var objetoGasto = item.ObjetoGasto.ToString();
                    var of = item.OF;
                    var ff = item.FF;

                    var codigoCircunscripcion = item.CodigoCircunscripcion;
                    var codigoOrgFinanciador = item.CodigoOrganismoFinanciador;
                    var codigoFuenteFinanciacion = item.CodigoFuenteFinanciamiento;
                    var codigoMateria = item.CodigoMateria;

                    var parametrosConfiguracion = new DynamicParameters();
                    parametrosConfiguracion.Add("@codigoObjetoGasto", codigoObjetoGasto);
                    parametrosConfiguracion.Add("@codigoCircunscripcion", codigoCircunscripcion);
                    parametrosConfiguracion.Add("@ejercicio", request.ParametroPoi);
                    parametrosConfiguracion.Add("@codigoCentro", codigoCentrodeResponsabilidad);
                    parametrosConfiguracion.Add("@codigoMateria", codigoMateria);

                    var configuracionPresupuestaria = await connection.QueryAsync<dynamic>(stringConfiguracion, parametrosConfiguracion);

                    configuracion = configuracionPresupuestaria.Select(c => new Configuracion
                    {
                        CodigoVersion = c.CodigoVersion,
                        CodigoConfigPresupuestaria = c.CodigoConfigPresupuestaria,
                        //CodigoAnteproyectoObjeto = c.CodigoAnteproyectoObjeto                     
                    }).FirstOrDefault() ?? new Configuracion();


                    //Obtenemos el CodigoAnteproyectoObjeto
                    var parametrosAnteproyectoObjeto = new DynamicParameters();
                    parametrosAnteproyectoObjeto.Add("@codigoVersionAnteproyecto", configuracion.CodigoVersion);
                    parametrosAnteproyectoObjeto.Add("@codigoConfigPresupuestaria", configuracion.CodigoConfigPresupuestaria);
                    parametrosAnteproyectoObjeto.Add("@codigoOF", codigoOrgFinanciador);
                    parametrosAnteproyectoObjeto.Add("@codigoFF", codigoFuenteFinanciacion);

                    var resultadoAnteproyectoOBG = await connection.QueryAsync<int>(stringAnteproyectoObjeto, parametrosAnteproyectoObjeto);                   
                    var codigoAnteproyectoObjeto = resultadoAnteproyectoOBG.FirstOrDefault();

                    //Obtenemos los datos para los montos de la configuracion
                    var parametrosMontoConfiguracion = new DynamicParameters();
                    parametrosMontoConfiguracion.Add("@versionanteproyecto", configuracion.CodigoVersion);
                    parametrosMontoConfiguracion.Add("@codigoconfigpres", configuracion.CodigoConfigPresupuestaria);
                    parametrosMontoConfiguracion.Add("@codigoAnteproyectoOBJ", codigoAnteproyectoObjeto);
                    parametrosMontoConfiguracion.Add("@ejercicioMonto", request.ParametroPoi);
                    parametrosMontoConfiguracion.Add("@codigoCentro", codigoCentrodeResponsabilidad);
                    parametrosMontoConfiguracion.Add("@codigoMateria", codigoMateria);

                    var montoConfigPresu = await connection.QueryAsync<dynamic>(stringMontoConfiguracion, parametrosMontoConfiguracion);

                    var cantidadConfiguracion = montoConfigPresu.FirstOrDefault()?.CantidadConfiguracion;
                    var montoConfiguracion = montoConfigPresu.FirstOrDefault()?.MontoConfiguracion;

                    //Obtenemos los datos para los montos de la solicitud
                    var parametrosMontoSolicitud = new DynamicParameters();
                    parametrosMontoSolicitud.Add("@codigoOBG", codigoObjetoGasto);
                    parametrosMontoSolicitud.Add("@ejercicioSolicitud", request.ParametroPoi);
                    parametrosMontoSolicitud.Add("@codigo_materia", codigoMateria);
                    parametrosMontoSolicitud.Add("@codigo_centro_responsabilidad", codigoCentrodeResponsabilidad);
                    parametrosMontoSolicitud.Add("@ejercicio", request.ParametroPoi);

                    var montoTotalSolicitud = await connection.QueryAsync<dynamic>(stringMontoSolicitud, parametrosMontoSolicitud);

                    var cantidadSolicitud = montoTotalSolicitud.FirstOrDefault()?.CantidadSolicitud;
                    var montoSolicitud = montoTotalSolicitud.FirstOrDefault()?.TotalSolicitud;

                    // Verifica si el objeto de gasto actual es igual al anterior
                    if (objetoGastoActual == objetoGasto)
                    {                      
                        if (codigoMateriaActual != codigoMateria && codigoCentrodeResponsabilidadActual != codigoCentrodeResponsabilidad)
                        {
                            // Si el objeto de gasto ha cambiado, crea una nueva configuración presupuestaria con un nuevo financiamiento
                            objetoGastoActual = objetoGasto;
                            codigoMateriaActual = codigoMateria;
                            codigoCentrodeResponsabilidadActual = codigoCentrodeResponsabilidad;

                            var nuevoFinanciamiento = new Financiamiento
                            {
                                CodigoAnteproyectoObjeto = codigoAnteproyectoObjeto,
                                CodigoFuenteFinanciamiento = codigoFuenteFinanciacion,
                                CodigoOrganismoFinanciador = codigoOrgFinanciador,
                                OF = of,
                                FF = ff,
                                CantidadConfiguracion = cantidadConfiguracion,
                                MontoConfiguracion = montoConfiguracion
                            };


                            var nuevaConfiguracionPresupuestaria = new ConfiguracionPresupuestariaPorObjetosGastosDTO
                            {
                                Poi = poi,
                                Departamento = departamento,
                                CodigoCentroResposabilidad = codigoCentrodeResponsabilidad,
                                CodigoMateria = codigoMateria,
                                DescripcionMateria = descripcionMateria,
                                CentroResponsabilidad = centroResponsabilidad,
                                CodigoObjetoGasto = codigoObjetoGasto,
                                ObjetoGasto = objetoGasto,
                                Financiamientos = new List<Financiamiento> { nuevoFinanciamiento },
                                CantidadSolicitud = cantidadSolicitud ?? 0,
                                MontoSolicitud = montoSolicitud ?? 0,
                                Configuracion = codigoAnteproyectoObjeto != 0 ? configuracion : null
                            };

                            // Agrega la nueva configuración presupuestaria a la lista
                            listado.Add(nuevaConfiguracionPresupuestaria);
                        }
                        else if(codigoMateriaActual != codigoMateria && codigoCentrodeResponsabilidadActual == codigoCentrodeResponsabilidad)
                        {
                            // Si el objeto de gasto ha cambiado, crea una nueva configuración presupuestaria con un nuevo financiamiento
                            objetoGastoActual = objetoGasto;
                            codigoMateriaActual = codigoMateria;
                            codigoCentrodeResponsabilidadActual = codigoCentrodeResponsabilidad;

                            var nuevoFinanciamiento = new Financiamiento
                            {
                                CodigoAnteproyectoObjeto = codigoAnteproyectoObjeto,
                                CodigoFuenteFinanciamiento = codigoFuenteFinanciacion,
                                CodigoOrganismoFinanciador = codigoOrgFinanciador,
                                OF = of,
                                FF = ff,
                                CantidadConfiguracion = cantidadConfiguracion,
                                MontoConfiguracion = montoConfiguracion
                            };


                            var nuevaConfiguracionPresupuestaria = new ConfiguracionPresupuestariaPorObjetosGastosDTO
                            {
                                Poi = poi,
                                Departamento = departamento,
                                CodigoCentroResposabilidad = codigoCentrodeResponsabilidad,
                                CodigoMateria = codigoMateria,
                                DescripcionMateria = descripcionMateria,
                                CentroResponsabilidad = centroResponsabilidad,
                                CodigoObjetoGasto = codigoObjetoGasto,
                                ObjetoGasto = objetoGasto,
                                Financiamientos = new List<Financiamiento> { nuevoFinanciamiento },
                                CantidadSolicitud = cantidadSolicitud ?? 0,
                                MontoSolicitud = montoSolicitud ?? 0,
                                Configuracion = codigoAnteproyectoObjeto != 0 ? configuracion : null
                            };

                            // Agrega la nueva configuración presupuestaria a la lista
                            listado.Add(nuevaConfiguracionPresupuestaria);
                        }
                        else if(codigoMateriaActual == codigoMateria && codigoCentrodeResponsabilidadActual != codigoCentrodeResponsabilidad)
                        {
                            // Si el objeto de gasto ha cambiado, crea una nueva configuración presupuestaria con un nuevo financiamiento
                            objetoGastoActual = objetoGasto;
                            codigoMateriaActual = codigoMateria;
                            codigoCentrodeResponsabilidadActual = codigoCentrodeResponsabilidad;

                            var nuevoFinanciamiento = new Financiamiento
                            {
                                CodigoAnteproyectoObjeto = codigoAnteproyectoObjeto,
                                CodigoFuenteFinanciamiento = codigoFuenteFinanciacion,
                                CodigoOrganismoFinanciador = codigoOrgFinanciador,
                                OF = of,
                                FF = ff,
                                CantidadConfiguracion = cantidadConfiguracion,
                                MontoConfiguracion = montoConfiguracion
                            };


                            var nuevaConfiguracionPresupuestaria = new ConfiguracionPresupuestariaPorObjetosGastosDTO
                            {
                                Poi = poi,
                                Departamento = departamento,
                                CodigoCentroResposabilidad = codigoCentrodeResponsabilidad,
                                CodigoMateria = codigoMateria,
                                DescripcionMateria = descripcionMateria,
                                CentroResponsabilidad = centroResponsabilidad,
                                CodigoObjetoGasto = codigoObjetoGasto,
                                ObjetoGasto = objetoGasto,
                                Financiamientos = new List<Financiamiento> { nuevoFinanciamiento },
                                CantidadSolicitud = cantidadSolicitud ?? 0,
                                MontoSolicitud = montoSolicitud ?? 0,
                                Configuracion = codigoAnteproyectoObjeto != 0 ? configuracion : null
                            };

                            // Agrega la nueva configuración presupuestaria a la lista
                            listado.Add(nuevaConfiguracionPresupuestaria);
                        }
                        else if(codigoMateriaActual == codigoMateria && codigoCentrodeResponsabilidadActual == codigoCentrodeResponsabilidad)
                        {
                            // Si es el mismo objeto de gasto, simplemente agrega un nuevo financiamiento a la lista existente
                            var nuevoFinanciamiento = new Financiamiento
                            {
                                CodigoAnteproyectoObjeto = codigoAnteproyectoObjeto,
                                CodigoFuenteFinanciamiento = codigoFuenteFinanciacion,
                                CodigoOrganismoFinanciador = codigoOrgFinanciador,
                                OF = of,
                                FF = ff,
                                CantidadConfiguracion = cantidadConfiguracion,
                                MontoConfiguracion = montoConfiguracion
                            };

                            // Agrega el nuevo financiamiento a la última configuración presupuestaria agregada
                            var ultimaConfiguracion = listado.Last();
                            ultimaConfiguracion.Financiamientos.Add(nuevoFinanciamiento);
                        }
                    }                  
                    else
                    {
                        // Si el objeto de gasto ha cambiado, crea una nueva configuración presupuestaria con un nuevo financiamiento
                        objetoGastoActual = objetoGasto;
                        codigoMateriaActual = codigoMateria;
                        codigoCentrodeResponsabilidadActual = codigoCentrodeResponsabilidad;

                        var nuevoFinanciamiento = new Financiamiento
                        {
                            CodigoAnteproyectoObjeto = codigoAnteproyectoObjeto,
                            CodigoFuenteFinanciamiento = codigoFuenteFinanciacion,
                            CodigoOrganismoFinanciador = codigoOrgFinanciador,
                            OF = of,
                            FF = ff,
                            CantidadConfiguracion = cantidadConfiguracion,
                            MontoConfiguracion = montoConfiguracion
                        };


                        var nuevaConfiguracionPresupuestaria = new ConfiguracionPresupuestariaPorObjetosGastosDTO
                        {
                            Poi = poi,
                            Departamento = departamento,
                            CodigoCentroResposabilidad = codigoCentrodeResponsabilidad,
                            CodigoMateria = codigoMateria,
                            DescripcionMateria = descripcionMateria,
                            CentroResponsabilidad = centroResponsabilidad,
                            CodigoObjetoGasto = codigoObjetoGasto,
                            ObjetoGasto = objetoGasto,
                            Financiamientos = new List<Financiamiento> { nuevoFinanciamiento },
                            CantidadSolicitud = cantidadSolicitud ?? 0,
                            MontoSolicitud = montoSolicitud ?? 0m,
                            Configuracion = codigoAnteproyectoObjeto != 0 ? configuracion : null
                        };

                        // Agrega la nueva configuración presupuestaria a la lista
                        listado.Add(nuevaConfiguracionPresupuestaria);
                    }               
                }

                var response = new Datos<IEnumerable<ConfiguracionPresupuestariaPorObjetosGastosDTO>>
                {
                    Items = listado,
                    TotalRegistros = totalTegistros
                };

                _logger.LogInformation("Fin de Proceso de la Copnfiguracion Presupuestaria por Objeto de Gastos");
                return response;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al realizar la Copnfiguracion Presupuestaria por Objeto de Gastos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<Datos<DatosdeConfiguracionPresupuestariaDTO>> InsertarCabeceraparaConfiguracionPresupuestaria(DatosparaCabeceraConfiguracionPresupuestariaRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de insertar la configuracion presupuestaria por Objeto de Gastos");

        //INSERTAMOS LA TABLA VERSIONES ANTEPROYECTOS
        string query_VersionAnteproyecto = @"INSERT INTO VERSIONES_ANTEPROYECTOS
                         (CODIGO_VERSION, NUMERO_VERSION, CODIGO_CIRCUNSCRIPCION, EJERCICIO, ESTADO, USUARIO_INSERTO, FECHA_INSERTO,version_consolidado,es_ley)
                          VALUES (@codigoVersion, @numeroVersion, @codigoCircunscripcion, @ejercicio, 1, @usuarioInserto, GETDATE(),0,0)";


        string query_UltimoValorCodigoSinelConsolidado = "SELECT ISNULL(MAX(CODIGO_VERSION), 0) FROM VERSIONES_ANTEPROYECTOS where version_consolidado = 0";

        string query_UltimoValorCodigoConConsolidado = "SELECT ISNULL(MAX(CODIGO_VERSION), 0) FROM VERSIONES_ANTEPROYECTOS where version_consolidado = 1";

        //INSERTAMOS LA TABLA CONFIGURACION PRESUPUESTARIA
        string query_ConfigPresupuestaria = @"INSERT INTO CONFIGURACION_PRESUPUESTARIA
                     (CODIGO_CONFIGURACION_PRESUPUESTARIA, CODIGO_OBJETO_GASTO, CODIGO_PROGRAMA, CODIGO_ACTIVIDAD, CODIGO_TIPO_PRESUPUESTO, CODIGO_DEPARTAMENTO, GRUPO, CODIGO_CENTRO_RESPONSABILIDAD, CODIGO_MATERIA, SUBGRUPO,ejercicio)
                      VALUES (@codigoConfigPres, @codigoObjetoGasto, @codigoPrograma, @codigoActividad, @codigoTipoPresupuesto, @codigoDepartamento, @grupo, @codigoCentroResponsabilidad, @codigoMateria, @subGrupo,@ejercicio)";

        string query_UltimoValorCodigoConfigPresupuestaria = "SELECT ISNULL(MAX(CODIGO_CONFIGURACION_PRESUPUESTARIA), 0) FROM CONFIGURACION_PRESUPUESTARIA";

        string queryValidarEjercicioSinConsolidado = @"
                                        select count(1) 
                            from VERSIONES_ANTEPROYECTOS vr 
                            WHERE version_consolidado = 0
                            AND vr.EJERCICIO 
                            in (select fc.EJERCICIO from VERSION_FECHA_CIERRE fc WHERE fc.ACTIVO = 1)";

        //Validamos si ya existe una configuracion presupuestaria para ese objeto
        string validarExisteConfiguracion = @"
                                    SELECT count(*)
                                    FROM CONFIGURACION_PRESUPUESTARIA cn  join VERSIONES_ANTEPROYECTOS va
                                    on cn.EJERCICIO = va.EJERCICIO
                                    join VERSIONES_ANTEPROYECTOS_OBJETOS vao on va.CODIGO_VERSION = vao.CODIGO_VERSION
                                    and vao.CODIGO_CONFIGURACION_PRESUPUESTARIA = cn.CODIGO_CONFIGURACION_PRESUPUESTARIA

                                    WHERE CN.CODIGO_OBJETO_GASTO = @codigoObjetoGasto
                                    AND CN.CODIGO_CENTRO_RESPONSABILIDAD = @codigoCentroResponsabilidad
                                    AND CN.CODIGO_MATERIA = @codigoMateria
                                    AND CN.EJERCICIO = @ejercicio
                                    AND va.VERSION_CONSOLIDADO = 0";



        //Obtenemo el Codigo de la Circunscripcion para lo que se necesite
        var codCircunscripcion = 0;
        if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("codCircunscripcion"))
        {           
            var dbUser = _httpContextAccessor.HttpContext.Request.Headers["codCircunscripcion"].ToString().Trim();
            if (!string.IsNullOrEmpty(dbUser))
            {
                codCircunscripcion = int.Parse(dbUser);             
            }
        }

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                //Validamos que exista una version para el ejercicio actual
                var existeEjerciciosinConsolidado = await connection.ExecuteScalarAsync<int>(queryValidarEjercicioSinConsolidado);

                var parametros = new DynamicParameters();

                // Obtenemos el valor sin el Consolidado
                int valorCodigoVersionesConConsolidado = await connection.ExecuteScalarAsync<int>(query_UltimoValorCodigoSinelConsolidado);                

                //Validar sin Circunscripcion es Capital 0 Sucursales
                var valorparaConsolidado = string.Empty;
                if (codCircunscripcion != 1)               
                    valorparaConsolidado = query_UltimoValorCodigoSinelConsolidado;                
                else
                    valorparaConsolidado = query_UltimoValorCodigoConConsolidado;


                // Generamos los valores para registrar la tabla Versiones Anteproyectos
                int valorCodigoVersiones = await connection.ExecuteScalarAsync<int>(valorparaConsolidado);
                int codigoVersiones = valorCodigoVersiones + 1;
                parametros.Add("@codigoVersion", codigoVersiones);
                parametros.Add("@numeroVersion", codigoVersiones);
                parametros.Add("@codigoCircunscripcion", request.CodigoCircunscripcion);
                parametros.Add("@usuarioInserto", request.UsuarioInserto);
                parametros.Add("@ejercicio", request.Ejercicio);

                // Generamos los valores para registrar la tabla Configuracion Presupuestaria
                int valorCodigoConfiguracion = await connection.ExecuteScalarAsync<int>(query_UltimoValorCodigoConfigPresupuestaria);
                int codigoConfigPres = valorCodigoConfiguracion + 1;
                parametros.Add("@codigoConfigPres", codigoConfigPres);
                parametros.Add("@codigoObjetoGasto", request.CodigoObjetoGasto);
                parametros.Add("@codigoPrograma", request.CodigoPrograma);
                parametros.Add("@codigoActividad", request.CodigoActividad);
                parametros.Add("@codigoTipoPresupuesto", request.CodigoTipoPresupuesto);
                parametros.Add("@codigoDepartamento", request.CodigoDepartamento);
                parametros.Add("@grupo", request.Grupo);
                parametros.Add("@codigoCentroResponsabilidad", request.CodigoCentroResponsabilidad);
                parametros.Add("@codigoMateria", request.CodigoMateria);
                parametros.Add("@subGrupo", request.SubGrupo);

                //Utilizamos para Validar antes del insertado de la configuracion Presupuestaria
                int existeConfiguracion = await connection.ExecuteScalarAsync<int>(validarExisteConfiguracion,parametros);

                //Si ya existe una version abierta para el ejercicio actual ya no se crea una nueva
                if (existeEjerciciosinConsolidado > 0)
                {
                    codigoVersiones = valorCodigoVersionesConConsolidado;
                }
                else
                {
                    //insertamos la Cabecera
                    await connection.ExecuteAsync(query_VersionAnteproyecto, parametros);
                }

                //si ya existe una configuracion para ese objeto, materia y centro en el ejercicio actual, ya no se crea otro para ese objeto
                if(existeConfiguracion > 0)
                {
                    codigoConfigPres = valorCodigoConfiguracion;
                }
                else
                {
                    //insertamos la Configuracion
                    await connection.ExecuteAsync(query_ConfigPresupuestaria, parametros);
                }
               
                var resultadoInsertado = new DatosdeConfiguracionPresupuestariaDTO
                {
                    CodigoVersion = codigoVersiones,
                    CodigoConfiguracionPresupuestaria = codigoConfigPres
                };

                var listado = new Datos<DatosdeConfiguracionPresupuestariaDTO>
                {
                    Items = resultadoInsertado,
                    TotalRegistros = 1
                };

                _logger.LogInformation("Fin de Proceso de insertar la configuracion presupuestaria por Objeto de Gastos");

                return listado;
            }
        }
        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al insertar la Configuracion Presupuestaria por Objeto de Gastos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<Datos<int>> InsertarAnteproyectoObjetos(DatosparaAnteproyectoObjetosRequest request, PresupuestoInicialyModificacionesConsolidadoDTO montos)
    {
        _logger.LogInformation("Inicio de Proceso de insertar la Tabla de Anteproyecto Objetos");

        //INSERTAMOS EN LA TABLA VERSIONES ANTEPROYECTO OBJETOS
        string query_VersionesAnteproyectoObjetos = @"INSERT INTO VERSIONES_ANTEPROYECTOS_OBJETOS
                     (CODIGO_ANTEPROYECTO_OBJETO, CODIGO_VERSION, CODIGO_CONFIGURACION_PRESUPUESTARIA, CODIGO_FUENTE_FINANCIAMIENTO, CODIGO_ORGANISMO_FINANCIADOR, EVENTO, PRESUPUESTO_INICIAL, MODIFICACIONES, MONTO_PLANIFICADO, codigo_circunscripcion_origen)
                      VALUES (@codigoAnteproyectoObjeto, @codigoVersion, @codigoConfigPres, @codigoFuenteFinanciamiento, @codigoOrganismoFinanciador, @evento, @presupuestoInicial, @modificacion, @montoPlanificado, @circunscripcionOrigen)";

        string query_UltimoValorCodigoVersionesAnteproyectoObjetos = "SELECT ISNULL(MAX(CODIGO_ANTEPROYECTO_OBJETO), 0) FROM VERSIONES_ANTEPROYECTOS_OBJETOS " +
                          "WHERE CODIGO_VERSION = @codigoVersion";      
        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {   
                var parametros = new DynamicParameters();
                // Generamos los valores para registrar la tabla Versiones Anteproyecto Objetos
                int valorVersionesAnteproyectoOBG = await connection.ExecuteScalarAsync<int>(query_UltimoValorCodigoVersionesAnteproyectoObjetos,
                    new { codigoVersion = request.CodigoVersionAnteproyecto, codigoConfigPres = request.CodigoConfiguracionPresupuestaria });
                int codigoAnteproyectoOBG = valorVersionesAnteproyectoOBG + 1;
                parametros.Add("@codigoAnteproyectoObjeto", codigoAnteproyectoOBG);
                parametros.Add("@codigoFuenteFinanciamiento", request.CodigoFuenteFinanciamiento);
                parametros.Add("@codigoOrganismoFinanciador", request.CodigoOrganismoFinanciador);
                parametros.Add("@evento", request.Evento);
                parametros.Add("@circunscripcionOrigen", request.CodigoCircunscripcionOrigen);


                parametros.Add("@presupuestoInicial", montos.PresupuestoInicial);
                parametros.Add("@modificacion", montos.Modificaciones);

                parametros.Add("@montoPlanificado", 0);
                parametros.Add("@codigoVersion", request.CodigoVersionAnteproyecto);
                parametros.Add("@codigoConfigPres", request.CodigoConfiguracionPresupuestaria);

                var resultado_VersionesAnteproyectoOBG = await connection.ExecuteAsync(query_VersionesAnteproyectoObjetos, parametros);

                var resultado = new Datos<int>
                {
                    Items = codigoAnteproyectoOBG,
                    TotalRegistros = 1
                };

                _logger.LogInformation("Fin de Proceso de insertar la Tabla de Anteproyecto Objetos");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al insertar la Tabla de Anteproyecto Objetos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<Datos<int>> InsertarBienesporConfiguracionPresupuestariaConsolidado(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion)
    {
        _logger.LogInformation("Inicio de Proceso de insertar la configuracion presupuestaria para los Bienes");

        var retornoFinal = 0;
        var listado = new Datos<int> { };

        //INSERTAMOS EN LA TABLA VERSIONES ANTEPROYECTO BIENES (consolidado)
        string query_VersionesAnteproyectoBienes = @"INSERT INTO VERSIONES_ANTEPROYECTOS_BIENES
                               (CODIGO_ANTEPROYECTO_BIEN,CODIGO_ANTEPROYECTO_OBJETO,CODIGO_VERSION,NUMERO_BIEN,DESCRIPCION_BIEN,CANTIDAD
                               ,VALOR_UNITARIO,USUARIO_INSERTO,FECHA_INSERTO,UNIDAD_MEDIDA,FUNDAMENTACION,SELECCIONADO)
                                VALUES
                                (@codigoAnteproyectoBien,@codigoAnteproyectoObjeto,@codigoVersion,@numeroBien,@descripcionBien,@cantidad,
                                 @valorUnitario,@usuarioInserto,GETDATE(),@unidadMedida,@fundamentacion,@seleccionado)";

        string query_UltimoValorCodigoVersionesAnteproyectoBienes = @"SELECT ISNULL(MAX(CODIGO_ANTEPROYECTO_BIEN),0) 
                                FROM VERSIONES_ANTEPROYECTOS_BIENES
                                WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoObjeto 
                                AND CODIGO_VERSION = @codigoVersion";

        var validarExiste = @"SELECT COUNT(*) 
                            FROM VERSIONES_ANTEPROYECTOS_BIENES 
                            WHERE CODIGO_VERSION = @codigoVersion                             
                            AND NUMERO_BIEN = @numeroBien";

        var cantidadBien = @"SELECT CANTIDAD 
                            FROM VERSIONES_ANTEPROYECTOS_BIENES 
                            WHERE CODIGO_VERSION = @codigoVersion                             
                            AND NUMERO_BIEN = @numeroBien";

        var actualizarCantidadBien = @"UPDATE VERSIONES_ANTEPROYECTOS_BIENES
                            SET CANTIDAD = @cantidad,
                                CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBG
                            WHERE CODIGO_VERSION = @version                             
                            AND NUMERO_BIEN = @nroBien";

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var parametros = new DynamicParameters();

                // Verificamos si ya existe el registro
                int existe = await connection.ExecuteScalarAsync<int>(validarExiste,new
                {
                    codigoAnteproyectoObjeto = CodigoAnteproyectoObj,
                    codigoVersion = CodigoVersion,
                    numeroBien = request.NumeroBien
                });

                // Obtenemos la Cantidad del Bien
                int cantidad = await connection.ExecuteScalarAsync<int>(cantidadBien, new
                {
                    codigoAnteproyectoObjeto = CodigoAnteproyectoObj,
                    codigoVersion = CodigoVersion,
                    numeroBien = request.NumeroBien
                });

                if (existe > 0)
                {
                    var cantidadSumado = cantidad + request.Cantidad;
                    await connection.ExecuteAsync(actualizarCantidadBien, new 
                    { 
                        cantidad = cantidadSumado,
                        version = CodigoVersion,
                        nroBien = request.NumeroBien,
                        codigoAnteproyectoOBG = CodigoAnteproyectoObj
                    });
                }
                else
                {
                    // Generamos los valores para registrar la tabla Versiones Anteproyecto Bienes
                    int valorVersionesAnteproyectoBienes = await connection.ExecuteScalarAsync<int>(
                        query_UltimoValorCodigoVersionesAnteproyectoBienes,
                        new { codigoAnteproyectoObjeto = CodigoAnteproyectoObj, codigoVersion = CodigoVersion });

                    int codigoAnteproyectoBienes = valorVersionesAnteproyectoBienes + 1;

                    // Asignamos los parámetros
                    parametros.Add("@codigoAnteproyectoBien", codigoAnteproyectoBienes);
                    parametros.Add("@codigoAnteproyectoObjeto", CodigoAnteproyectoObj);
                    parametros.Add("@codigoVersion", CodigoVersion);
                    parametros.Add("@numeroBien", request.NumeroBien);
                    parametros.Add("@descripcionBien", request.DescripcionBien);
                    parametros.Add("@cantidad", request.Cantidad);
                    parametros.Add("@valorUnitario", request.ValorUnitario);
                    parametros.Add("@usuarioInserto", request.UsuarioInserto); // Este parámetro faltaba en la versión original
                    parametros.Add("@unidadMedida", request.UnidadMedida);
                    parametros.Add("@fundamentacion", request.Fundamentacion);
                    parametros.Add("@seleccionado", request.Seleccionado);

                    var resultado_VersionesAnteproyectoBienes = await connection.ExecuteAsync(query_VersionesAnteproyectoBienes, parametros);

                    listado = new Datos<int>
                    {
                        Items = resultado_VersionesAnteproyectoBienes,
                        TotalRegistros = 1
                    };             
                }

                _logger.LogInformation("Fin de Proceso de insertar la configuracion presupuestaria para los Bienes");
                return listado;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al insertar la Configuracion Presupuestaria por Bienes" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<Datos<int>> InsertarBienesporConfiguracionPresupuestariaNoConsolidado(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion)
    {
        _logger.LogInformation("Inicio de Proceso de insertar la configuracion presupuestaria para los Bienes en la tabla de Versiones bienes no consolidado");

        var retornoFinal = 0;
        var listado = new Datos<int> { };

        //INSERTAMOS EN LA TABLA VERSIONES ANTEPROYECTO BIENES (no consolidado)
        string query_VersionesAnteproyectoBienes = @"INSERT INTO versiones_anteproyectos_conf_bienes
                               (CODIGO_ANTEPROYECTO_BIEN,CODIGO_ANTEPROYECTO_OBJETO,CODIGO_VERSION,NUMERO_BIEN,DESCRIPCION_BIEN,CANTIDAD
                               ,VALOR_UNITARIO,USUARIO_INSERTO,FECHA_INSERTO,UNIDAD_MEDIDA,FUNDAMENTACION,SELECCIONADO,codigo_centro_responsabilidad,codigo_materia)
                                VALUES
                                (@codigoAnteproyectoBien,@codigoAnteproyectoObjeto,@codigoVersion,@numeroBien,@descripcionBien,@cantidad,
                                 @valorUnitario,@usuarioInserto,GETDATE(),@unidadMedida,@fundamentacion,@seleccionado,@codigoCentroResp,@codigoMateria)";

        string query_UltimoValorCodigoVersionesAnteproyectoBienes = "SELECT ISNULL(MAX(CODIGO_ANTEPROYECTO_BIEN),0) FROM versiones_anteproyectos_conf_bienes " +
                            "WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoObjeto AND CODIGO_VERSION = @codigoVersion";

        var validarExiste = @"SELECT COUNT(*) FROM versiones_anteproyectos_conf_bienes WHERE CODIGO_VERSION = @codigoVersion AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoObjeto AND NUMERO_BIEN = @numeroBien";
        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var parametros = new DynamicParameters();

                // Verificamos si ya existe el registro
                int existe = await connection.ExecuteScalarAsync<int>(validarExiste, new
                {
                    codigoAnteproyectoObjeto = CodigoAnteproyectoObj,
                    codigoVersion = CodigoVersion,
                    numeroBien = request.NumeroBien
                });

                if (existe > 0)
                {
                    listado = new Datos<int>
                    {
                        Items = -1,
                        TotalRegistros = 0
                    };
                }
                else
                {
                    // Generamos los valores para registrar la tabla Versiones Anteproyecto Bienes
                    int valorVersionesAnteproyectoBienes = await connection.ExecuteScalarAsync<int>(
                        query_UltimoValorCodigoVersionesAnteproyectoBienes,
                        new { codigoAnteproyectoObjeto = CodigoAnteproyectoObj, codigoVersion = CodigoVersion });

                    int codigoAnteproyectoBienes = valorVersionesAnteproyectoBienes + 1;

                    // Asignamos los parámetros
                    parametros.Add("@codigoAnteproyectoBien", codigoAnteproyectoBienes);
                    parametros.Add("@codigoAnteproyectoObjeto", CodigoAnteproyectoObj);
                    parametros.Add("@codigoVersion", CodigoVersion);
                    parametros.Add("@numeroBien", request.NumeroBien);
                    parametros.Add("@descripcionBien", request.DescripcionBien);
                    parametros.Add("@cantidad", request.Cantidad);
                    parametros.Add("@valorUnitario", request.ValorUnitario);
                    parametros.Add("@usuarioInserto", request.UsuarioInserto); // Este parámetro faltaba en la versión original
                    parametros.Add("@unidadMedida", request.UnidadMedida);
                    parametros.Add("@fundamentacion", request.Fundamentacion);
                    parametros.Add("@seleccionado", request.Seleccionado);
                    parametros.Add("@codigoCentroResp", request.CodigoCentroResponsabilidad);
                    parametros.Add("@codigoMateria", request.CodigoMateria);

                    var resultado_VersionesAnteproyectoBienes = await connection.ExecuteAsync(query_VersionesAnteproyectoBienes, parametros);

                    listado = new Datos<int>
                    {
                        Items = resultado_VersionesAnteproyectoBienes,
                        TotalRegistros = 1
                    };
                }

                _logger.LogInformation("Fin de Proceso de insertar la configuracion presupuestaria para los Bienes en la tabla de Versiones bienes no consolidado");
                return listado;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al insertar la Configuracion Presupuestaria por Bienes en la tabla de Versiones bienes no consolidado" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<List<BienesparaConfiguracionPresupuestariaDTO>> ObtenerBienesparaAgregarFinanciamiento(BienesparaConfiguracionPresupuestariaRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de Obtener Lista de Bienes para la Configuracion Presupuestaria");

        try
        {
            string query = @"
                        SELECT
                                sobd.numero_bien AS NumeroBien,
                                MAX(sobd.descripcion) AS Descripcion,  
                                ISNULL(SUM(CONVERT(BIGINT, sobd.cantidad)), 0) AS Cantidad,
                                ISNULL(AVG(CONVERT(DECIMAL(18), sobd.costo_unitario)), 0) AS ValorUnitario,
                                ISNULL(SUM(CONVERT(BIGINT, sobd.cantidad) * CONVERT(DECIMAL(18), sobd.costo_unitario)), 0) AS Total,
                                MAX(sobd.fundamentacion) AS Fundamentacion, 
                                MAX(vlb.codigo_unidad_medida) AS UnidadMedida,  
                                'false' AS Seleccionado
                            FROM
                                solicitudes_objetos_bienes_detalle sobd
                                JOIN solicitudes_objetos_detalle sod ON sobd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto
                                AND sobd.codigo_solicitud = sod.codigo_solicitud
                                JOIN solicitudes_bienes_circunscripcion sbc ON sobd.codigo_solicitud = sbc.codigo_solicitud
                                JOIN vListaBienesPrioritarios vlb ON vlb.codigo_catalogo = sobd.numero_bien
                                AND vlb.codigo_objeto_gasto = sod.codigo_objeto_gasto
                                AND vlb.codigo_circunscripcion = sbc.codigo_circunscripcion

                            WHERE
                                sod.codigo_objeto_gasto = @parametroObjetoGasto 
                                AND sod.estado = 2  
                                AND sbc.codigo_centro_responsabilidad = @codigoCentroResp
		                        AND sbc.codigo_materia = @codigoMateria
                                AND sbc.ejercicio = @parametroEjercicio
                                AND vlb.ejercicio in (@ejercicioVista,0)
                                 AND NOT EXISTS (
                                         SELECT 1
                                            FROM VERSIONES_ANTEPROYECTOS_CONF_BIENES vab
                                            WHERE vab.NUMERO_BIEN = sobd.numero_bien
				                            AND vab.codigo_centro_responsabilidad = @codigoCentroResp
				                            AND vab.codigo_materia = @codigoMateria
                                            AND vab.CODIGO_VERSION = @codigoversion
                                            AND vab.SELECCIONADO = 1
                                    )
                            GROUP BY                            
                            sobd.numero_bien";


            var parametros = new DynamicParameters();
            parametros.Add("@codigoversion", request.CodigoVersion);
            parametros.Add("@codigoversionObjeto", request.CodigoVersionOBG);
            parametros.Add("@parametroObjetoGasto", request.CodigoObjetoGasto);
            parametros.Add("@parametroEjercicio", request.Ejercicio);
            var ejercicioVicta = request.Ejercicio - 1;
            parametros.Add("@ejercicioVista", ejercicioVicta);
            parametros.Add("@codigoCentroResp", request.CodigoCentroResponsabilidad);
            parametros.Add("@codigoMateria", request.CodigoMateria);


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<BienesparaConfiguracionPresupuestariaDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de Obtener Lista de Bienes para la Configuracion Presupuestaria");

                return resultado.ToList();
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al realizar la Configuracion Presupuestaria por Objeto de Gastos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<DatosparaConfiguracionPresupuestariaDTO> ObtenerDatosparaInsertarConfiguracionPresupuestaria(DatosparaConfiguracionPresupuestariaRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de Obtener Lista de Bienes para la Copnfiguracion Presupuestaria");

        try
        {
            string query = @"
                          select distinct
                            --Seccion Anteproyecto
                            --sbc.usuario_inserto as UsuarioInserto,
                            sbc.codigo_circunscripcion as CodigoCircunscripcion,
                            sbc.ejercicio as Ejercicio,
                            vcr.nombre_circunscripcion as DescripcionCircunscripcion,
                            --Seccion Configuracion Presupuestaria
                            vlb.codigo_objeto_gasto as CodigoObjetoGasto,
                            vlb.numero_objeto_gasto as NumeroObjetoGasto,
                            vlb.descripcion_objeto_gasto as DescripcionObjetoGasto,
                            vcr.codigo_programa as CodigoPrograma,
                            vcr.numero_programa as NumeroPrograma,
                            vcr.descripcion_programa as DescripcionPrograma,
                            vcr.codigo_actividad as CodigoActividad,
                            vcr.numero_actividad as NumeroActividad,
                            vcr.descripcion_actividad as DescripcionActividad,
                            vcr.codigo_tipo_presupuesto as CodigoTipoPresupuesto,
                            vcr.numero_tipo_presupuesto as NumeroTipoPresupuesto,
                            vcr.descripcion_tipo_presupuesto as DescripcionTipoPresupuesto,
                            vcr.codigo_departamento as CodigoDepartamento,
                            vcr.numero_departamento as NumeroDepartamento,
                            vcr.descripcion_departamento as DescripcionDepartamento,
                            vlb.numero_grupo as Grupo,
                            vlb.codigo_grupo as CodigoGrupo,      
                            vlb.descripcion_grupo as DescripcionGrupo,
                            vcr.codigo_centro_responsabilidad as CodigoCentroResponsabilidad, 
                            vcr.descripcion_centro_responsabilidad as DescripcionCentroResponsabilidad,
                            vcr.codigo_materia as CodigoMateria,
                            vcr.descripcion_materia as DescripcionMateria,
                            vlb.numero_subgrupo as SubGrupo,
                            vlb.codigo_subgrupo as CodigoSubGrupo,
                            vlb.descripcion_subgrupo as DescripcionSubGrupo      

                           from  
                            solicitudes_bienes_circunscripcion sbc 
                            join solicitudes_objetos_detalle sod  on sod.codigo_solicitud=sbc.codigo_solicitud 
                            join solicitudes_objetos_bienes_detalle sobd  on sobd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto 
                            and sobd.codigo_solicitud=sod.codigo_solicitud 
                            join vListaCentrosResponsabilidadPorCircunscripcion vcr  
                            on vcr.codigo_centro_responsabilidad=sbc.codigo_centro_responsabilidad and vcr.codigo_circunscripcion= sbc.codigo_circunscripcion 
                            and vcr.codigo_materia=sbc.codigo_materia 
                            join vListaBienesPrioritarios vlb  on vlb.codigo_catalogo=sobd.numero_bien  
                            and vlb.codigo_objeto_gasto=sod.codigo_objeto_gasto and vlb.codigo_circunscripcion = sbc.codigo_circunscripcion 

                           where  
                            sod.codigo_objeto_gasto= @parametroObjetodeGasto
                            and sbc.codigo_centro_responsabilidad= @parametroCentrodeResponsabilidad
                            and sbc.codigo_materia = @codigoMateria
                            and sbc.ejercicio= @parametroEjercicio 
                            and vlb.ejercicio in (@ejercicioVista,0)

                           group by 
                            vcr.descripcion_centro_responsabilidad,vlb.codigo_objeto_gasto, vlb.descripcion_objeto_gasto, vlb.numero_objeto_gasto, 
                            sbc.poi , vcr.numero_programa,vcr.descripcion_programa, vcr.numero_actividad , vcr.descripcion_actividad, 
                            vcr.numero_departamento , vlb.codigo_grupo, vlb.codigo_subgrupo,sbc.ejercicio,sbc.usuario_inserto,sbc.codigo_circunscripcion,
                            vcr.codigo_programa,vcr.codigo_actividad,vcr.codigo_tipo_presupuesto,vcr.codigo_departamento,
                            vcr.codigo_centro_responsabilidad,vcr.codigo_materia,vcr.nombre_circunscripcion,vcr.descripcion_tipo_presupuesto,
                            vcr.descripcion_departamento,vcr.descripcion_materia,vlb.numero_objeto_gasto,vcr.numero_programa,vcr.numero_actividad,
                            vcr.numero_tipo_presupuesto,vcr.numero_departamento,vlb.descripcion_grupo,vlb.descripcion_subgrupo,
                            vlb.numero_subgrupo,vlb.numero_grupo";


            string queryCantidadTotalRegistros = @"
                                SELECT COUNT(*) AS TotalRegistros 
                                from  
                                    solicitudes_bienes_circunscripcion sbc 
                                    join solicitudes_objetos_detalle sod  on sod.codigo_solicitud=sbc.codigo_solicitud 
                                    join solicitudes_objetos_bienes_detalle sobd  on sobd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto 
                                    and sobd.codigo_solicitud=sod.codigo_solicitud 
                                    join vListaCentrosResponsabilidadPorCircunscripcion vcr  
                                    on vcr.codigo_centro_responsabilidad=sbc.codigo_centro_responsabilidad and vcr.codigo_circunscripcion= sbc.codigo_circunscripcion 
                                    and vcr.codigo_materia=sbc.codigo_materia 
                                    join vListaBienesPrioritarios vlb  on vlb.codigo_catalogo=sobd.numero_bien  
                                    and vlb.codigo_objeto_gasto=sod.codigo_objeto_gasto and vlb.codigo_circunscripcion = sbc.codigo_circunscripcion 

                                where  
                                sod.codigo_objeto_gasto= @parametroObjetodeGasto
                                and sbc.codigo_centro_responsabilidad= @parametroCentrodeResponsabilidad
                                and sbc.codigo_materia = @codigoMateria
                                and sbc.ejercicio= @parametroEjercicio 
                                and vlb.ejercicio in (@ejercicioVista,0)
                                and sod.estado = 2

                                group by 
                                vcr.descripcion_centro_responsabilidad,vlb.codigo_objeto_gasto, vlb.descripcion_objeto_gasto, vlb.numero_objeto_gasto, 
                                sbc.poi , vcr.numero_programa,vcr.descripcion_programa, vcr.numero_actividad , vcr.descripcion_actividad, 
                                vcr.numero_departamento , vlb.codigo_grupo, vlb.codigo_subgrupo,sbc.ejercicio,sbc.usuario_inserto,sbc.codigo_circunscripcion,
                                vcr.codigo_programa,vcr.codigo_actividad,vcr.codigo_tipo_presupuesto,vcr.codigo_departamento,
                                vcr.codigo_centro_responsabilidad,vcr.codigo_materia";


            var parametros = new DynamicParameters();
            parametros.Add("@parametroObjetodeGasto", request.CodigoObjetoGasto);
            parametros.Add("@parametroCentrodeResponsabilidad", request.CentrodeResponsabilidad);
            var valorEjercicio = request.Ejercicio - 1;
            parametros.Add("@parametroEjercicio", request.Ejercicio);
            parametros.Add("@ejercicioVista", valorEjercicio);
            parametros.Add("@codigoMateria", request.CodigoMateria);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QuerySingleOrDefaultAsync<DatosparaConfiguracionPresupuestariaDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de Obtener Lista de Bienes para la Configuracion Presupuestaria");

                return resultado;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al realizar la Copnfiguracion Presupuestaria por Objeto de Gastos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<Datos<IEnumerable<OrganismoFinanciadorDTO>>> ObtenerDatosOrganismoFinanciador(OrganismoFinanciadorRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener datos de los organismos financiadores");

        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
        var query = string.Empty;

        try
        {
            query = @"
                            SELECT 
                                s.codigo_organismo_financiador as CodigoOrganismoFinanciador,
                                s.numero_organismo_financiador as NumeroOrganismoFinanciador,
                                s.descrip_organismo_financiador as DescripcionOrganismoFinanciador
                                FROM organismo_financiador s
                                WHERE activo = 1 AND codigo_organismo_financiador > 0";

            if (!string.IsNullOrEmpty(request.CodigoOrganismoFinanciador.ToString()) ||
                !string.IsNullOrEmpty(request.NumeroOrganismoFinanciador) ||
                !string.IsNullOrEmpty(request.DescripcionOrganismoFinanciador) ||
                !string.IsNullOrEmpty(request.TerminoDeBusqueda)
                )
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    query += @"
                                    AND (CONVERT(NVARCHAR(MAX), s.codigo_organismo_financiador) LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR s.numero_organismo_financiador LIKE '%' + @terminoDeBusqueda + '%'                                       
                                        OR s.descrip_organismo_financiador LIKE '%' + @terminoDeBusqueda + '%')";
                }
                else
                {
                    query += @"
                                    AND (@codigoOrganismoFinanciador IS NULL OR CONVERT(NVARCHAR(MAX), s.codigo_organismo_financiador) LIKE '%' + @codigoOrganismoFinanciador + '%')                       
                                    AND (@numeroOrganismoFinanciador IS NULL OR s.numero_organismo_financiador LIKE '%' + @numeroOrganismoFinanciador + '%')                                       
                                    AND (@descripcionOrganismoFinanciador IS NULL OR s.descrip_organismo_financiador LIKE '%' + @descripcionOrganismoFinanciador + '%')";
                }
            }

            query += @" ORDER BY s.codigo_organismo_financiador";
            query += @" OFFSET @saltarRegistros ROWS";
            query += @" FETCH NEXT @cantidadRegistros ROWS ONLY";


            string queryCantidadTotalRegistros = @"
                                SELECT COUNT(*) AS TotalRegistros 
                                    FROM organismo_financiador s
                                WHERE activo = 1 AND codigo_organismo_financiador > 0";

            if (!string.IsNullOrEmpty(request.CodigoOrganismoFinanciador.ToString()) ||
                !string.IsNullOrEmpty(request.NumeroOrganismoFinanciador) ||
                !string.IsNullOrEmpty(request.DescripcionOrganismoFinanciador) ||
                !string.IsNullOrEmpty(request.TerminoDeBusqueda)
                )
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    queryCantidadTotalRegistros += @"
                                      AND (CONVERT(NVARCHAR(MAX), s.codigo_organismo_financiador) LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR s.numero_organismo_financiador LIKE '%' + @terminoDeBusqueda + '%'                                       
                                        OR s.descrip_organismo_financiador LIKE '%' + @terminoDeBusqueda + '%')";
                }
                else
                {
                    queryCantidadTotalRegistros += @"
                                       AND (@codigoOrganismoFinanciador IS NULL OR CONVERT(NVARCHAR(MAX), s.codigo_organismo_financiador) LIKE '%' + @codigoOrganismoFinanciador + '%')                       
                                        AND (@numeroOrganismoFinanciador IS NULL OR s.numero_organismo_financiador LIKE '%' + @numeroOrganismoFinanciador + '%')                                       
                                        AND (@descripcionOrganismoFinanciador IS NULL OR s.descrip_organismo_financiador LIKE '%' + @descripcionOrganismoFinanciador + '%')";
                }
            }

            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);
            parametros.Add("@terminoDeBusqueda", request.TerminoDeBusqueda);

            // Añadir los parámetros correspondientes de búsqueda              
            parametros.Add("@codigoOrganismoFinanciador", request.CodigoOrganismoFinanciador);
            parametros.Add("@numeroOrganismoFinanciador", request.NumeroOrganismoFinanciador);
            parametros.Add("@descripcionOrganismoFinanciador", request.DescripcionOrganismoFinanciador);


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QueryAsync<OrganismoFinanciadorDTO>(query, parametros);

                var response = new Datos<IEnumerable<OrganismoFinanciadorDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalTegistros
                };

                _logger.LogInformation("Fin de Proceso de obtener datos de los organismos financiadores");
                return response;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener datos de la tabla organismo financiador" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<Datos<IEnumerable<FuenteFinanciamientoDTO>>> ObtenerDatosFuenteFinanciamiento(FuenteFinanciamientoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener datos de las Fuentes Financiacion");

        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
        var query = string.Empty;

        try
        {
            query = @"
                            select                                 
                                    s.codigo_fuente_financiamiento as CodigoFuenteFinanciamiento,
                                    s.numero_fuente_financiamiento as NumeroFuenteFinanciamiento,
                                    s.descrip_fuente_financiamiento as DescripcionFuenteFinanciamiento
                            from fuente_financiamiento s
                            where s.activo = 1 AND s.codigo_fuente_financiamiento > 0";

            if (!string.IsNullOrEmpty(request.CodigoFuenteFinanciamiento.ToString()) ||
                !string.IsNullOrEmpty(request.NumeroFuenteFinanciamiento) ||
                !string.IsNullOrEmpty(request.DescripcionFuenteFinanciamiento) ||
                !string.IsNullOrEmpty(request.TerminoDeBusqueda)
                )
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    query += @"
                                    AND (CONVERT(NVARCHAR(MAX), s.codigo_fuente_financiamiento) LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR s.numero_fuente_financiamiento LIKE '%' + @terminoDeBusqueda + '%'                                       
                                        OR s.descrip_fuente_financiamiento LIKE '%' + @terminoDeBusqueda + '%')";
                }
                else
                {
                    query += @"
                                    AND (@codigoFF IS NULL OR CONVERT(NVARCHAR(MAX), s.codigo_fuente_financiamiento) LIKE '%' + @codigoFF + '%')                       
                                    AND (@numeroFF IS NULL OR s.numero_fuente_financiamiento LIKE '%' + @numeroFF + '%')                                       
                                    AND (@descripcionFF IS NULL OR s.descrip_fuente_financiamiento LIKE '%' + @descripcionFF + '%')";
                }
            }

            query += @" ORDER BY s.codigo_fuente_financiamiento";
            query += @" OFFSET @saltarRegistros ROWS";
            query += @" FETCH NEXT @cantidadRegistros ROWS ONLY";


            string queryCantidadTotalRegistros = @"
                                SELECT COUNT(*) AS TotalRegistros 
                                    from fuente_financiamiento s
                            where s.activo = 1 AND s.codigo_fuente_financiamiento > 0 ";

            if (!string.IsNullOrEmpty(request.CodigoFuenteFinanciamiento.ToString()) ||
                !string.IsNullOrEmpty(request.NumeroFuenteFinanciamiento) ||
                !string.IsNullOrEmpty(request.DescripcionFuenteFinanciamiento) ||
                !string.IsNullOrEmpty(request.TerminoDeBusqueda)
                )
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    queryCantidadTotalRegistros += @"
                                       AND (CONVERT(NVARCHAR(MAX), s.codigo_fuente_financiamiento) LIKE '%' + @terminoDeBusqueda + '%'                       
                                        OR s.numero_fuente_financiamiento LIKE '%' + @terminoDeBusqueda + '%'                                       
                                        OR s.descrip_fuente_financiamiento LIKE '%' + @terminoDeBusqueda + '%')";
                }
                else
                {
                    queryCantidadTotalRegistros += @"
                                        AND (@codigoFF IS NULL OR CONVERT(NVARCHAR(MAX), s.codigo_fuente_financiamiento) LIKE '%' + @codigoFF + '%')                       
                                    AND (@numeroFF IS NULL OR s.numero_fuente_financiamiento LIKE '%' + @numeroFF + '%')                                       
                                    AND (@descripcionFF IS NULL OR s.descrip_fuente_financiamiento LIKE '%' + @descripcionFF + '%')";
                }
            }

            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);
            parametros.Add("@terminoDeBusqueda", request.TerminoDeBusqueda);

            // Añadir los parámetros correspondientes de búsqueda              
            parametros.Add("@codigoFF", request.CodigoFuenteFinanciamiento);
            parametros.Add("@numeroFF", request.NumeroFuenteFinanciamiento);
            parametros.Add("@descripcionFF", request.DescripcionFuenteFinanciamiento);


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QueryAsync<FuenteFinanciamientoDTO>(query, parametros);

                var response = new Datos<IEnumerable<FuenteFinanciamientoDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalTegistros
                };

                _logger.LogInformation("Fin de Proceso de obtener datos de las Fuentes Financiacion");
                return response;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener datos de la tabla fuente de financiacion" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<int> EliminarObjetodeAnteproyectoObjetoyBienes(VerionesAnteproyectoObjetosEliminarRequest request)
    {
        _logger.LogInformation("Inicio del Proceso de eliminar objeto de la version de anteproyecto de objetos");

        string queryEliminarBienes = @"DELETE from versiones_anteproyectos_bienes
                                                WHERE CODIGO_VERSION = @codigoVersion
                                                AND CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto";

        string queryEliminarIntermedia = @"DELETE from versiones_anteproyectos_conf_bienes
                                                WHERE CODIGO_VERSION = @codigoVersion
                                                AND CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto
                                                AND codigo_centro_responsabilidad = @codigoCentroResp
                                                AND codigo_materia = @codigoMateria";

        string queryEliminarObjetos = @"DELETE FROM VERSIONES_ANTEPROYECTOS_OBJETOS
                                              WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto
	                                          AND CODIGO_VERSION = @codigoVersion
	                                          AND CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria";

        var parametros = new DynamicParameters();
        parametros.Add("@codigoVersionAnteproyecto", request.Parametro_CodigoAnteProyectoObjeto);
        parametros.Add("@codigoVersion", request.Parametro_CodigoVersion);
        parametros.Add("@codigoConfigPresupuestaria", request.Parametro_Config_Presupuestaria);
        parametros.Add("@codigoCentroResp", request.CodigoCentroResponsabilidad);
        parametros.Add("@codigoMateria", request.CodigoMateria);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var eliminarBienes = await connection.ExecuteAsync(queryEliminarBienes, parametros);

                var eliminarBienesIntermedia = await connection.ExecuteAsync(queryEliminarIntermedia, parametros);

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

    public async Task<int> ModificarFinanciamientoAnteproyectoObjeto(ModificarFinanciamientoAnteproyectoObjetosRequest request)
    {
        _logger.LogInformation("Inicio del Proceso de modificar la version de anteproyecto de objetos");

        string query = @"UPDATE VERSIONES_ANTEPROYECTOS_OBJETOS
                               SET 
                                  CODIGO_FUENTE_FINANCIAMIENTO = @ff
                                  ,CODIGO_ORGANISMO_FINANCIADOR = @of                                   
                                  ,EVENTO = @evento
      
                             WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto
                                   AND CODIGO_VERSION = @codigoVersion
                                   AND CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria";

        var parametros = new DynamicParameters();
        parametros.Add("@ff", request.CodigoFuenteFinanciamiento);
        parametros.Add("@of", request.CodigoOrganismoFinanciador);
        parametros.Add("@evento", request.Evento);

        parametros.Add("@codigoVersionAnteproyecto", request.CodigoAnteProyectoObjeto);
        parametros.Add("@codigoVersion", request.CodigoVersion);
        parametros.Add("@codigoConfigPresupuestaria", request.ConfiguracionPresupuestaria);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de modificar la version de anteproyecto de objetos");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new RegistrosParaSolicitudException("Ocurrió un error al modificar los datos en la tabla Versiones Anteproyecto Objetos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<int> ActualizarCantidadBiendelConsolidadoBienes(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion)
    {
        _logger.LogInformation("Inicio de Proceso de actualizar la cantidad de Bienes de la tabla de anteproyecto bienes consolidado");        

        var cantidadBien = @"SELECT CANTIDAD 
                            FROM VERSIONES_ANTEPROYECTOS_BIENES 
                            WHERE CODIGO_VERSION = @codigoVersion                             
                            AND NUMERO_BIEN = @numeroBien";

        var actualizarCantidadBien = @"UPDATE VERSIONES_ANTEPROYECTOS_BIENES
                            SET CANTIDAD = @cantidad
                            WHERE CODIGO_VERSION = @version                             
                            AND NUMERO_BIEN = @nroBien";

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {              
               
                // Obtenemos la Cantidad del Bien
                int cantidad = await connection.ExecuteScalarAsync<int>(cantidadBien, new
                {
                    codigoAnteproyectoObjeto = CodigoAnteproyectoObj,
                    codigoVersion = CodigoVersion,
                    numeroBien = request.NumeroBien
                });

                var resultado = 0;
                var monto = (cantidad > 0) ? cantidad - request.Cantidad : cantidad;                
                    
                resultado = await connection.ExecuteAsync(actualizarCantidadBien, new
                {
                    cantidad = monto,
                    version = CodigoVersion,
                    nroBien = request.NumeroBien
                });                          
                            
                _logger.LogInformation("Fin de Proceso de actualizar la cantidad de Bienes de la tabla de anteproyecto bienes consolidado");
                return resultado;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al actualizar la cantidad de Bienes de la tabla de anteproyecto bienes consolidado" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<int> EliminarBienesporFuentedeFinanciamiento(int cabVersion, int cabObjeto, int cabBienes, int codigoMateria, int codigoCentroResponsabilidad)
    {
        _logger.LogInformation("Inicio del Proceso de modificar la version de anteproyecto de bienes");

        string query = @"DELETE from VERSIONES_ANTEPROYECTOS_CONF_BIENES
                                WHERE CODIGO_VERSION = @codigoVersion
                                AND CODIGO_ANTEPROYECTO_OBJETO = @codigoVerObjeto
                                AND CODIGO_ANTEPROYECTO_BIEN = @codigoVerBienes
                                AND codigo_materia = @codigoMateria
								AND codigo_centro_responsabilidad = @codigoCentroResp";

        var parametros = new DynamicParameters();
        parametros.Add("@codigoVerBienes", cabBienes);
        parametros.Add("@codigoVerObjeto", cabObjeto);
        parametros.Add("@codigoVersion", cabVersion);
        parametros.Add("@codigoMateria", codigoMateria);
        parametros.Add("@codigoCentroResp", codigoCentroResponsabilidad);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de modificar la version de anteproyecto de bienes");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new RegistrosParaSolicitudException("Ocurrió un error al modificar los datos en la tabla Versiones Anteproyecto Bienes" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<int> EliminarBienesporFuentedeFinanciamientoModif(int cabVersion, int cabObjeto, int cabBienes, int codigoMateria, int codigoCentroResponsabilidad)
    {
        _logger.LogInformation("Inicio del Proceso de modificar la version de anteproyecto de bienes");

        string query = @"DELETE from VERSIONES_ANTEPROYECTOS_BIENES
                                WHERE CODIGO_VERSION = @codigoVersion
                                AND CODIGO_ANTEPROYECTO_OBJETO = @codigoVerObjeto
                                AND CODIGO_ANTEPROYECTO_BIEN = @codigoVerBienes";

        var parametros = new DynamicParameters();
        parametros.Add("@codigoVerBienes", cabBienes);
        parametros.Add("@codigoVerObjeto", cabObjeto);
        parametros.Add("@codigoVersion", cabVersion);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de modificar la version de anteproyecto de bienes");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new RegistrosParaSolicitudException("Ocurrió un error al modificar los datos en la tabla Versiones Anteproyecto Bienes" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<Datos<IEnumerable<BienesparaConfiguracionPresupuestariaDTO>>> ObtenerBienesparaFuenteFinanciamiento(BienesparaConfiguracionPresupuestariaRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de Obtener Lista de Bienes para la Configuracion Presupuestaria");

        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;

        try
        {
            
                string filtrosComunes = "";
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    filtrosComunes = @"
                    AND (
                        CAST(NumeroBien AS NVARCHAR(200)) LIKE '%' + @terminobusqueda + '%' 
                        OR Descripcion LIKE '%' + @terminobusqueda + '%'
                        OR CAST(ISNULL(Cantidad, 0) AS NVARCHAR(200)) LIKE '%' + REPLACE(@terminobusqueda, '.', '') + '%'
                        OR CAST(ISNULL(ValorUnitario, 0) AS NVARCHAR(200)) LIKE '%' + REPLACE(@terminobusqueda, '.', '') + '%'
                        OR CAST(ISNULL(Cantidad * ValorUnitario, 0) AS NVARCHAR(200)) LIKE '%' + REPLACE(@terminobusqueda, '.', '')  + '%'
                    )";
                }
                else
                {
                    filtrosComunes = @"
                    AND (@numeroBien IS NULL OR CAST(NumeroBien AS NVARCHAR(200)) LIKE '%' + @numeroBien + '%')                       
                    AND (@descripcionBien IS NULL OR Descripcion LIKE '%' + @descripcionBien + '%') 
                    AND (@cantidad IS NULL OR CAST(ISNULL(Cantidad, 0) AS NVARCHAR(200)) LIKE '%' + REPLACE(CAST(@cantidad AS NVARCHAR(200)), '.', '')  + '%') 
                    AND (@valorUnitario IS NULL OR CAST(ISNULL(ValorUnitario, 0) AS NVARCHAR(200)) LIKE '%' + REPLACE( CAST(@valorUnitario AS NVARCHAR(200)), '.', '') + '%') 
                    AND (@total IS NULL OR CAST(ISNULL(Total, 0) AS NVARCHAR(200)) LIKE '%' + REPLACE(CAST(@total AS NVARCHAR(200)), '.', '')  + '%')";
                }



            string query = $@"
                    SELECT *
                    FROM (
                        SELECT
                            NULL as CodigoAnteproyectoBien,
                            sobd.numero_bien AS NumeroBien,
                            MAX(sobd.descripcion) AS Descripcion,  
                            ISNULL(SUM(CONVERT(BIGINT, sobd.cantidad)), 0) AS Cantidad,
                            ISNULL(MAX(CONVERT(DECIMAL(18), sobd.costo_unitario)), 0) AS ValorUnitario,
                            ISNULL(SUM(CONVERT(BIGINT, sobd.cantidad) * CONVERT(DECIMAL(18), sobd.costo_unitario)), 0) AS Total,
                            MAX(sobd.fundamentacion) AS Fundamentacion, 
                            MAX(vlb.codigo_unidad_medida) AS UnidadMedida,  
                            'false' AS Seleccionado
                        FROM
                            solicitudes_objetos_bienes_detalle sobd
                            JOIN solicitudes_objetos_detalle sod ON sobd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto
                                                                AND sobd.codigo_solicitud = sod.codigo_solicitud
                            JOIN solicitudes_bienes_circunscripcion sbc ON sobd.codigo_solicitud = sbc.codigo_solicitud
                            JOIN vListaBienesPrioritarios vlb ON vlb.codigo_catalogo = sobd.numero_bien
                                                             AND vlb.codigo_objeto_gasto = sod.codigo_objeto_gasto
                                                             AND vlb.codigo_circunscripcion = sbc.codigo_circunscripcion
                        WHERE
                            sod.codigo_objeto_gasto = @parametroObjetoGasto 
                            AND sod.estado = 2               
                            AND sbc.codigo_materia = @codigoMateria
                            AND sbc.codigo_centro_responsabilidad = @codigoCentroResp
                            AND sbc.ejercicio = @parametroEjercicio                            
                            AND vlb.ejercicio in (@ejercicioVista,0)
                            AND NOT EXISTS (
                                SELECT 1
                                FROM versiones_anteproyectos_conf_bienes vab
                                WHERE vab.NUMERO_BIEN = sobd.numero_bien
                                AND vab.codigo_materia = @codigoMateria
			                    AND VAB.codigo_centro_responsabilidad = @codigoCentroResp
                                AND vab.CODIGO_VERSION = @codigoversion
                                AND vab.SELECCIONADO = 1)
                           
                           GROUP BY 
                             sobd.numero_bien

                        UNION

                        SELECT 
                            vab.CODIGO_ANTEPROYECTO_BIEN as CodigoAnteproyectoBien,
                            vab.NUMERO_BIEN AS NumeroBien,
                            MAX(vab.DESCRIPCION_BIEN) AS Descripcion,
		                    ISNULL(SUM(CONVERT(BIGINT, vab.CANTIDAD)), 0) AS Cantidad,
		                    ISNULL(MAX(CONVERT(DECIMAL(18), vab.VALOR_UNITARIO)), 0) AS ValorUnitario,
		                    ISNULL(SUM(CONVERT(BIGINT, vab.CANTIDAD) * CONVERT(DECIMAL(18), vab.VALOR_UNITARIO)), 0) AS Total,
                            MAX(vab.FUNDAMENTACION) AS Fundamentacion,
                            MAX(vab.UNIDAD_MEDIDA) AS UnidadMedida,
                            'true' AS Seleccionado   
                        FROM 
                            versiones_anteproyectos_conf_bienes vab
                        WHERE 
                            vab.CODIGO_VERSION = @codigoversion
                            AND vab.CODIGO_ANTEPROYECTO_OBJETO = @codigoversionObjeto
                            AND vab.codigo_centro_responsabilidad = @codigoCentroResp
		                    AND vab.codigo_materia = @codigoMateria
                          
                        GROUP BY
		                 vab.CODIGO_ANTEPROYECTO_BIEN,
                         vab.NUMERO_BIEN

                    ) AS resultado
                    WHERE 1 = 1 
                    {string.Format(filtrosComunes)}
                    ORDER BY NumeroBien
                    OFFSET @saltarRegistros ROWS FETCH NEXT @cantidadRegistros ROWS ONLY";


            var queryCantidadTotalRegistros = $@"
                                SELECT Count(*) as TotalRegistros
                                FROM (
                                    SELECT
                                        NULL as CodigoAnteproyectoBien,
                                        sobd.numero_bien AS NumeroBien,
                                        MAX(sobd.descripcion) AS Descripcion,  
                                        ISNULL(SUM(CONVERT(BIGINT, sobd.cantidad)), 0) AS Cantidad,
                                        ISNULL(MAX(CONVERT(DECIMAL(18), sobd.costo_unitario)), 0) AS ValorUnitario,
                                        ISNULL(SUM(CONVERT(BIGINT, sobd.cantidad) * CONVERT(DECIMAL(18), sobd.costo_unitario)), 0) AS Total,
                                        MAX(sobd.fundamentacion) AS Fundamentacion, 
                                        MAX(vlb.codigo_unidad_medida) AS UnidadMedida,  
                                        'false' AS Seleccionado
                                    FROM
                                        solicitudes_objetos_bienes_detalle sobd
                                        JOIN solicitudes_objetos_detalle sod ON sobd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto
                                                                            AND sobd.codigo_solicitud = sod.codigo_solicitud
                                        JOIN solicitudes_bienes_circunscripcion sbc ON sobd.codigo_solicitud = sbc.codigo_solicitud
                                        JOIN vListaBienesPrioritarios vlb ON vlb.codigo_catalogo = sobd.numero_bien
                                                                         AND vlb.codigo_objeto_gasto = sod.codigo_objeto_gasto
                                                                         AND vlb.codigo_circunscripcion = sbc.codigo_circunscripcion
                                    WHERE
                                        sod.codigo_objeto_gasto = @parametroObjetoGasto 
                                        AND sod.estado = 2   
                                        AND sbc.codigo_centro_responsabilidad = @codigoCentroResp
		                                AND sbc.codigo_materia = @codigoMateria
                                        AND sbc.ejercicio = @parametroEjercicio
                                        AND vlb.ejercicio in (@ejercicioVista,0)
                                        AND NOT EXISTS (
                                            SELECT 1
                                            FROM versiones_anteproyectos_conf_bienes vab
                                            WHERE vab.NUMERO_BIEN = sobd.numero_bien
                                            AND vab.codigo_materia = @codigoMateria
			                                AND vab.codigo_centro_responsabilidad = @codigoCentroResp
                                            AND vab.CODIGO_VERSION = @codigoversion
                                            AND vab.SELECCIONADO = 1)                                        
                                    GROUP BY 
                                         sobd.numero_bien

                                    UNION

                                    SELECT 
                                        vab.CODIGO_ANTEPROYECTO_BIEN as CodigoAnteproyectoBien,
                                        vab.NUMERO_BIEN AS NumeroBien,
                                        MAX(vab.DESCRIPCION_BIEN) AS Descripcion,
		                                ISNULL(SUM(CONVERT(BIGINT, vab.CANTIDAD)), 0) AS Cantidad,
		                                ISNULL(MAX(CONVERT(DECIMAL(18), vab.VALOR_UNITARIO)), 0) AS ValorUnitario,
		                                ISNULL(SUM(CONVERT(BIGINT, vab.CANTIDAD) * CONVERT(DECIMAL(18), vab.VALOR_UNITARIO)), 0) AS Total,
                                        MAX(vab.FUNDAMENTACION) AS Fundamentacion,
                                        MAX(vab.UNIDAD_MEDIDA) AS UnidadMedida,
                                        'true' AS Seleccionado   
                                     FROM 
                                        versiones_anteproyectos_conf_bienes vab
                                     WHERE 
                                        vab.CODIGO_VERSION = @codigoversion
                                        AND vab.CODIGO_ANTEPROYECTO_OBJETO = @codigoversionObjeto
                                        AND vab.codigo_centro_responsabilidad = @codigoCentroResp
		                                AND vab.codigo_materia = @codigoMateria                                        
                                     GROUP BY 
		                                vab.CODIGO_ANTEPROYECTO_BIEN,
                                        vab.NUMERO_BIEN
                                ) AS resultado
                                WHERE 1 = 1 
                                {string.Format(filtrosComunes)}";



            var parametros = new DynamicParameters();
            parametros.Add("@codigoversion", request.CodigoVersion);
            parametros.Add("@codigoversionObjeto", request.CodigoVersionOBG);

            parametros.Add("@numeroBien", request.NumeroBien.ToString());
            parametros.Add("@descripcionBien", request.DescripcionBien);
            parametros.Add("@cantidad", request.Cantidad);
            parametros.Add("@valorUnitario", request.CostoUnitario);
            parametros.Add("@total", request.MontoTotal);

            parametros.Add("@parametroObjetoGasto", request.CodigoObjetoGasto);
            parametros.Add("@parametroEjercicio", request.Ejercicio);
            var ejercicioVista = request.Ejercicio - 1;
            parametros.Add("@ejercicioVista", ejercicioVista);
            parametros.Add("@terminobusqueda", request.TerminoDeBusqueda);
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);
            parametros.Add("@codigoCentroResp", request.CodigoCentroResponsabilidad);
            parametros.Add("@codigoMateria", request.CodigoMateria);


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QueryAsync<BienesparaConfiguracionPresupuestariaDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de Obtener Lista de Bienes para la Configuracion Presupuestaria");

                var response = new Datos<IEnumerable<BienesparaConfiguracionPresupuestariaDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalTegistros
                };

                return response;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener los bienes para la fuente de financiamiento" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<IEnumerable<FinanciamientoVersionAnteproyectoObjetosResponseDTO>> ObtenerDatosVersionesAnteproyectosObjetos(FinanciamientoVersionAnteproyectoObjetosRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener datos de la tabla Anteproyecto Objetos");

        try
        {
            string query = @"
                          SELECT 
                            vr.CODIGO_ANTEPROYECTO_OBJETO as CodigoAnteproyectoObjeto,
	                        vr.CODIGO_FUENTE_FINANCIAMIENTO as CodigoFuenteFinanciacion,
                            ff.numero_fuente_financiamiento as NumeroFF,
	                        FF.descrip_fuente_financiamiento as DescripcionFuenteFinanciacion,	
	                        vr.CODIGO_ORGANISMO_FINANCIADOR as CodigoOrganismoFinanciador,
                            orgf.numero_organismo_financiador as NumeroORGF,
	                        ORGF.descrip_organismo_financiador as DescripcionOrganismoFinanciador,	
	                        COALESCE(SUM(CONVERT(BIGINT, bn.CANTIDAD)), 0) AS Cantidad,
                            COALESCE(SUM(CONVERT(DECIMAL(18,2), bn.VALOR_UNITARIO) * CONVERT(BIGINT, bn.CANTIDAD)), 0) AS Monto,
                            vr.EVENTO as Evento

                        FROM VERSIONES_ANTEPROYECTOS_OBJETOS VR JOIN
                        organismo_financiador ORGF on vr.CODIGO_ORGANISMO_FINANCIADOR = ORGF.codigo_organismo_financiador
                        join fuente_financiamiento FF ON VR.CODIGO_FUENTE_FINANCIAMIENTO = FF.codigo_fuente_financiamiento
                        LEFT JOIN VERSIONES_ANTEPROYECTOS_CONF_BIENES BN ON vr.CODIGO_VERSION = bn.CODIGO_VERSION
                        AND vr.CODIGO_ANTEPROYECTO_OBJETO = bn.CODIGO_ANTEPROYECTO_OBJETO

                        WHERE VR.CODIGO_VERSION = @codigoVersion
                        AND vr.CODIGO_CONFIGURACION_PRESUPUESTARIA = @numeroConfPresupuestaria

                        group by vr.CODIGO_FUENTE_FINANCIAMIENTO,FF.descrip_fuente_financiamiento,
                        ORGF.descrip_organismo_financiador,vr.CODIGO_ORGANISMO_FINANCIADOR,
                        vr.CODIGO_ANTEPROYECTO_OBJETO,vr.EVENTO,orgf.numero_organismo_financiador,
                        ff.numero_fuente_financiamiento";

            // Definición de parámetros
            var parametros = new DynamicParameters();

            parametros.Add("@codigoVersion", request.CodigoVersionAnteproyectos);
            parametros.Add("@numeroConfPresupuestaria", request.CodigoConfiguracionPresupuestaria);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<FinanciamientoVersionAnteproyectoObjetosResponseDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de obtener datos de la tabla Anteproyecto Objetos");
                return resultado;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener datos de la tabla Anteproyecto Objetos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<IEnumerable<DatosdeSolicitudporObjetoGastoResponseDTO>> ObtenerDatosdelaSolicitud(DatosdeSolicitudporObjetoGastoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener datos de la Solicitud por Objeto de Gasto");

        try
        {           
            string query = @"
                         select 
                            numero_objeto_gasto+'-'+ vlb.descripcion_objeto_gasto as ObjetoGasto, 
                            sbc.codigo_solicitud as CodigoSolicitud,
                            sbc.numero_solicitud as NroSolicitud,
                            sbc.fecha_solicitud as FechaEmision, 
	                        sbc.codigo_circunscripcion as CodigoCircunscripcion,
                            vcr.nombre_circunscripcion as DescripcionCircunscripcion,
                            (select u.username 
                            from usuarios_poder_judicial u
                            where u.codigo_usuario = sbc.usuario_inserto) as UsuarioSolicitante,
                            sbc.ejercicio as Año, 
                            vcr.descripcion_centro_responsabilidad as CentroResponsabilidad,
	                        vcr.codigo_materia as CodigoMateria,
                            vcr.descripcion_materia as MateriaJuridica, 
                            ( select sum(sobd.costo_unitario* sobd.cantidad)  
                            from solicitudes_objetos_bienes_detalle sobd  
                            where sobd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto 
                            and sobd.codigo_solicitud=sod.codigo_solicitud) as Montototal, 
                            ( select sum(sobd.cantidad)  from solicitudes_objetos_bienes_detalle sobd 
                            where sobd.codigo_solicitud_objeto = sod.codigo_solicitud_objeto 
                            and sobd.codigo_solicitud=sod.codigo_solicitud) as CantidadTotal   

                        from 	solicitudes_bienes_circunscripcion sbc 
                        join solicitudes_objetos_detalle sod  
                        on sod.codigo_solicitud=sbc.codigo_solicitud 
                        join vListaCentrosResponsabilidadPorCircunscripcion vcr 
                        on vcr.codigo_centro_responsabilidad=sbc.codigo_centro_responsabilidad 
                        and vcr.codigo_circunscripcion= sbc.codigo_circunscripcion 
                        and vcr.codigo_materia=sbc.codigo_materia 
                        join vListaBienesPrioritarios vlb  
                        on vlb.codigo_objeto_gasto=sod.codigo_objeto_gasto 
                        and vlb.codigo_circunscripcion = sbc.codigo_circunscripcion 

                        where 	sod.codigo_objeto_gasto = @codigoObjetoGasto
                        and sbc.ejercicio= @ejercicio 
                        and vlb.ejercicio in (@ejercicioVista,0) 
                        and sod.estado = 2
                        and sbc.codigo_materia = @codigoMateria
                        and sbc.codigo_centro_responsabilidad = @codigoCentroRespon

                        group by 
                            numero_objeto_gasto, vlb.descripcion_objeto_gasto, sod.codigo_solicitud_objeto, sbc.fecha_solicitud , 
                            sbc.ejercicio, vcr.descripcion_centro_responsabilidad,vcr.descripcion_materia,sod.codigo_solicitud,
                            sbc.usuario_inserto,sbc.codigo_circunscripcion,vcr.nombre_circunscripcion,vcr.codigo_materia,
                            sbc.codigo_solicitud,sbc.numero_solicitud";

            // Definición de parámetros
            var parametros = new DynamicParameters();

            parametros.Add("@codigoObjetoGasto", request.CodigoObjetoGasto);
            parametros.Add("@ejercicio", request.Ejercicio);
            var ejercicioVista = request.Ejercicio -1; 
            parametros.Add("@ejercicioVista", ejercicioVista);
            parametros.Add("@codigoCentroRespon", request.CodigoCentroResponsabilidad);
            parametros.Add("@codigoMateria", request.CodigoMateria);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<DatosdeSolicitudporObjetoGastoResponseDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de obtener datos de la Solicitud por Objeto de Gasto");
                return resultado;
            }
        }

        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al obtener datos de la tabla Anteproyecto Objetos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<int> ModificarMontoPlanificadoAnteproyectoObjeto(int versionAnteproyecto, int configuracionPresupuestaria, int versionAnteproyectoObjeto,
        int codigoMateria, int codigoCentroResp)
    {
        _logger.LogInformation("Inicio del Proceso de modificar la version de anteproyecto de objetos");

        string queryMonto = @" SELECT             
                                    COALESCE(SUM(CONVERT(DECIMAL(18), bn.VALOR_UNITARIO) * CONVERT(BIGINT, bn.CANTIDAD)), 0) AS Monto

			                        FROM VERSIONES_ANTEPROYECTOS_CONF_BIENES BN       
		                           WHERE bn.CODIGO_VERSION = @codigoVersion       
			                             AND bn.CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto
                                         AND bn.codigo_centro_responsabilidad = @codigoCentroResponsabilidad
                                         AND bn.codigo_materia = @codigoMateria";

        string queryUpdate = @" UPDATE VERSIONES_ANTEPROYECTOS_OBJETOS
                                      set MONTO_PLANIFICADO = @monto

                                    WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto
                                    AND CODIGO_VERSION = @codigoVersion
                                    AND CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria";       

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var parametros = new DynamicParameters();

                parametros.Add("@codigoVersionAnteproyecto", versionAnteproyectoObjeto);
                parametros.Add("@codigoVersion", versionAnteproyecto);
                parametros.Add("@codigoConfigPresupuestaria", configuracionPresupuestaria);
                parametros.Add("@codigoCentroResponsabilidad", codigoCentroResp);
                parametros.Add("@codigoMateria", codigoMateria);

                var monto = await connection.QueryFirstOrDefaultAsync<decimal>(queryMonto, parametros);
                parametros.Add("@monto", monto);

                var resultado = await connection.ExecuteAsync(queryUpdate, parametros);

                _logger.LogInformation("Fin del Proceso de modificar el monto de la fuente de financiamiento de la version de anteproyecto de objetos");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new RegistrosParaSolicitudException("Ocurrió un error al modificar el monto de la Fuente de Financiamiento en la tabla Versiones Anteproyecto Objetos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<int> ModificarMontoPlanificadoAnteproyectoObjetoModif(int versionAnteproyecto, int configuracionPresupuestaria, int versionAnteproyectoObjeto,
    int codigoMateria, int codigoCentroResp)
    {
        _logger.LogInformation("Inicio del Proceso de modificar la version de anteproyecto de objetos");

        string queryMonto = @" SELECT             
                                    COALESCE(SUM(CONVERT(DECIMAL(18), bn.VALOR_UNITARIO) * CONVERT(BIGINT, bn.CANTIDAD)), 0) AS Monto

			                        FROM VERSIONES_ANTEPROYECTOS_BIENES BN       
		                           WHERE bn.CODIGO_VERSION = @codigoVersion       
			                             AND bn.CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto";

        string queryUpdate = @" UPDATE VERSIONES_ANTEPROYECTOS_OBJETOS
                                      set MONTO_PLANIFICADO = @monto

                                    WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto
                                    AND CODIGO_VERSION = @codigoVersion
                                    AND CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria";

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var parametros = new DynamicParameters();

                parametros.Add("@codigoVersionAnteproyecto", versionAnteproyectoObjeto);
                parametros.Add("@codigoVersion", versionAnteproyecto);
                parametros.Add("@codigoConfigPresupuestaria", configuracionPresupuestaria);           

                var monto = await connection.QueryFirstOrDefaultAsync<decimal>(queryMonto, parametros);
                parametros.Add("@monto", monto);

                var resultado = await connection.ExecuteAsync(queryUpdate, parametros);

                _logger.LogInformation("Fin del Proceso de modificar el monto de la fuente de financiamiento de la version de anteproyecto de objetos");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new RegistrosParaSolicitudException("Ocurrió un error al modificar el monto de la Fuente de Financiamiento en la tabla Versiones Anteproyecto Objetos" + "||-->" + ex.Message + "<--||");
        }
    }

    public async Task<int> VerificarExisteVersiondeAnteproyectoCerrado(int ejercicio, int codigoCircunscripcion)
    {
        _logger.LogInformation("Inicio del Proceso de verificar el estado del anteproyecto.");

        string query = $@"
        SELECT COUNT(codigo_version) FROM VERSIONES_ANTEPROYECTOS
        WHERE ejercicio = @ejercicio 
        AND codigo_circunscripcion = @codigoCircunscripcion
        AND estado = 2 ";

        // Obtiene el codigo de circunscripcion Jurisdiccional
        string queryCircunscripcionReal = @"SELECT 
	                        cir.codigo_circunscripcion as CodigoCircunscripcion
                        FROM circunscripciones cir
                        WHERE cir.codigo_circunscripcion_jurisdiccional = @codigoCircunscripcion";

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var parametros = new DynamicParameters();
                parametros.Add("@codigoCircunscripcion", codigoCircunscripcion);

                codigoCircunscripcion = await connection.ExecuteScalarAsync<int>(queryCircunscripcionReal, parametros);

                parametros = new DynamicParameters();
                parametros.Add("@ejercicio", ejercicio);
                parametros.Add("@codigoCircunscripcion", codigoCircunscripcion);

                var resultado = await connection.ExecuteScalarAsync<int>(query, parametros);

                if (resultado != 0)
                {
                    resultado = 0;
                }
                else
                {
                    resultado = 1;
                }

                _logger.LogInformation("Fin del Proceso de verificar el estado del anteproyecto.");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new RegistrosParaSolicitudException("Ocurrió un error al verificar el estado del anteproyecto." + "||-->" + ex.Message + "<--||");
        }
    }

}
