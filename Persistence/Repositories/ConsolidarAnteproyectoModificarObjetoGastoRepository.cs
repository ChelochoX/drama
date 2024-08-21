using Application.Services.Interfaces.IRepository;
using Dapper;
using Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;
using Domain.Entities.Request;
using Domain.Exceptions;
using Domain.Exceptions.AsignacionContratoPorObjetosGastos;
using Domain.Exceptions.ImportarArchivoSIPOIExcepcions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Persistence.Repositories;

public class ConsolidarAnteproyectoModificarObjetoGastoRepository : IConsolidarAnteproyectoModificarObjetoGastoRepository
{
    private readonly DbConnections _conexion;
    private readonly ILogger<ConsolidarAnteproyectoModificarObjetoGastoRepository> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ConsolidarAnteproyectoModificarObjetoGastoRepository(ILogger<ConsolidarAnteproyectoModificarObjetoGastoRepository> logger, DbConnections conexion, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _conexion = conexion;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task<IEnumerable<CentroResponsabilidadyMateriaDTO>> ObtenerCentroResponsabilidadyMateriaporUsuario(string cedulaUsuario)
    {
        _logger.LogInformation("Inicio de Proceso de Obtener Centro de responsabilidad y Materia del usuario");

        try
        {
            string query = @"
                            SELECT 
	                            vcr.descripcion_centro_responsabilidad as CentroResponsabilidad,
	                            vcr.descripcion_materia as Materia

                            FROM vListaUsuariosPorCentrosResponsabilidad vucr
                            join vListaCentrosResponsabilidadPorCircunscripcion vcr
                            on vcr.codigo_centro_responsabilidad=vucr.codigo_centro_responsabilidad 
                            and vcr.codigo_materia=vucr.codigo_materia

                            WHERE vucr.cedula_identidad= @cedula"; //4626946


            var parametros = new DynamicParameters();
            parametros.Add("@cedula", cedulaUsuario);
           

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<CentroResponsabilidadyMateriaDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de Obtener Centro de responsabilidad y Materia del usuario");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error inesperado al obtener los datos de materia y centro de responsabilidad" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<IEnumerable<CircunscripcionesparaConsolidadoAnteproyectoDTO>> ObtenerCircunscripciones()
    {
        _logger.LogInformation("Inicio de Proceso de Obtener las circunscripciones");

        try
        {
            string query = @"
                            SELECT 
	                            codigo_circunscripcion as CodigoCircunscripcion,
	                            nombre_circunscripcion as DescripcionCircunscripcion
                            FROM circunscripciones
                            WHERE codigo_circunscripcion NOT IN (-1,0)";           


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<CircunscripcionesparaConsolidadoAnteproyectoDTO>(query);

                _logger.LogInformation("Fin de Proceso de Obtener las circunscripciones");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error inesperado al obtener los datos de las circunscripciones" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<IEnumerable<ObjetosGastosConsolidacionAnteproyectoDTO>> ObtenerObjetosGastos( int ejercicio)
    {
        _logger.LogInformation("Inicio de Proceso de Obtener  Objetos de gastos");

        try
        {
            string query = @"
                            SELECT distinct 
				                vlb.codigo_objeto_gasto as CodigoObjetoGasto,
				                vlb.numero_objeto_gasto as NumeroObjetoGasto,
                                vlb.descripcion_objeto_gasto as DescripcionObjetoGasto

                            FROM vListaBienesPrioritarios vlb  
                            WHERE  vlb.ejercicio in (@ejercicio -1,0) 
                            AND vlb.activo=1";

            var parametros = new DynamicParameters();
            parametros.Add("@ejercicio", ejercicio);


            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<ObjetosGastosConsolidacionAnteproyectoDTO>(query, parametros);

                _logger.LogInformation("Fin de Proceso de Obtener  Objetos de gastos");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error inesperado al obtener los datos de Objetos de Gastos" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<IEnumerable<DatosparaConfiguracionPresupuestariaConsolidadoDTO>> ObtenerDatosparalaConfiguracionPresupuestaria(DatosparaConfiguracionPresupuestariaConsolidadoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de Obtener  los datos necesarios para la configuracion presupuestaria");

        try
        {
            string query1 = @"
                            select distinct 
                                vcr.descripcion_centro_responsabilidad as DescripcionCentroResponsabilidad,
                                vcr.numero_programa+'-'+ vcr.descripcion_programa as Programa,  
                                vcr.numero_actividad +'-'+ vcr.descripcion_actividad as Actividad,  
                                vcr.numero_departamento as Departamento, 
                                vcr.numero_tipo_presupuesto as NumeroPresupuesto,  
                                vcr.descripcion_tipo_presupuesto as TipoPresupuesto,
                                vcr.codigo_centro_responsabilidad as CodigoCentroResponsabilidad,  
                                vcr.codigo_programa as CodigoPrograma,  
                                vcr.codigo_actividad as CodigoActividad,  
                                vcr.codigo_circunscripcion as CodigoCircunscripcion,  
                                vcr.codigo_departamento as CodigoDpto, 
                                vcr.codigo_tipo_presupuesto as CodigoTipoPresupuesto 

                                from    
                                vListaCentrosResponsabilidadPorCircunscripcion vcr  
                                where    
                                vcr.codigo_centro_responsabilidad = @codigoCentroResponsabilidad";

            string query2 = @"
                            select  
                                max(vlb.numero_grupo+'-'+vlb.descripcion_grupo)  as Grupo,  
                                max(vlb.numero_subgrupo+'-'+vlb.descripcion_subgrupo) as SubGrupo, 
                                vlb.codigo_grupo as CodigoGrupo,  
                                vlb.codigo_subgrupo as CodigoSubgrupo   

                                from 	vListaBienesPrioritarios vlb  
                                where vlb.codigo_objeto_gasto = @codigoOBG 
                                and vlb.ejercicio in (@ejercicio - 1,0)  

                                group by 
                                vlb.codigo_grupo, 
                                vlb.codigo_subgrupo";
     

            var parametros = new DynamicParameters();
            parametros.Add("@codigoCentroResponsabilidad", request.CodigoCentroResponsabilidad);
            parametros.Add("@codigoOBG", request.CodigoObjetoGasto);
            parametros.Add("@ejercicio", request.Ejercicio);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado1 = await connection.QueryFirstOrDefaultAsync<DatosparaConfiguracionPresupuestariaConsolidadoDTO>(query1, parametros);
                var resultado2 = await connection.QueryAsync<DatosparaConfiguracionPresupuestariaConsolidadoDTO>(query2, parametros);


                var resultadoFinal = new List<DatosparaConfiguracionPresupuestariaConsolidadoDTO>();
                foreach (var res in resultado2)
                {
                    var datos = new DatosparaConfiguracionPresupuestariaConsolidadoDTO
                    {
                        DescripcionCentroResponsabilidad = resultado1.DescripcionCentroResponsabilidad,
                        Programa = resultado1.Programa,
                        Actividad = resultado1.Actividad,
                        Departamento = resultado1.Departamento,
                        NumeroPresupuesto = resultado1.NumeroPresupuesto,
                        TipoPresupuesto = resultado1.TipoPresupuesto,
                        CodigoCentroResponsabilidad = resultado1.CodigoCentroResponsabilidad,
                        CodigoPrograma = resultado1.CodigoPrograma,
                        CodigoActividad = resultado1.CodigoActividad,
                        CodigoCircunscripcion = resultado1.CodigoCircunscripcion,
                        CodigoDpto = resultado1.CodigoDpto,
                        CodigoTipoPresupuesto = resultado1.CodigoTipoPresupuesto,
                        Grupo = res.Grupo,
                        SubGrupo = res.SubGrupo,
                        CodigoGrupo = res.CodigoGrupo,
                        CodigoSubgrupo = res.CodigoSubgrupo
                    };
                    resultadoFinal.Add(datos);
                }

                _logger.LogInformation("Fin de Proceso de Obtener  los datos necesarios para la configuracion presupuestaria");

                return resultadoFinal;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error inesperado al obtener los datos para la configuracion presupuestaria del consolidado" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<IEnumerable<BienesdeAnteproyectoBienesConsolidadoDTO>> ObtenerBienesdeAnteproyectoObjeto(BienesdeAnteproyectoBienesConsolidadoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de Obtener los datos de bienes de versiones anteproyecto bienes");

        try
        {
            string query = @"
                              SELECT
                                CODIGO_ANTEPROYECTO_BIEN as CodigoAnteproyectoBien,
                                NUMERO_BIEN as NumeroBien,
                                DESCRIPCION_BIEN as DescripcionBien,    
                                COALESCE(CONVERT(BIGINT, CANTIDAD), 0) as Cantidad,      
                                COALESCE(CONVERT(DECIMAL(18), VALOR_UNITARIO), 0) as ValorUnitario,
                                COALESCE(CONVERT(DECIMAL(18), VALOR_UNITARIO) * CONVERT(BIGINT, CANTIDAD), 0) AS MontoTotal,
                                UNIDAD_MEDIDA as UnidadMedida,
                                FUNDAMENTACION as Fundamentacion,
                                SELECCIONADO as Seleccionado

                            FROM VERSIONES_ANTEPROYECTOS_BIENES
                            WHERE CODIGO_VERSION = @codigoVersion
                            AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBG
                            GROUP BY
                             NUMERO_BIEN,DESCRIPCION_BIEN,CANTIDAD,VALOR_UNITARIO,UNIDAD_MEDIDA,
                             FUNDAMENTACION,SELECCIONADO,CODIGO_ANTEPROYECTO_BIEN";

            string querytotal = @" SELECT 
 	                            COALESCE(SUM(CONVERT(BIGINT, CANTIDAD)), 0) as CantidadTotal,  
	                            COALESCE(SUM(CONVERT(DECIMAL(18), VALOR_UNITARIO) * CONVERT(BIGINT, CANTIDAD)), 0) as MontoTotal

                            FROM VERSIONES_ANTEPROYECTOS_BIENES
                            WHERE CODIGO_VERSION = @codigoVersion
                            AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBG";

            var parametros = new DynamicParameters();
            parametros.Add("@codigoVersion", request.CodigoVersion);
            parametros.Add("@codigoAnteproyectoOBG", request.CodigoAnteproyectoObjeto);           

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryAsync<BienesdeAnteproyectoBienesConsolidadoDTO>(query, parametros);

                var total = await connection.QueryFirstOrDefaultAsync<Totales>(querytotal, parametros);

                if (total != null)
                {
                    foreach (var item in resultado)
                    {
                        item.Totales.CantidadTotal = total.CantidadTotal;
                        item.Totales.MontoTotal = total.MontoTotal;
                    }
                }
                _logger.LogInformation("Fin de Proceso de Obtener los datos de bienes de versiones anteproyecto bienes");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error inesperado al obtener los datos de bienes de anteproyecto bienes" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<Datos<IEnumerable<BienesparaConsolidadoDTO>>> ObtenerBienesparaConsolidado(BienesparaConsolidadoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de Obtener los datos de bienes para versiones anteproyecto bienes");
        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
        string filtrosAdicionales = string.Empty;

        try
        {
            if (!string.IsNullOrEmpty(request.CodigoCatalogo?.ToString()) ||              
               !string.IsNullOrEmpty(request.DescripcionCatalogo) ||
               !string.IsNullOrEmpty(request.TerminoDeBusqueda))
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    filtrosAdicionales += @"
                AND (
                    CONVERT(NVARCHAR, codigo_catalogo) LIKE '%' + @TerminoDeBusqueda + '%'                 
                    OR descripcion_catalogo LIKE '%' + @TerminoDeBusqueda + '%'
                )";
                }
                else
                {
                    filtrosAdicionales += @"
                AND (@codigoCatalogo IS NULL OR CONVERT(NVARCHAR, codigo_catalogo) LIKE '%' + @codigoCatalogo + '%')                                                            
                AND (@descripcionCatalogo IS NULL OR descripcion_catalogo LIKE '%' + @descripcionCatalogo + '%')";
                }
            }

            string query = $@"
                             select 
                                codigo_catalogo as CodigoCatalogo,
                                descripcion_catalogo as DescripcionCatalogo, 
                                valor_unitario as ValorUnitario, 
                                codigo_unidad_medida as CodigoUnidadMedida,
	                            descripcion_unidad_medida as UnidadMedida 

                            from vListaBienesPrioritarios vlb  
                            where vlb.ejercicio in (@ejercicio - 1,0) 
                            And codigo_objeto_gasto = @codigoOBG
                            {filtrosAdicionales}
                            ORDER BY codigo_catalogo DESC
                            OFFSET @saltarRegistros ROWS
                            FETCH NEXT @cantidadRegistros ROWS ONLY";

            var queryCantidadTotalRegistros = $@"
                            SELECT COUNT(*) AS TotalRegistros 

                            from vListaBienesPrioritarios vlb  
                            where vlb.ejercicio in (@ejercicio - 1,0) 
                            And codigo_objeto_gasto = @codigoOBG
                            {filtrosAdicionales}";


            var parametros = new DynamicParameters();
            parametros.Add("@ejercicio", request.Ejercicio);
            parametros.Add("@codigoOBG", request.CodigoObjetoGasto);
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);

            parametros.Add("@codigoCatalogo", request.CodigoCatalogo?.ToString());          
            parametros.Add("@descripcionCatalogo", request.DescripcionCatalogo);
            parametros.Add("@TerminoDeBusqueda", request.TerminoDeBusqueda);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QueryAsync<BienesparaConsolidadoDTO>(query, parametros);

                var response = new Datos<IEnumerable<BienesparaConsolidadoDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalRegistros
                };

                _logger.LogInformation("Fin de Proceso de Obtener los datos de bienes para versiones anteproyecto bienes");

                return response;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error inesperado al obtener los datos de bienes para anteproyecto bienes" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> InsertarConfiguracionPresupuestariaDesdeConsolidado(ConfiguracionPresupuestariaConsolidadoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de insertar la configuracion presupuestaria por Objeto de Gastos en el Consolidado");      
          
        //INSERTAMOS LA TABLA CONFIGURACION PRESUPUESTARIA
        string query = @"INSERT INTO CONFIGURACION_PRESUPUESTARIA
                     (CODIGO_CONFIGURACION_PRESUPUESTARIA, CODIGO_OBJETO_GASTO, CODIGO_PROGRAMA, CODIGO_ACTIVIDAD, CODIGO_TIPO_PRESUPUESTO, CODIGO_DEPARTAMENTO, GRUPO, CODIGO_CENTRO_RESPONSABILIDAD, CODIGO_MATERIA, SUBGRUPO,ejercicio)
                      VALUES (@codigoConfigPres, @codigoObjetoGasto, @codigoPrograma, @codigoActividad, @codigoTipoPresupuesto, @codigoDepartamento, @grupo, @codigoCentroResponsabilidad, @codigoMateria, @subGrupo,@ejercicio)";

        string query_UltimoValorCodigoConfigPresupuestaria = "SELECT ISNULL(MAX(CODIGO_CONFIGURACION_PRESUPUESTARIA), 0) FROM CONFIGURACION_PRESUPUESTARIA";      

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {              

                var parametros = new DynamicParameters();               

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
                parametros.Add("@ejercicio", request.Ejercicio);

                //insertamos la Configuracion
                var resultado = await connection.ExecuteAsync(query, parametros);                              

                _logger.LogInformation("Fin de Proceso de insertar la configuracion presupuestaria por Objeto de Gastos en el Consolidado");

                return codigoConfigPres;
            }
        }
        catch (Exception ex)
        {
            throw new GeneracionSolicitudesException("Ocurrió un error inesperado al insertar la Configuracion Presupuestaria por Objeto de Gastos del Consolidado" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<PresupuestoInicialyModificacionesConsolidadoDTO> ObtenerPresupuestoInicialyModificaciones(PresupuestoInicialyModificacionesConsolidadoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de Obtener los montos de Presupuesto Inicial y Modificaciones");
       
        try
        {           
            string query = $@"
                             select
                                COALESCE(SUM(CONVERT(DECIMAL(18), vao.presupuesto_inicial)),0) as PresupuestoInicial,
                                COALESCE(SUM(CONVERT(DECIMAL(18), vao.modificaciones)),0) as Modificaciones

                                from VERSIONES_ANTEPROYECTOS_OBJETOS vao 
                                join VERSIONES_ANTEPROYECTOS va ON 	va.CODIGO_VERSION=vao.CODIGO_VERSION 
                                join CONFIGURACION_PRESUPUESTARIA cp on cp.CODIGO_CONFIGURACION_PRESUPUESTARIA=vao.CODIGO_CONFIGURACION_PRESUPUESTARIA 

                                where va.EJERCICIO= (@ejercicio -1) 
                                and cp.CODIGO_OBJETO_GASTO = @codigoOBG 
                                and vao.CODIGO_FUENTE_FINANCIAMIENTO = @codigoFF
                                and vao.CODIGO_ORGANISMO_FINANCIADOR= @codigoOF
                                and cp.CODIGO_CENTRO_RESPONSABILIDAD= @codigoCentroResp
                                and cp.CODIGO_MATERIA = @codigioMateria";          


            var parametros = new DynamicParameters();
            parametros.Add("@ejercicio", request.Ejercicio);
            parametros.Add("@codigoOBG", request.CodigoObjetoGasto);
            parametros.Add("@codigoFF", request.CodigoFF);
            parametros.Add("@codigoOF", request.CodigoOF);
            parametros.Add("@codigoCentroResp", request.CodigoCentroResponsabilidad);
            parametros.Add("@codigioMateria", request.CodigoMateria);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QuerySingleOrDefaultAsync<PresupuestoInicialyModificacionesConsolidadoDTO>(query, parametros);               

                _logger.LogInformation("Inicio de Proceso de Obtener los montos de Presupuesto Inicial y Modificaciones");

                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error inesperado al obtener los los montos de Presupuesto Inicial y Modificaciones" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> ModificarAnteproyectoObjetoConsolidado(ModificarOBGConsolidadoRequest request)
    {
        _logger.LogInformation("Inicio del Proceso de modificar Objetos de version de anteproyecto objetos");

        string query = @"UPDATE VERSIONES_ANTEPROYECTOS_OBJETOS
                           SET CODIGO_FUENTE_FINANCIAMIENTO  = @ff,
                               CODIGO_ORGANISMO_FINANCIADOR = @of,
                               EVENTO = @evento,
                               PRESUPUESTO_INICIAL = @presupuestoInicial,
                               MODIFICACIONES = @modificaciones                              

                         WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoVersionAnteproyecto
                           AND CODIGO_VERSION = @codigoVersion
                           AND CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria";

        var parametros = new DynamicParameters();
        parametros.Add("@ff", request.CodigoFuenteFinanciamiento);
        parametros.Add("@of", request.CodigoOrganismoFinanciador);
        parametros.Add("@evento", request.Evento);
        parametros.Add("@presupuestoInicial", request.PresupuestoInicial);
        parametros.Add("@modificaciones", request.Modificaciones);      

        parametros.Add("@codigoVersionAnteproyecto", request.CodigoAnteProyectoObjeto);
        parametros.Add("@codigoVersion", request.CodigoVersion);
        parametros.Add("@codigoConfigPresupuestaria", request.ConfiguracionPresupuestaria);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de modificar Objetos de version de anteproyecto objetos");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error al modificar los datos en la tabla Versiones Anteproyecto Objetos" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> ModificarAnteproyectoBienesConsolidado(ModificarBienesConsolidadoRequest request)
    {
        _logger.LogInformation("Inicio del Proceso de modificar Bienes de version de anteproyecto Bienes");

        string query = @"UPDATE VERSIONES_ANTEPROYECTOS_BIENES
                           SET 
                              NUMERO_BIEN = @numeroBien,
                              DESCRIPCION_BIEN = @descripcionBien,
                              CANTIDAD = @cantidad,
                              VALOR_UNITARIO = @montoUnitario,      
                              USUARIO_MODIFICACION = @usuarioModif,
                              FECHA_MODIFICACION = GETDATE(),     
                              FUNDAMENTACION = @fundamentacion
                              
                         WHERE CODIGO_ANTEPROYECTO_BIEN = @codigoAnteproyectoBien 
                           AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBG
                           AND CODIGO_VERSION = @codigoVersion";

        var parametros = new DynamicParameters();
        parametros.Add("@numeroBien", request.NumeroBien);
        parametros.Add("@descripcionBien", request.DescripcionBien);
        parametros.Add("@cantidad", request.Cantidad);
        parametros.Add("@montoUnitario", request.ValorUnitario);
        parametros.Add("@fundamentacion", request.Fundamentacion);
        parametros.Add("@usuarioModif", request.CodigoUsuarioModificacion);

        parametros.Add("@codigoAnteproyectoOBG", request.CodigoAnteProyectoObjeto);
        parametros.Add("@codigoVersion", request.CodigoVersion);
        parametros.Add("@codigoAnteproyectoBien", request.CodigoAnteproyectoBien);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de modificar Bienes de version de anteproyecto Bienes");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error al modificar los datos en la tabla Versiones Anteproyecto Bienes" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> ValidarExistenciaenAnteproyectoObjetoConsolidado(ValidarExistenciaenAnteproyectoObjetoConsolidadoRequest request)
    {
        _logger.LogInformation("Inicio del Proceso de validar si existe el objeto de gasto para esa configuracion");

        string query = @"SELECT count(*)
                        FROM VERSIONES_ANTEPROYECTOS_OBJETOS vao
                        JOIN CONFIGURACION_PRESUPUESTARIA cnf ON 
                         vao.CODIGO_CONFIGURACION_PRESUPUESTARIA = cnf.CODIGO_CONFIGURACION_PRESUPUESTARIA
                        JOIN VERSIONES_ANTEPROYECTOS VA ON va.CODIGO_VERSION = vao.CODIGO_VERSION

                        WHERE cnf.CODIGO_OBJETO_GASTO = @codigoOBG
                        AND cnf.CODIGO_CENTRO_RESPONSABILIDAD = @codigoCentro
                        AND vao.CODIGO_FUENTE_FINANCIAMIENTO = @codigoFF
                        AND vao.CODIGO_ORGANISMO_FINANCIADOR = @codigoOF
                        AND va.CODIGO_VERSION = @codigoVersion
                        AND va.CODIGO_CIRCUNSCRIPCION = @codigoCircunscripcion";


        var codigoCircunscripcion = 0;
        if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("codCircunscripcion"))
        {
            var dbUser = _httpContextAccessor.HttpContext.Request.Headers["codCircunscripcion"].ToString().Trim();
            if (!string.IsNullOrEmpty(codigoCircunscripcion.ToString()))
            {
                codigoCircunscripcion = int.Parse(dbUser);               
            }
        }

        var parametros = new DynamicParameters();
        parametros.Add("@codigoOBG", request.CodigoOBG);
        parametros.Add("@codigoFF", request.CodigoFF);
        parametros.Add("@codigoOF", request.CodigoOF);
        parametros.Add("@codigoCentro", request.CodigoCentro);             
        parametros.Add("@codigoVersion", request.CodigoVersion);
        parametros.Add("@codigoCircunscripcion", codigoCircunscripcion);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryFirstOrDefaultAsync<int>(query, parametros);

                _logger.LogInformation("Fin del Proceso de validar si existe el objeto de gasto para esa configuracion");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error al validar existencia de Objeto Gasto para la Configuracion Presupuestaria en Consolidado" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> ObtenerMontoPlanificado(MontoPlanificadoRequest request)
    {
        _logger.LogInformation("Inicio del Proceso de obtener el monto planificado de versiones objeto consolidado");

        string query = @"SELECT 
                            COALESCE(SUM(CONVERT(DECIMAL(18), MONTO_PLANIFICADO)),0) as MontoPlanificado

                        FROM VERSIONES_ANTEPROYECTOS_OBJETOS                                     
                        WHERE CODIGO_VERSION = @codigoVersion
                        AND CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresu";

        var parametros = new DynamicParameters();
        parametros.Add("@codigoAnteproyectoOBG", request.CodigoAnteProyectoObjeto); 
        parametros.Add("@codigoVersion", request.CodigoVersion);
        parametros.Add("@codigoConfigPresu", request.CodigoConfiguracionPresupuestaria);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryFirstOrDefaultAsync<int>(query, parametros);

                _logger.LogInformation("Fin del Proceso de obtener el monto planificado de versiones objeto consolidado");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error al obtener el monto planificado de la version anteproyecto objeto en Consolidado" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> ObtenerEjercicioActivo()
    {
        _logger.LogInformation("Inicio del Proceso de obtener el ejercicio con el estado activo");

        string query = @"select coalesce(max(EJERCICIO),0) 
                            from VERSION_FECHA_CIERRE
                            WHERE ACTIVO = 1";
        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.QueryFirstOrDefaultAsync<int>(query);

                _logger.LogInformation("Fin del Proceso de obtener el ejercicio con el estado activo");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidarAnteproyectoException("Ocurrió un error al obtener el ejercicio con el estado activo de fecha cierre" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<Datos<IEnumerable<ObjetosGastosPendientesaConfigurarDTO>>> ValidarOBGPerteneceaConfiguracion(int codigoVersion)
    {
        _logger.LogInformation("Inicio del Proceso de validar si Objeto de Gasto y Bienes Pertenecen a una Configuracion");

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var parametros = new DynamicParameters();
                parametros.Add("@codigoVersion", codigoVersion);

                using (var multi = await connection.QueryMultipleAsync("ValidarOBGyBienesPendientesDeConfiguracion", parametros, commandType: CommandType.StoredProcedure))
                {

                    var objetosGasto = (await multi.ReadAsync<ObjetosGastosPendientesaConfigurarDTO>()).ToList();

                    var objetosGastoValidos = objetosGasto.Where(o =>
                     o.codigo_objeto_gasto != null ||
                     o.codigo_materia != null ||
                     o.codigo_centro_responsabilidad != null
                     ).ToList();

                    var totalPendientes = objetosGastoValidos.Count;

                    _logger.LogInformation("Fin del Proceso de validar si Objeto de Gasto y Bienes Pertenecen a una Configuracion");

                    return new Datos<IEnumerable<ObjetosGastosPendientesaConfigurarDTO>>
                    {
                        Items = objetosGasto,
                        TotalRegistros = totalPendientes
                    };
                }
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error al validar objeto y bienes si pertenecen a una configuracion presupuestaria" + "||-->" + ex.Message + "<--||");
        }
    }


}
