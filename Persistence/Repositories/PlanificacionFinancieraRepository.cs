using Application.Services.Interfaces.IRepository;
using Dapper;
using DocumentFormat.OpenXml.Drawing;
using Domain.Entities.PlanificacionFinanciera;
using Domain.Entities.Request;
using Domain.Exceptions;
using Domain.Exceptions.ImportarArchivoSIPOIExcepcions;
using Microsoft.Extensions.Logging;

namespace Persistence.Repositories;

public class PlanificacionFinancieraRepository : IPlanificacionFinancieraRepository
{

    private readonly DbConnections _conexion;
    private readonly ILogger<PlanificacionFinancieraRepository> _logger;

    public PlanificacionFinancieraRepository(DbConnections conexion, ILogger<PlanificacionFinancieraRepository> logger)
    {
        _conexion = conexion;
        _logger = logger;
    }
    
    public async Task<Datos<IEnumerable<VersionesAnteproyectoDTO>>> ListadoVersionesanteproyectos(VersionesAnteproyectoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el listado de versiones de planificacion financiera");

        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
        string filtrosAdicionales = string.Empty;
        string terminoDeBusquedaSQL =string.Empty;
        //Tratamos de identificar si el dato que viene es una fecha
        DateTime fechaBusqueda;
        if(!string.IsNullOrEmpty(request.TerminoDeBusqueda))
            {
             terminoDeBusquedaSQL = request.TerminoDeBusqueda.Replace(" ", "");
        }
        bool esFechaBusqueda = DateTime.TryParseExact(terminoDeBusquedaSQL, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out fechaBusqueda);
      
            terminoDeBusquedaSQL = esFechaBusqueda ? fechaBusqueda.ToString("yyyy-MM-dd") : terminoDeBusquedaSQL;

            
        

        try
        {
            if (!string.IsNullOrEmpty(request.NumeroVersion?.ToString()) ||
                !string.IsNullOrEmpty(request.Ejercicio?.ToString()) ||
                !string.IsNullOrEmpty(request.DescripcionEstado) ||
                !string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                request.Fecha.HasValue)
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    filtrosAdicionales += @"
                AND (
                    CONVERT(NVARCHAR, va.NUMERO_VERSION) LIKE '%' + @TerminoDeBusqueda + '%'                       
                    OR CONVERT(NVARCHAR, va.EJERCICIO) LIKE '%' + @TerminoDeBusqueda + '%'
                    OR r.descripcion_referencia LIKE '%' + @TerminoDeBusqueda + '%'
                    OR CONVERT(NVARCHAR(MAX), va.FECHA_MODIFICACION, 126) LIKE '%' + @terminoDeBusquedaSQL + '%' 
                )";
                }
                else
                {
                    filtrosAdicionales += @"
                AND (@NumeroVersion IS NULL OR CONVERT(NVARCHAR, va.NUMERO_VERSION) LIKE '%' + @NumeroVersion + '%')                       
                AND (@Ejercicio IS NULL OR CONVERT(NVARCHAR, va.EJERCICIO) LIKE '%' + @Ejercicio + '%')                                       
                AND (@DescripcionEstado IS NULL OR r.descripcion_referencia LIKE '%' + @DescripcionEstado + '%')";


                    if (request.Fecha.HasValue)
                    {
                        filtrosAdicionales += @" 
                        AND CONVERT(DATE, ISNULL(va.FECHA_MODIFICACION, va.FECHA_INSERTO)) = @Fecha";
                    }
                }
            }

            var query = $@"
                            SELECT
                                va.CODIGO_VERSION as CodigoVersion,
                                va.NUMERO_VERSION as NumeroVersion, 
                                va.EJERCICIO as Ejercicio, 
                                va.ESTADO as CodigoEstado,
                                r.descripcion_referencia as DescripcionEstado, 
                                ISNULL(va.FECHA_MODIFICACION, va.FECHA_INSERTO) as Fecha 
                            FROM  
                                VERSIONES_ANTEPROYECTOS va
                            JOIN 
                                referencias r ON va.ESTADO = r.codigo_referencia
                            JOIN 
                                tipo_referencias tr ON r.codigo_tipo_referencia = tr.codigo_tipo_referencia
                            WHERE                                  
                                tr.dominio_tipo_referencia = 'ESTADO_VERSION'
                                AND TR.estado = 1                               
                                AND va.version_consolidado=0
                                {filtrosAdicionales}
                            ORDER BY 
                                va.EJERCICIO DESC
                            OFFSET @saltarRegistros ROWS
                            FETCH NEXT @cantidadRegistros ROWS ONLY";

            var queryCantidadTotalRegistros = $@"
                            SELECT COUNT(*) AS TotalRegistros 
                            FROM VERSIONES_ANTEPROYECTOS va
                            JOIN referencias r ON va.ESTADO = r.codigo_referencia
                            JOIN tipo_referencias tr ON r.codigo_tipo_referencia = tr.codigo_tipo_referencia
                            WHERE  
                                tr.dominio_tipo_referencia = 'ESTADO_VERSION'
                                AND TR.estado = 1
                                AND va.numero_version = (
                                    SELECT MIN(numero_version)  
                                    FROM VERSIONES_ANTEPROYECTOS 
                                    WHERE EJERCICIO = va.EJERCICIO) 
                                AND va.version_consolidado=0
                                {filtrosAdicionales}";

            // Definición de parámetros
            var convertDate = request.Fecha?.ToString("yyyy-MM-dd");
            var parametros = new DynamicParameters();
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);
            parametros.Add("@NumeroVersion", request.NumeroVersion?.ToString());
            parametros.Add("@Ejercicio", request.Ejercicio?.ToString());
            parametros.Add("@DescripcionEstado", request.DescripcionEstado);
            parametros.Add("@TerminoDeBusqueda", request.TerminoDeBusqueda);
            parametros.Add("@Fecha", convertDate);
            parametros.Add("@terminoDeBusquedaSQL", terminoDeBusquedaSQL);
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QueryAsync<VersionesAnteproyectoDTO>(query, parametros);

                var response = new Datos<IEnumerable<VersionesAnteproyectoDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalRegistros
                };

                _logger.LogInformation("Fin de Proceso de obtener el listado de versiones de planificacion financiera");
                return response;
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener el listado de la planificacion financiera" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> ObtenerDiasRestantesCierre()
    {
        _logger.LogInformation("Inicio de Proceso de obtener los días restantes hasta la fecha de cierre");

        try
        {
            var query = @"
            SELECT
                DATEDIFF(DAY, GETDATE(), vfc.FECHA_CIERRE) AS dias_restantes_cierre 
            FROM  
                VERSION_FECHA_CIERRE vfc 
            WHERE  
                vfc.activo = 1";

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var diasRestantes = await connection.ExecuteScalarAsync<int>(query);

                _logger.LogInformation("Fin de Proceso de obtener los días restantes hasta la fecha de cierre");
                return diasRestantes;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ocurrió un error inesperado al obtener los días restantes hasta la fecha de cierre: {ex.Message}");
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener los días restantes hasta la fecha de cierre" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<Datos<IEnumerable<PlanificacionFinancieraDTO>>> ListadoPlanificacionFinanciera(PlanificacionFinancieraRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el listado de versiones de planificacion financiera");

        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
        string filtrosAdicionales = string.Empty;
        string paginacion = @" OFFSET @saltarRegistros ROWS
                            FETCH NEXT @cantidadRegistros ROWS ONLY";
        try
        {
            if (!string.IsNullOrEmpty(request.NumeroObjetoGasto?.ToString()) ||
                    !string.IsNullOrEmpty(request.NumeroFuenteFinanciador?.ToString()) ||
                    !string.IsNullOrEmpty(request.NumeroOrgFinanciador?.ToString()) ||
                    !string.IsNullOrEmpty(request.NumeroDpto?.ToString()) ||
                    !string.IsNullOrEmpty(request.PresupuestoAprobado?.ToString()) ||
                    !string.IsNullOrEmpty(request.Fundamentacion) ||
                    !string.IsNullOrEmpty(request.Diferencia) ||
                     !string.IsNullOrEmpty(request.TotalMensual) ||
                    !string.IsNullOrEmpty(request.TerminoDeBusqueda))
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    filtrosAdicionales += @"
                            AND (
                                CONVERT(NVARCHAR, t1.EJERCICIO) LIKE '%' + @TerminoDeBusqueda + '%'                       
                                OR CONVERT(NVARCHAR, t3.numero_og) LIKE '%' + @TerminoDeBusqueda + '%'
                                OR CONVERT(NVARCHAR, t1.NUMERO_FF) LIKE '%' + @TerminoDeBusqueda + '%'
                                OR CONVERT(NVARCHAR, t1.NUMERO_OF) LIKE '%' + @TerminoDeBusqueda + '%'
                                OR CONVERT(NVARCHAR, t1.NUMERO_DPTO) LIKE '%' + @TerminoDeBusqueda + '%'                               
                                OR CONVERT(NVARCHAR, t1.presupuesto_aprobado) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
                                OR  CONVERT(NVARCHAR,ISNULL((isnull((enero + febrero + marzo + abril + mayo + junio + julio + agosto + septiembre + octubre + noviembre + diciembre), 0)-presupuesto_aprobado), 0) ) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
                                OR  CONVERT(NVARCHAR,ISNULL((isnull((enero + febrero + marzo + abril + mayo + junio + julio + agosto + septiembre + octubre + noviembre + diciembre), 0)), 0) ) LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'

OR t3.FUNDAMENTACION LIKE '%' + @TerminoDeBusqueda + '%'
                            )";
                }
                else
                {
                    filtrosAdicionales += @"
                            AND (@ejercicio IS NULL OR CONVERT(NVARCHAR, t1.EJERCICIO) LIKE '%' + @ejercicio + '%')     
                            AND (@nroObjetoGasto IS NULL OR CONVERT(NVARCHAR, t3.numero_og) LIKE '%' + @nroObjetoGasto + '%')                       
                            AND (@nroFuenteFinanciador IS NULL OR CONVERT(NVARCHAR, t1.NUMERO_FF) LIKE '%' + @nroFuenteFinanciador + '%')                       
                            AND (@nroOrgFinanciador IS NULL OR CONVERT(NVARCHAR, t1.NUMERO_OF) LIKE '%' + @nroOrgFinanciador + '%')                       
                            AND (@nroDpto IS NULL OR CONVERT(NVARCHAR, t1.NUMERO_DPTO) LIKE '%' + @nroDpto + '%')   
                            AND (@presupAprobado IS NULL OR CONVERT(NVARCHAR, t1.presupuesto_aprobado) LIKE '%' + REPLACE(@presupAprobado, '.', '') + '%')   
                            AND (@diferencia IS NULL OR  CONVERT(NVARCHAR,ISNULL((isnull((enero + febrero + marzo + abril + mayo + junio + julio + agosto + septiembre + octubre + noviembre + diciembre), 0)-presupuesto_aprobado), 0) ) LIKE '%' + REPLACE(@diferencia, '.', '') + '%')
                            AND (@totalMensual IS NULL OR  CONVERT(NVARCHAR,ISNULL((isnull((enero + febrero + marzo + abril + mayo + junio + julio + agosto + septiembre + octubre + noviembre + diciembre), 0)), 0) ) LIKE '%' + REPLACE(@totalMensual, '.', '') + '%')
                           
AND (@fundamentacion IS NULL OR t3.FUNDAMENTACION LIKE '%' + @fundamentacion + '%')";
                }
            }

            var query = $@"
                            SELECT  
                                t1.CODIGO_VERSION as CodigoVersion,
	                            t1.codigo_ff as CodigoFF,
	                            t1.codigo_of as CodigoOF, 
                                t1.codigo_objeto_gasto as CodigoOBG,
                                EJERCICIO as Ejercicio,
	                            numero_og as NumeroObjetoGasto, 
	                            numero_ff as NumeroFuenteFinanciador, 
	                            numero_of as NumeroOrgFinanciador, 
	                            numero_dpto as NumeroDpto, 
	                            fundamentacion as Fundamentacion,
	                            presupuesto_aprobado as PresupuestoAprobado, 
	                            ISNULL(enero,0) as Enero,	
	                            ISNULL(febrero,0) as Febrero,
	                            ISNULL(marzo,0) as Marzo,  
	                            ISNULL(abril,0) as Abril,  
	                            ISNULL(mayo,0) as Mayo,  
	                            ISNULL(junio,0) as Junio, 
	                            ISNULL(julio,0) as Julio, 
	                            ISNULL(agosto,0) as Agosto, 
	                            ISNULL(septiembre,0) as Septiembre, 
	                            ISNULL(octubre,0) as Octubre,  
	                            ISNULL(noviembre,0) as Noviembre,  
	                            ISNULL(diciembre,0) as Diciembre, 
	                            ISNULL((enero + febrero + marzo + abril + mayo + junio + julio + agosto + septiembre + octubre + noviembre + diciembre),0) AS TotalMensual, 
	                            ISNULL((isnull((enero + febrero + marzo + abril + mayo + junio + julio + agosto + septiembre + octubre + noviembre + diciembre), 0)-presupuesto_aprobado), 0) as Diferencia 
                            FROM  
                            ( 
                                SELECT 
                                    va.CODIGO_VERSION, 
		                            ff.codigo_fuente_financiamiento as codigo_ff,
		                            fo.codigo_organismo_financiador as codigo_of,
                                    ff.numero_fuente_financiamiento AS subgrupo_ff, 
                                    fo.numero_organismo_financiador AS subgrupo_of, 
                                    crc.numero_departamento AS subgrupo_dpto, 
                                    ff.numero_fuente_financiamiento AS numero_ff, 
                                    fo.numero_organismo_financiador AS numero_of, 
                                    crc.numero_departamento AS numero_dpto, 
		                            COALESCE(SUM(CONVERT(DECIMAL(18,2), vab.VALOR_UNITARIO) * CONVERT(BIGINT, vab.CANTIDAD)), 0) AS presupuesto_aprobado,
                                    cp.codigo_objeto_gasto, 
		                            vao.CODIGO_FUENTE_FINANCIAMIENTO, vao.CODIGO_ORGANISMO_FINANCIADOR, 
		                            CAST(crc.numero_tipo_presupuesto AS varchar) + '-' + crc.descripcion_tipo_presupuesto AS tipo_presupuesto1, 
		                            CAST(crc.numero_programa AS varchar) + '-' + crc.descripcion_programa AS programa1, 
		                            CAST(crc.numero_actividad AS varchar) + '-' + crc.descripcion_actividad AS actividad1, 
		                            (select r.valor_alfanumerico from referencias r 
		                            where r.codigo_tipo_referencia=(select tr.codigo_tipo_referencia from tipo_referencias tr where tr.descripcion_tipo_referencia like '%ENTIDAD_PRESUPUESTO%')) AS entidad, 
		                            va.EJERCICIO 
                                FROM versiones_anteproyectos_objetos vao  
                                JOIN versiones_anteproyectos_bienes vab ON (vab.codigo_anteproyecto_objeto = vao.codigo_anteproyecto_objeto AND vab.codigo_version = vao.codigo_version) 
                                JOIN configuracion_presupuestaria cp ON (cp.codigo_configuracion_presupuestaria = vao.codigo_configuracion_presupuestaria) 
                                JOIN versiones_anteproyectos va ON (va.codigo_version = vao.codigo_version) 
                                JOIN vlistacentrosresponsabilidadporcircunscripcion crc ON  (crc.codigo_centro_responsabilidad = cp.codigo_centro_responsabilidad AND crc.codigo_materia = cp.codigo_materia) 
                                JOIN fuente_financiamiento ff ON (ff.codigo_fuente_financiamiento = vao.codigo_fuente_financiamiento)  
                                JOIN organismo_financiador fo ON (fo.codigo_organismo_financiador = vao.codigo_organismo_financiador) 
                                GROUP BY  
                                va.CODIGO_VERSION, 
	                            ff.numero_fuente_financiamiento,  
                                fo.numero_organismo_financiador,  
                                crc.numero_departamento,  
                                cp.CODIGO_OBJETO_GASTO,  
	                            vao.CODIGO_FUENTE_FINANCIAMIENTO, vao.CODIGO_ORGANISMO_FINANCIADOR 
	                            , crc.numero_tipo_presupuesto, crc.descripcion_tipo_presupuesto, 
	                            crc.numero_programa, crc.descripcion_programa, 
	                            crc.numero_actividad, crc.descripcion_actividad, 
	                            va.EJERCICIO,ff.codigo_fuente_financiamiento,fo.codigo_organismo_financiador
                            ) AS t1 
                            LEFT JOIN  
                            ( 
                                SELECT 
                                    va.CODIGO_VERSION, 
		                            cp.codigo_objeto_gasto,  
                                    vao.codigo_fuente_financiamiento,  
                                    vao.codigo_organismo_financiador,
                                    SUM(CASE WHEN p.mes = 1 THEN p.monto ELSE 0 END) AS enero,
                                    SUM(CASE WHEN p.mes = 2 THEN p.monto ELSE 0 END) AS febrero, 
                                    SUM(CASE WHEN p.mes = 3 THEN p.monto ELSE 0 END) AS marzo, 
                                    SUM(CASE WHEN p.mes = 4 THEN p.monto ELSE 0 END) AS abril, 
                                    SUM(CASE WHEN p.mes = 5 THEN p.monto ELSE 0 END) AS mayo, 
                                    SUM(CASE WHEN p.mes = 6 THEN p.monto ELSE 0 END) AS junio, 
                                    SUM(CASE WHEN p.mes = 7 THEN p.monto ELSE 0 END) AS julio, 
                                    SUM(CASE WHEN p.mes = 8 THEN p.monto ELSE 0 END) AS agosto, 
                                    SUM(CASE WHEN p.mes = 9 THEN p.monto ELSE 0 END) AS septiembre, 
                                    SUM(CASE WHEN p.mes = 10 THEN p.monto ELSE 0 END) AS octubre, 
                                    SUM(CASE WHEN p.mes = 11 THEN p.monto ELSE 0 END) AS noviembre, 
                                    SUM(CASE WHEN p.mes = 12 THEN p.monto ELSE 0 END) AS diciembre 
                                FROM VERSIONES_ANTEPROYECTO_PLANIFICACION p 
                                JOIN versiones_anteproyectos_objetos vao ON (vao.codigo_anteproyecto_objeto=p.codigo_anteproyecto_objeto  
                                AND vao.codigo_version=p.codigo_version) 
                                JOIN configuracion_presupuestaria cp ON (cp.codigo_configuracion_presupuestaria=vao.codigo_configuracion_presupuestaria) 
	                            JOIN VERSIONES_ANTEPROYECTOS va on (va.CODIGO_VERSION=vao.CODIGO_VERSION) 
	                            GROUP BY 
                                    va.CODIGO_VERSION,
                            cp.codigo_objeto_gasto,  
                                    vao.codigo_fuente_financiamiento, 
                                    vao.codigo_organismo_financiador  
                            ) AS t2 ON t1.codigo_objeto_gasto = t2.codigo_objeto_gasto  
                                    AND t1.CODIGO_FUENTE_FINANCIAMIENTO = t2.CODIGO_FUENTE_FINANCIAMIENTO 
                                    AND t1.codigo_organismo_financiador = t2.codigo_organismo_financiador 
                            AND t1.CODIGO_VERSION=t2.CODIGO_VERSION 
                            LEFT JOIN ( 
                            select distinct 
                            bp2.numero_grupo + ' - ' + bp2.descripcion_grupo AS grupo, bp2.numero_subgrupo, bp2.descripcion_subgrupo, 
                            bp2.numero_objeto_gasto as numero_og,bp2.descripcion_objeto_gasto as fundamentacion,  
                            vao.CODIGO_VERSION,cp.CODIGO_OBJETO_GASTO,vao.CODIGO_FUENTE_FINANCIAMIENTO, vao.CODIGO_ORGANISMO_FINANCIADOR 
                            from VERSIONES_ANTEPROYECTOS_OBJETOS vao  
                            JOIN configuracion_presupuestaria cp ON (cp.codigo_configuracion_presupuestaria = vao.codigo_configuracion_presupuestaria) 
                            join vListaBienesPrioritarios bp2 on bp2.codigo_objeto_gasto=cp.CODIGO_OBJETO_GASTO 
                            ) as t3 on t3.CODIGO_VERSION=t1.CODIGO_VERSION  
                            and t3.CODIGO_OBJETO_GASTO=t1.CODIGO_OBJETO_GASTO 
                            and t3.CODIGO_FUENTE_FINANCIAMIENTO=t1.CODIGO_FUENTE_FINANCIAMIENTO 
                            and t3.CODIGO_ORGANISMO_FINANCIADOR=t1.CODIGO_ORGANISMO_FINANCIADOR 
                            WHERE t1.codigo_version = @codigoVersion
                            {filtrosAdicionales}
                            order by numero_og, numero_ff, numero_of  {paginacion}";


           // var paginacion = $"ORDER BY t1.ObjetoGasto Asc OFFSET {saltarRegistros} ROWS FETCH NEXT {request.CantidadRegistros} ROWS ONLY";

            //query = string.Format(query, filtrosAdicionales, paginacion);

            var queryCantidadTotalRegistros = $@"
                            SELECT COUNT(*) AS TotalRegistros FROM (
                                SELECT  
                                    EJERCICIO as Ejercicio,
                                    numero_og as NumeroObjetoGasto, 
                                    numero_ff as NumeroFuenteFinanciador, 
                                    numero_of as NumeroOrgFinanciador, 
                                    numero_dpto as NumeroDpto, 
                                    fundamentacion as Fundamentacion,
                                    presupuesto_aprobado as PresupuestoAprobado, 
                                    ISNULL(enero,0) as Enero,   
                                    ISNULL(febrero,0) as Febrero,
                                    ISNULL(marzo,0) as Marzo,  
                                    ISNULL(abril,0) as Abril,  
                                    ISNULL(mayo,0) as Mayo,  
                                    ISNULL(junio,0) as Junio, 
                                    ISNULL(julio,0) as Julio, 
                                    ISNULL(agosto,0) as Agosto, 
                                    ISNULL(septiembre,0) as Septiembre, 
                                    ISNULL(octubre,0) as Octubre,  
                                    ISNULL(noviembre,0) as Noviembre,  
                                    ISNULL(diciembre,0) as Diciembre, 
                                    ISNULL((enero + febrero + marzo + abril + mayo + junio + julio + agosto + septiembre + octubre + noviembre + diciembre),0) AS TotalMensual, 
                                    ISNULL((isnull((enero + febrero + marzo + abril + mayo + junio + julio + agosto + septiembre + octubre + noviembre + diciembre), 0)-presupuesto_aprobado), 0) as Diferencia 
                                FROM  
                                ( 
                                    SELECT 
                                        va.CODIGO_VERSION, 
                                        ff.numero_fuente_financiamiento AS subgrupo_ff, 
                                        fo.numero_organismo_financiador AS subgrupo_of, 
                                        crc.numero_departamento AS subgrupo_dpto, 
                                        ff.numero_fuente_financiamiento AS numero_ff, 
                                        fo.numero_organismo_financiador AS numero_of, 
                                        crc.numero_departamento AS numero_dpto,  
                                        COALESCE(SUM(CONVERT(DECIMAL(18,2), vab.VALOR_UNITARIO) * CONVERT(BIGINT, vab.CANTIDAD)), 0) AS presupuesto_aprobado,
                                        cp.codigo_objeto_gasto, 
                                        vao.CODIGO_FUENTE_FINANCIAMIENTO, vao.CODIGO_ORGANISMO_FINANCIADOR, 
                                        CAST(crc.numero_tipo_presupuesto AS varchar) + '-' + crc.descripcion_tipo_presupuesto AS tipo_presupuesto1, 
                                        CAST(crc.numero_programa AS varchar) + '-' + crc.descripcion_programa AS programa1, 
                                        CAST(crc.numero_actividad AS varchar) + '-' + crc.descripcion_actividad AS actividad1, 
                                        (select r.valor_alfanumerico from referencias r 
                                        where r.codigo_tipo_referencia=(select tr.codigo_tipo_referencia from tipo_referencias tr where tr.descripcion_tipo_referencia like '%ENTIDAD_PRESUPUESTO%')) AS entidad, 
                                        va.EJERCICIO 
                                    FROM versiones_anteproyectos_objetos vao  
                                    JOIN versiones_anteproyectos_bienes vab ON (vab.codigo_anteproyecto_objeto = vao.codigo_anteproyecto_objeto AND vab.codigo_version = vao.codigo_version) 
                                    JOIN configuracion_presupuestaria cp ON (cp.codigo_configuracion_presupuestaria = vao.codigo_configuracion_presupuestaria) 
                                    JOIN versiones_anteproyectos va ON (va.codigo_version = vao.codigo_version) 
                                    JOIN vlistacentrosresponsabilidadporcircunscripcion crc ON  (crc.codigo_centro_responsabilidad = cp.codigo_centro_responsabilidad AND crc.codigo_materia = cp.codigo_materia) 
                                    JOIN fuente_financiamiento ff ON (ff.codigo_fuente_financiamiento = vao.codigo_fuente_financiamiento)  
                                    JOIN organismo_financiador fo ON (fo.codigo_organismo_financiador = vao.codigo_organismo_financiador) 
                                    GROUP BY  
                                        va.CODIGO_VERSION, 
                                        ff.numero_fuente_financiamiento,  
                                        fo.numero_organismo_financiador,  
                                        crc.numero_departamento,  
                                        cp.CODIGO_OBJETO_GASTO,  
                                        vao.CODIGO_FUENTE_FINANCIAMIENTO, vao.CODIGO_ORGANISMO_FINANCIADOR 
                                        , crc.numero_tipo_presupuesto, crc.descripcion_tipo_presupuesto, 
                                        crc.numero_programa, crc.descripcion_programa, 
                                        crc.numero_actividad, crc.descripcion_actividad, 
                                        va.EJERCICIO 
                                ) AS t1 
                                LEFT JOIN  
                                ( 
                                    SELECT 
                                        va.CODIGO_VERSION, 
                                        cp.codigo_objeto_gasto,  
                                        vao.codigo_fuente_financiamiento,  
                                        vao.codigo_organismo_financiador,
                                        SUM(CASE WHEN p.mes = 1 THEN p.monto ELSE 0 END) AS enero,
                                        SUM(CASE WHEN p.mes = 2 THEN p.monto ELSE 0 END) AS febrero, 
                                        SUM(CASE WHEN p.mes = 3 THEN p.monto ELSE 0 END) AS marzo, 
                                        SUM(CASE WHEN p.mes = 4 THEN p.monto ELSE 0 END) AS abril, 
                                        SUM(CASE WHEN p.mes = 5 THEN p.monto ELSE 0 END) AS mayo, 
                                        SUM(CASE WHEN p.mes = 6 THEN p.monto ELSE 0 END) AS junio, 
                                        SUM(CASE WHEN p.mes = 7 THEN p.monto ELSE 0 END) AS julio, 
                                        SUM(CASE WHEN p.mes = 8 THEN p.monto ELSE 0 END) AS agosto, 
                                        SUM(CASE WHEN p.mes = 9 THEN p.monto ELSE 0 END) AS septiembre, 
                                        SUM(CASE WHEN p.mes = 10 THEN p.monto ELSE 0 END) AS octubre, 
                                        SUM(CASE WHEN p.mes = 11 THEN p.monto ELSE 0 END) AS noviembre, 
                                        SUM(CASE WHEN p.mes = 12 THEN p.monto ELSE 0 END) AS diciembre 
                                    FROM VERSIONES_ANTEPROYECTO_PLANIFICACION p 
                                    JOIN versiones_anteproyectos_objetos vao ON (vao.codigo_anteproyecto_objeto=p.codigo_anteproyecto_objeto  
                                    AND vao.codigo_version=p.codigo_version) 
                                    JOIN configuracion_presupuestaria cp ON (cp.codigo_configuracion_presupuestaria=vao.codigo_configuracion_presupuestaria) 
                                    JOIN VERSIONES_ANTEPROYECTOS va on (va.CODIGO_VERSION=vao.CODIGO_VERSION) 
                                    GROUP BY 
                                        va.CODIGO_VERSION,
                                cp.codigo_objeto_gasto,  
                                        vao.codigo_fuente_financiamiento, 
                                        vao.codigo_organismo_financiador  
                                ) AS t2 ON t1.codigo_objeto_gasto = t2.codigo_objeto_gasto  
                                        AND t1.CODIGO_FUENTE_FINANCIAMIENTO = t2.CODIGO_FUENTE_FINANCIAMIENTO 
                                        AND t1.codigo_organismo_financiador = t2.codigo_organismo_financiador 
                                AND t1.CODIGO_VERSION=t2.CODIGO_VERSION 
                                LEFT JOIN ( 
                                select distinct 
                                bp2.numero_grupo + ' - ' + bp2.descripcion_grupo AS grupo, bp2.numero_subgrupo, bp2.descripcion_subgrupo, 
                                bp2.numero_objeto_gasto as numero_og,bp2.descripcion_objeto_gasto as fundamentacion,  
                                vao.CODIGO_VERSION,cp.CODIGO_OBJETO_GASTO,vao.CODIGO_FUENTE_FINANCIAMIENTO, vao.CODIGO_ORGANISMO_FINANCIADOR 
                                from VERSIONES_ANTEPROYECTOS_OBJETOS vao  
                                JOIN configuracion_presupuestaria cp ON (cp.codigo_configuracion_presupuestaria = vao.codigo_configuracion_presupuestaria) 
                                join vListaBienesPrioritarios bp2 on bp2.codigo_objeto_gasto=cp.CODIGO_OBJETO_GASTO 
                                ) as t3 on t3.CODIGO_VERSION=t1.CODIGO_VERSION  
                                and t3.CODIGO_OBJETO_GASTO=t1.CODIGO_OBJETO_GASTO 
                                and t3.CODIGO_FUENTE_FINANCIAMIENTO=t1.CODIGO_FUENTE_FINANCIAMIENTO 
                                and t3.CODIGO_ORGANISMO_FINANCIADOR=t1.CODIGO_ORGANISMO_FINANCIADOR 
                                WHERE t1.codigo_version = @codigoVersion
                                {filtrosAdicionales} 
                            ) AS CountQuery";

            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@ejercicio", request.Ejercicio?.ToString());
            parametros.Add("@nroObjetoGasto", request.NumeroObjetoGasto?.ToString());
            parametros.Add("@nroOrgFinanciador", request.NumeroOrgFinanciador?.ToString());
            parametros.Add("@nroFuenteFinanciador", request.NumeroFuenteFinanciador?.ToString());
            parametros.Add("@nroDpto", request.NumeroDpto.ToString());
            parametros.Add("@fundamentacion", request.Fundamentacion);
            parametros.Add("@presupAprobado", request.PresupuestoAprobado?.ToString());
            parametros.Add("@Ejercicio", request.Ejercicio?.ToString());           
            parametros.Add("@TerminoDeBusqueda", request.TerminoDeBusqueda);
            parametros.Add("@codigoVersion", request.CodigoVersion);           
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);
            parametros.Add("@diferencia", request.Diferencia); 
            parametros.Add("@totalMensual", request.TotalMensual);
            parametros.Add("@saltarRegistros", saltarRegistros);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QueryAsync<PlanificacionFinancieraDTO>(query, parametros);

                var response = new Datos<IEnumerable<PlanificacionFinancieraDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalRegistros
                };

                _logger.LogInformation("Fin de Proceso de obtener el listado de versiones de planificacion financiera");
                return response;
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener el listado de la planificacion financiera" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<IEnumerable<PlanificacionFinancieraporObjetoGastoDTO>> ListadoPlanificacionFinancieraporObjetoGasto(PlanificacionFinancieraporObjetoGastoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el listado de versiones de planificacion financiera por Objeto de Gastos");
             

        try
        {           
            var query = @"
                           SELECT  
                                vao.CODIGO_ANTEPROYECTO_OBJETO as CodigoAnteproyectoObjeto, 
                                crc.descripcion_actividad as Actividad, 
                                va.EJERCICIO as Ejercicio, 
                                t2.numero_og+'-'+ t2.fundamentacion as ObjetoGasto, 
                                ff.codigo_fuente_financiamiento as CodigoFF,
                                ff.numero_fuente_financiamiento as FuenteFinanciamiento, 
                                f.codigo_organismo_financiador as CodigoOG,
                                f.numero_organismo_financiador as OrganismoFinanciador,  
                                crc.codigo_centro_responsabilidad as CodigoCentroResponsabilidad,
                                crc.descripcion_centro_responsabilidad as CentroResponsabilidad, 
                                crc.codigo_materia as CodigoMateria,
                                crc.descripcion_materia as Materia, 
                                vao.evento as Evento,  
                                COALESCE(SUM(CONVERT(DECIMAL(18), vab.VALOR_UNITARIO) * CONVERT(BIGINT, vab.CANTIDAD)), 0) AS SumaValorTotal

                                FROM versiones_anteproyectos_objetos vao  
                                JOIN versiones_anteproyectos va ON (vao.codigo_version = va.codigo_version) 
                                JOIN configuracion_presupuestaria cp ON (cp.codigo_configuracion_presupuestaria = vao.codigo_configuracion_presupuestaria)
                                JOIN fuente_financiamiento ff ON (ff.codigo_fuente_financiamiento = vao.codigo_fuente_financiamiento) 
                                JOIN organismo_financiador f ON (f.codigo_organismo_financiador = vao.codigo_organismo_financiador) 
                                JOIN versiones_anteproyectos_bienes vab ON (vab.codigo_anteproyecto_objeto = vao.codigo_anteproyecto_objeto  
                                AND vab.codigo_version = vao.codigo_version ) 
                                JOIN vListaCentrosResponsabilidadPorCircunscripcion crc on (crc.codigo_centro_responsabilidad=cp.CODIGO_CENTRO_RESPONSABILIDAD
                                and crc.codigo_materia=cp.CODIGO_MATERIA) 
                                JOIN( 
                                select distinct 
                                bp2.numero_objeto_gasto as numero_og, 
                                bp2.descripcion_objeto_gasto as fundamentacion, 
                                vao.CODIGO_VERSION,  
                                vao.CODIGO_ANTEPROYECTO_OBJETO 
                                from VERSIONES_ANTEPROYECTOS_OBJETOS vao  
                                JOIN configuracion_presupuestaria cp ON (cp.codigo_configuracion_presupuestaria = vao.codigo_configuracion_presupuestaria) 
                                join vListaBienesPrioritarios bp2 on bp2.codigo_objeto_gasto=cp.CODIGO_OBJETO_GASTO 
                                ) as t2 on t2.CODIGO_VERSION=vao.CODIGO_VERSION and vao.CODIGO_ANTEPROYECTO_OBJETO=t2.CODIGO_ANTEPROYECTO_OBJETO 

                            WHERE  

                            cp.CODIGO_OBJETO_GASTO = @codigoOBG
                            AND va.CODIGO_VERSION = @codigoVersion
                            AND vao.CODIGO_FUENTE_FINANCIAMIENTO = @codigoff
                            AND vao.CODIGO_ORGANISMO_FINANCIADOR = @codigoof

                            group by vao.CODIGO_ANTEPROYECTO_OBJETO,crc.descripcion_actividad, 
                            va.EJERCICIO,t2.numero_og,t2.fundamentacion,ff.numero_fuente_financiamiento ,  
                            f.numero_organismo_financiador,crc.descripcion_centro_responsabilidad, 
                            crc.descripcion_materia,vao.evento,crc.codigo_centro_responsabilidad,
                            crc.codigo_materia,ff.codigo_fuente_financiamiento,f.codigo_organismo_financiador";

          
            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@codigoOBG", request.CodigoObjetoGasto);
            parametros.Add("@codigoVersion", request.CodigoVersion);
            parametros.Add("@codigoff", request.CodigoFuenteFinanciador);
            parametros.Add("@codigoof", request.CodigoOrganismoFinanciador);       
           

            using (var connection = this._conexion.CreateSqlConnection())
            {               
                var resultado = await connection.QueryAsync<PlanificacionFinancieraporObjetoGastoDTO>(query, parametros);              

                _logger.LogInformation("Fin de Proceso de obtener el listado de versiones de planificacion financiera por Objeto de Gastos");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener el listado de la planificacion financiera por objeto de gasto" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<IEnumerable<PlanificacionMensualDTO>> ListadoPlanificacionMensual(PlanificacionMensualRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el listado de versiones de planificacion mensual");

        try
        {
            var query = @"
                           SELECT  
                                vap.CODIGO_ANTEPROYECTO_PLANIFICACON as CodigoAnteproyectoPlanificacion,
                                vap.MES as NumeroMes,
                                DATENAME(month, DATEADD(month, vap.MES - 1, '1900-01-01')) AS Mes, 
	                            CONVERT(DECIMAL(18), vap.MONTO) as Monto

                            FROM VERSIONES_ANTEPROYECTO_PLANIFICACION vap 
                            JOIN VERSIONES_ANTEPROYECTOS_OBJETOS vao ON vao.CODIGO_ANTEPROYECTO_OBJETO = vap.CODIGO_ANTEPROYECTO_OBJETO  
                            AND vao.CODIGO_VERSION = vap.CODIGO_VERSION 
                            JOIN CONFIGURACION_PRESUPUESTARIA cp ON cp.CODIGO_CONFIGURACION_PRESUPUESTARIA = vao.CODIGO_CONFIGURACION_PRESUPUESTARIA 

                            WHERE vao.CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBG
                            AND vao.CODIGO_VERSION = @codigoVersion 

                            ORDER BY  
                                vao.CODIGO_ANTEPROYECTO_OBJETO,vap.MES";


            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@codigoAnteproyectoOBG", request.CodigoAnteproyectoObjeto);
            parametros.Add("@codigoVersion", request.CodigoVersion);


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<PlanificacionMensualDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de obtener el listado de versiones de planificacion mensual");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener el listado de la planificacion mensual" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<CodigoAnteproyectoOBJparaPlanificacionFinancieraDTO> ObtenerCodigoAnteproyectoObjeto(CodigoAnteproyectoOBJparaPlanificacionFinancieraRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el codigo del anteproyecto objeto");

        try
        {
            var query = @"
                           SELECT  
                            vao.CODIGO_ANTEPROYECTO_OBJETO  as CodigoAnteproyectoObjeto

                            FROM versiones_anteproyectos va
                            JOIN versiones_anteproyectos_objetos vao ON vao.codigo_version = va.codigo_version 
                            JOIN configuracion_presupuestaria cp ON cp.codigo_configuracion_presupuestaria = vao.codigo_configuracion_presupuestaria
                            JOIN fuente_financiamiento ff ON ff.codigo_fuente_financiamiento = vao.codigo_fuente_financiamiento 
                            JOIN organismo_financiador f ON f.codigo_organismo_financiador = vao.codigo_organismo_financiador 

                            WHERE  

                                cp.CODIGO_CENTRO_RESPONSABILIDAD = @codigoCentroResponsabilidad  
                            AND cp.CODIGO_MATERIA = @codigoMateria 
                            AND cp.CODIGO_OBJETO_GASTO = @codigoOBJ
                                AND va.CODIGO_VERSION = @codigoVersion
                            AND vao.CODIGO_FUENTE_FINANCIAMIENTO = @codigoFF
                            AND vao.CODIGO_ORGANISMO_FINANCIADOR = @codigoOG

                            GROUP BY  
                            vao.CODIGO_ANTEPROYECTO_OBJETO";


            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@codigoCentroResponsabilidad", request.CodigoCentroResponsabilidad);
            parametros.Add("@codigoMateria", request.CodigoMateria);
            parametros.Add("@codigoOBJ", request.CodigoObjetoGasto);
            parametros.Add("@codigoVersion", request.CodigoVersion);
            parametros.Add("@codigoFF", request.CodigoFF);
            parametros.Add("@codigoOG", request.CodigoOG);


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<CodigoAnteproyectoOBJparaPlanificacionFinancieraDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de obtener el codigo del anteproyecto objeto");

                return resultado.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener el codigo de anteproyecto objeto" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<IEnumerable<MesesparaPlanificacionFinancieraDTO>> ObtenerMesesParalaPlanificacionFinanciera(MesesparaPlanificacionFinancieraRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener los meses para la planificacion mensual");

        try
        {
            var query = @"
                           WITH Meses AS (
                            SELECT 1 AS Mes, 'Enero' AS NombreMes UNION ALL
                            SELECT 2, 'Febrero' UNION ALL
                            SELECT 3, 'Marzo' UNION ALL
                            SELECT 4, 'Abril' UNION ALL
                            SELECT 5, 'Mayo' UNION ALL
                            SELECT 6, 'Junio' UNION ALL
                            SELECT 7, 'Julio' UNION ALL
                            SELECT 8, 'Agosto' UNION ALL
                            SELECT 9, 'Septiembre' UNION ALL
                            SELECT 10, 'Octubre' UNION ALL
                            SELECT 11, 'Noviembre' UNION ALL
                            SELECT 12, 'Diciembre'
                        )
                        SELECT m.Mes, m.NombreMes
                        FROM Meses m
                        LEFT JOIN [dbo].[VERSIONES_ANTEPROYECTO_PLANIFICACION] v
                            ON m.Mes = v.MES
                            AND v.CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoObjeto
                            AND v.CODIGO_VERSION = @codigoVersion
                        WHERE v.MES IS NULL";


            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@codigoAnteproyectoObjeto", request.CodigoAnteproyectoObjeto);
            parametros.Add("@codigoVersion", request.CodigoVersion);     

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<MesesparaPlanificacionFinancieraDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de obtener los meses para la planificacion mensual");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener los meses para la planificacion mensual" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> InsertarPlanificacionFinanciera(InsertarPlanificacionFinancieraRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de insertar la planificacion financiera en la tabla versiones anteproyecto planificacion");

        try
        {          
            string query_UltimoValorCodigo = "SELECT ISNULL(MAX(CODIGO_ANTEPROYECTO_PLANIFICACON), 0) " +
                "FROM VERSIONES_ANTEPROYECTO_PLANIFICACION WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ AND CODIGO_VERSION = @codigoVersion";
            
            string query = @"INSERT INTO VERSIONES_ANTEPROYECTO_PLANIFICACION
                     (CODIGO_ANTEPROYECTO_PLANIFICACON,CODIGO_ANTEPROYECTO_OBJETO,CODIGO_VERSION,MES,MONTO,USUARIO_INSERTO,FECHA_INSERTO)
                      VALUES (@codigoPlanificacion, @codigoAnteproyectoOBJ, @codigoVersion, @mes, @monto, @usuario,GETDATE())";
                          
            using (var connection = this._conexion.CreateSqlConnection())
            {           
                var parametros = new DynamicParameters();

                int valorCodigo = await connection.ExecuteScalarAsync<int>(query_UltimoValorCodigo,
                new { codigoVersion = request.CodigoVersion, codigoAnteproyectoOBJ = request.CodigoAnteproyectoObjeto });

                int codigoPlanificacion = valorCodigo + 1;
                parametros.Add("@codigoPlanificacion", codigoPlanificacion);
                parametros.Add("@codigoAnteproyectoOBJ", request.CodigoAnteproyectoObjeto );
                parametros.Add("@codigoVersion", request.CodigoVersion);
                parametros.Add("@mes", request.Mes);
                parametros.Add("@monto", request.Monto);
                parametros.Add("@usuario", request.UsuarioInserto);

                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin de Proceso de insertar la planificacion financiera en la tabla versiones anteproyecto planificacion");

                return resultado;
             
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al insertar en la tabla de versiones anteproyecto planificacion" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<decimal> ValidarMontodePlanificacionFinanciera(MesesparaPlanificacionFinancieraRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el monto de planificaciones financieras realizadas");

        try
        {
            var query = @"
                         SELECT COALESCE(SUM(CONVERT(DECIMAL(18), vap.MONTO)),0)
                            FROM VERSIONES_ANTEPROYECTO_PLANIFICACION vap
                            WHERE vap.CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoObjeto
                            AND vap.CODIGO_VERSION = @codigoVersion";


            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@codigoAnteproyectoObjeto", request.CodigoAnteproyectoObjeto);
            parametros.Add("@codigoVersion", request.CodigoVersion);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QuerySingleAsync<decimal>(query, parametros);

                _logger.LogInformation("Fin de Proceso de obtener el monto de planificaciones financieras realizadas");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener el monto de las planificaciones financieras" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> ValidarCantidadMesesporEvento(MesesparaPlanificacionFinancieraRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener la cantidad de meses para validar la cantidad de eventos");

        try
        {
            var query = @"
                  SELECT COUNT(DISTINCT vap.MES)
                        FROM VERSIONES_ANTEPROYECTO_PLANIFICACION vap
                        WHERE vap.CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoObjeto
                        AND vap.CODIGO_VERSION = @codigoVersion";


            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@codigoAnteproyectoObjeto", request.CodigoAnteproyectoObjeto);
            parametros.Add("@codigoVersion", request.CodigoVersion);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QuerySingleAsync<int>(query, parametros);

                _logger.LogInformation("Fin de Proceso de obtener la cantidad de meses para validar la cantidad de eventos");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener la cantidad de meses para validar la cantidad de evento" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> EditarPlanificacionFinanciera(InsertarPlanificacionFinancieraRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de actualizar la planificacion financiera en la tabla versiones anteproyecto planificacion");

        try
        {            
            string query = @"UPDATE VERSIONES_ANTEPROYECTO_PLANIFICACION
                                SET MES = @mes,
	                                MONTO = @monto,
	                                USUARIO_MODIFICACION = @usuario,
	                                FECHA_MODIFICACION = GETDATE()
                                WHERE	
	                                CODIGO_ANTEPROYECTO_PLANIFICACON = @codigoPlanificacion
                                AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ
                                AND CODIGO_VERSION = @codigoVersion";

            using (var connection = this._conexion.CreateSqlConnection())
            {             
                var parametros = new DynamicParameters();             
                          
                parametros.Add("@codigoPlanificacion", request.CodigoAnteproyectoPlanificacion);
                parametros.Add("@codigoAnteproyectoOBJ", request.CodigoAnteproyectoObjeto);
                parametros.Add("@codigoVersion", request.CodigoVersion);
                parametros.Add("@mes", request.Mes);
                parametros.Add("@monto", request.Monto);
                parametros.Add("@usuario", request.UsuarioInserto);

                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin de Proceso de actualizar la planificacion financiera en la tabla versiones anteproyecto planificacion");

                return resultado;

            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al actualizar en la tabla de versiones anteproyecto planificacion" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<IEnumerable<PlanificacionMensualDTO>> ListadoPlanificacionMensualporItem(PlanificacionMensualporItemRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el listado de versiones de planificacion mensual por Item");

        try
        {
            var query = @"
                        SELECT 
                            MES as Mes,
	                        MONTO as Monto,
	                        CODIGO_ANTEPROYECTO_PLANIFICACON as CodigoAnteproyectoPlanificacion

                        FROM VERSIONES_ANTEPROYECTO_PLANIFICACION
                        WHERE CODIGO_ANTEPROYECTO_PLANIFICACON = @codigoAnteproyectoPlanificacion
                        AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBG
                        AND CODIGO_VERSION = @codigoVersion";


            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@codigoAnteproyectoOBG", request.CodigoAnteproyectoObjeto);
            parametros.Add("@codigoVersion", request.CodigoVersion);
            parametros.Add("@codigoAnteproyectoPlanificacion", request.CodigoAnteproyectoPlanificacion);


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<PlanificacionMensualDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de obtener el listado de versiones de planificacion mensual por Item");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al obtener el listado de la planificacion mensual por Item" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> EliminarPlanificacionFinanciera(EliminarPlanificacionFinancieraRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de eliminar la planificacion financiera en la tabla versiones anteproyecto planificacion");

        try
        {
            string query = @"DELETE FROM VERSIONES_ANTEPROYECTO_PLANIFICACION                                
                                WHERE	
	                                CODIGO_ANTEPROYECTO_PLANIFICACON = @codigoPlanificacion
                                AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ
                                AND CODIGO_VERSION = @codigoVersion";

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var parametros = new DynamicParameters();

                parametros.Add("@codigoPlanificacion", request.CodigoAnteproyectoPlanificacion);
                parametros.Add("@codigoAnteproyectoOBJ", request.CodigoAnteproyectoObjeto);
                parametros.Add("@codigoVersion", request.CodigoVersion);               

                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin de Proceso de eliminar la planificacion financiera en la tabla versiones anteproyecto planificacion");

                return resultado;

            }
        }
        catch (Exception ex)
        {
            throw new PlanificacionFinancieraException("Ocurrió un error inesperado al actualizar en la tabla de versiones anteproyecto planificacion" + "||-->" + ex.Message + "<--||");
        }
    }
}




