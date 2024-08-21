using Application.Services.Interfaces.IRepository;
using Dapper;
using Domain.Entities.ConsolidadoAnteproyectoPresupuesto;
using Domain.Entities.Request;
using Domain.Exceptions.AsignacionContratoPorObjetosGastos;
using Microsoft.Extensions.Logging;

namespace Persistence.Repositories;

public class ConsolidadoAnteproyectoPresupuestoRepository: IConsolidadoAnteproyectoPresupuestoRepository
{
    private readonly DbConnections _conexion;
    private readonly ILogger<ConsolidadoAnteproyectoPresupuestoRepository> _logger;

    public ConsolidadoAnteproyectoPresupuestoRepository(ILogger<ConsolidadoAnteproyectoPresupuestoRepository> logger, DbConnections conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<IEnumerable<ObjetoGastosConsolidadoAnteproyectoDTO>> ListarConsolidadoAnteproyectoporObjetoGasto(ObjetoGastosConsolidadoAnteproyectoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el listado de objetos de gastos de la consolidacion de anteproyecto");    

        try
        {         
            var query = $@"
                            SELECT  
                                v.ejercicio as Ejercicio, 
                                v.nombre_circunscripcion as NombreCircunscripcion, 
                                v.numero_og as NumeroOBG,  
                                v.numero_ff as NumeroFF,  
                                v.numero_of as NumeroOG, 
                                v.numero_dpto as NumeroDpto,  
                                v.fundamentacion as Fundamentacion,
                                v.codigo_configuracion_presupuestaria as CodigoConfigPresupuestaria,
                                COALESCE(SUM(CONVERT(DECIMAL(18), v.presupuesto_inicial)), 0) as PresupuestoInicial,  
                                COALESCE(SUM(CONVERT(DECIMAL(18), v.modificaciones)), 0) as Modificaciones,
                                COALESCE(SUM(CONVERT(DECIMAL(18), v.presupuesto_vigente)), 0) as PersupuestoVigente,
                                COALESCE(SUM(CONVERT(DECIMAL(18), v.proyecto_presupuesto)), 0) as ProyectoPresupuesto,
                                COALESCE(SUM(CONVERT(DECIMAL(18), v.proyecto_presupuesto) - CONVERT(DECIMAL(18), v.presupuesto_vigente)), 0) as Diferencia
                            FROM vlistaAnteproyectoPresupuestarioCircunscripcion v 
                            WHERE v.codigo_version = @codigoVersion  
                                AND v.codigo_objeto_gasto = @codigoOBG
                                AND v.CODIGO_FUENTE_FINANCIAMIENTO = @codigoFF
                                AND v.CODIGO_ORGANISMO_FINANCIADOR = @codigoOG
                                AND v.codigo_circunscripcion_origen = @codigoCircunscripcion                                
                            GROUP BY 
                                v.ejercicio,v.nombre_circunscripcion,
                                v.numero_og,v.numero_ff,
                                v.numero_of,v.numero_dpto,
                                v.fundamentacion,codigo_configuracion_presupuestaria";        

            // Definición de parámetros
            var parametros = new DynamicParameters();           
            parametros.Add("@codigoOBG", request.CodigoObjetoGasto);
            parametros.Add("@codigoFF", request.CodigoFF);
            parametros.Add("@codigoOG", request.CodigoOG);
            parametros.Add("@codigoVersion", request.CodigoVersion);
            parametros.Add("@codigoCircunscripcion", request.CodigoCircunscripcion);           
                       
            using (var connection = this._conexion.CreateSqlConnection())
            {               
                var resultado = await connection.QueryAsync<ObjetoGastosConsolidadoAnteproyectoDTO>(query, parametros);              

                _logger.LogInformation("Fin de Proceso de obtener el listado de objetos de gastos de la consolidacion de anteproyecto");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error inesperado al obtener el listado de objetos de gastos de la consolidacion de anteproyecto" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<Datos<IEnumerable<BienesdelConsolidadoAnteproyectoDTO>>> ListarBienesdelConsolidadoAnteproyectoPresupuestario(BienesdelConsolidadoAnteproyectoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el listado de bienes de la consolidacion de anteproyecto");

        int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;
        string filtrosAdicionales = string.Empty;

        try
        {
        if (!string.IsNullOrEmpty(request.DescripcionCentroResponsabilidad) ||
       !string.IsNullOrEmpty(request.DescripcionMateria) ||
       !string.IsNullOrEmpty(request.DescripcionBien) ||
       !string.IsNullOrEmpty(request.ValorUnitario?.ToString()) ||
       !string.IsNullOrEmpty(request.Cantidad?.ToString()) ||
       !string.IsNullOrEmpty(request.MontoTotal?.ToString()) ||
       !string.IsNullOrEmpty(request.TerminoDeBusqueda))
            {

                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    filtrosAdicionales += @"AND (crc.descripcion_centro_responsabilidad LIKE '%' + @TerminoDeBusqueda + '%'                       
                                OR crc.descripcion_materia LIKE '%' + @TerminoDeBusqueda + '%'
                                OR bp.numero_bien + ' - ' + bp.descripcion_bien LIKE '%' + @TerminoDeBusqueda + '%'
                                OR COALESCE(CONVERT(NVARCHAR, vab.VALOR_UNITARIO), '') LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%'
                                OR COALESCE(CONVERT(NVARCHAR, vab.CANTIDAD), '') LIKE '%' + @TerminoDeBusqueda + '%'
                                OR COALESCE(CONVERT(NVARCHAR, vab.VALOR_UNITARIO * vab.CANTIDAD), '') LIKE '%' + REPLACE(@TerminoDeBusqueda, '.', '') + '%')";
                }
                else
                {
                    filtrosAdicionales += @"AND (@DescripcionCentroResponsabilidad IS NULL OR crc.descripcion_centro_responsabilidad LIKE '%' + @DescripcionCentroResponsabilidad + '%')
                            AND (@DescripcionMateria IS NULL OR crc.descripcion_materia LIKE '%' + @DescripcionMateria + '%')
                            AND (@DescripcionBien IS NULL OR bp.numero_bien + ' - ' + bp.descripcion_bien LIKE '%' + @DescripcionBien + '%')
                            AND (@ValorUnitario IS NULL OR COALESCE(CONVERT(NVARCHAR, vab.VALOR_UNITARIO), '') LIKE '%' + REPLACE(@ValorUnitario, '.', '') + '%')
                            AND (@Cantidad IS NULL OR COALESCE(CONVERT(NVARCHAR, vab.CANTIDAD), '') LIKE '%' + @Cantidad + '%')
                            AND (@MontoTotal IS NULL OR COALESCE(CONVERT(NVARCHAR, vab.VALOR_UNITARIO * vab.CANTIDAD), '') LIKE '%' + REPLACE(@MontoTotal, '.', '') + '%')";
                }
            }

            var query = $@"
                           SELECT 
                            va.CODIGO_VERSION as CodigoVersion, 
                            vao.CODIGO_ANTEPROYECTO_OBJETO as CodigoAnteproyectoObjeto, 
                            vab.CODIGO_ANTEPROYECTO_BIEN CodigoAnteproyectoBien, 
                            crc.descripcion_centro_responsabilidad as DescripcionCentroResponsabilidad,
                            crc.descripcion_materia as DescripcionMateria, 
                            bp.numero_bien+ ' - ' + bp.descripcion_bien as DescripcionBien,  
                            COALESCE(CONVERT(BIGINT, vab.VALOR_UNITARIO), 0) as ValorUnitario,
                            COALESCE(CONVERT(BIGINT, vab.CANTIDAD), 0) AS Cantidad,     
                            COALESCE(CONVERT(BIGINT, vab.VALOR_UNITARIO) * CONVERT(BIGINT, vab.CANTIDAD), 0) AS MontoTotal

                            FROM VERSIONES_ANTEPROYECTOS_BIENES vab 
                            JOIN VERSIONES_ANTEPROYECTOS_OBJETOS vao on 
                            vao.CODIGO_ANTEPROYECTO_OBJETO = vab.CODIGO_ANTEPROYECTO_OBJETO 
                            and vao.CODIGO_VERSION = vab.CODIGO_VERSION 
                            JOIN VERSIONES_ANTEPROYECTOS va on 
                            va.CODIGO_VERSION = vao.CODIGO_VERSION 
                            JOIN CONFIGURACION_PRESUPUESTARIA cp on 
                            cp.CODIGO_CONFIGURACION_PRESUPUESTARIA = vao.CODIGO_CONFIGURACION_PRESUPUESTARIA 
                            JOIN vListaBienesPrioritarios bp on 
                            bp.codigo_catalogo = vab.NUMERO_BIEN 
                            and bp.ejercicio in (va.EJERCICIO-1,0)  
                            JOIN vListaCentrosResponsabilidadPorCircunscripcion crc on  
                            crc.codigo_centro_responsabilidad = cp.CODIGO_CENTRO_RESPONSABILIDAD 
                            and crc.codigo_materia = cp.CODIGO_MATERIA 

                            WHERE va.CODIGO_VERSION = @codigoVersion
                                and cp.CODIGO_OBJETO_GASTO = @codigoOBG
                                and vao.CODIGO_FUENTE_FINANCIAMIENTO = @codigoFF
                                and vao.CODIGO_ORGANISMO_FINANCIADOR = @codigoOG
                                and vao.codigo_circunscripcion_origen = @codigoCircunscripcion 
                                {filtrosAdicionales}

                            ORDER BY cp.CODIGO_CENTRO_RESPONSABILIDAD , cp.CODIGO_MATERIA                       
                            OFFSET @saltarRegistros ROWS
                            FETCH NEXT @cantidadRegistros ROWS ONLY";

            var queryCantidadTotalRegistros = $@"
                            SELECT 
                             COUNT(*) AS TotalRegistros 

                            FROM VERSIONES_ANTEPROYECTOS_BIENES vab 
                            JOIN VERSIONES_ANTEPROYECTOS_OBJETOS vao on 
                            vao.CODIGO_ANTEPROYECTO_OBJETO = vab.CODIGO_ANTEPROYECTO_OBJETO 
                            and vao.CODIGO_VERSION = vab.CODIGO_VERSION 
                            JOIN VERSIONES_ANTEPROYECTOS va on 
                            va.CODIGO_VERSION = vao.CODIGO_VERSION 
                            JOIN CONFIGURACION_PRESUPUESTARIA cp on 
                            cp.CODIGO_CONFIGURACION_PRESUPUESTARIA = vao.CODIGO_CONFIGURACION_PRESUPUESTARIA 
                            JOIN vListaBienesPrioritarios bp on 
                            bp.codigo_catalogo = vab.NUMERO_BIEN 
                            and bp.ejercicio in (va.EJERCICIO - 1,0)  
                            JOIN vListaCentrosResponsabilidadPorCircunscripcion crc on  
                            crc.codigo_centro_responsabilidad = cp.CODIGO_CENTRO_RESPONSABILIDAD 
                            and crc.codigo_materia = cp.CODIGO_MATERIA 

                            WHERE va.CODIGO_VERSION = @codigoVersion
                            and cp.CODIGO_OBJETO_GASTO = @codigoOBG
                            and vao.CODIGO_FUENTE_FINANCIAMIENTO = @codigoFF
                            and vao.CODIGO_ORGANISMO_FINANCIADOR = @codigoOG
                            and vao.codigo_circunscripcion_origen = @codigoCircunscripcion
                            {filtrosAdicionales}";


            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);
            parametros.Add("@codigoOBG", request.CodigoObjetoGasto);
            parametros.Add("@codigoFF", request.CodigoFF);
            parametros.Add("@codigoOG", request.CodigoOG);
            parametros.Add("@codigoVersion", request.CodigoVersion);
            parametros.Add("@codigoCircunscripcion", request.CodigoCircunscripcion);
            parametros.Add("@TerminoDeBusqueda", request.TerminoDeBusqueda);
            parametros.Add("@DescripcionCentroResponsabilidad", request.DescripcionCentroResponsabilidad);
            parametros.Add("@DescripcionMateria", request.DescripcionMateria);
            parametros.Add("@DescripcionBien", request.DescripcionBien);
            parametros.Add("@ValorUnitario", request.ValorUnitario);
            parametros.Add("@Cantidad", request.Cantidad.HasValue ? request.Cantidad.Value.ToString() : null);
            parametros.Add("@MontoTotal", request.MontoTotal);

            using (var connection = this._conexion.CreateSqlConnection())
            {
                var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                var resultado = await connection.QueryAsync<BienesdelConsolidadoAnteproyectoDTO>(query, parametros);

                var response = new Datos<IEnumerable<BienesdelConsolidadoAnteproyectoDTO>>
                {
                    Items = resultado,
                    TotalRegistros = totalRegistros
                };

                _logger.LogInformation("Fin de Proceso de obtener el listado de bienes de la consolidacion de anteproyecto");
                return response;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error inesperado al obtener el listado de bienes de la consolidacion de anteproyecto" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> ModificarBiendeVersionAnteproyecto(ModificarBienesVersionAnteproyectoRequest request, int usuarioModificacion)
    {
        _logger.LogInformation("Inicio del Proceso de modificar la version de anteproyecto bienes");

        string query = @"UPDATE VERSIONES_ANTEPROYECTOS_BIENES
                           SET      
                              CANTIDAD = @cantidad,
                              VALOR_UNITARIO = @valorunitario,    
                              USUARIO_MODIFICACION = @usuarioModif,
                              FECHA_MODIFICACION = GETDATE()      

                         WHERE CODIGO_ANTEPROYECTO_BIEN = @codigoAnteproyectoBien
                               AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ
                               AND CODIGO_VERSION = @codigoVersion";

        var parametros = new DynamicParameters();
        parametros.Add("@cantidad", request.Cantidad);
        parametros.Add("@valorunitario",request.ValorUnitario);
        parametros.Add("@usuarioModif", usuarioModificacion);

        parametros.Add("@codigoAnteproyectoBien", request.CodigoAnteproyectoBien);
        parametros.Add("@codigoAnteproyectoOBJ", request.CodigoAnteproyectoObjeto);
        parametros.Add("@codigoVersion", request.CodigoVersion);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de modificar la version de anteproyecto bienes");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error al modificar los datos en la tabla Versiones Anteproyecto Bienes" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> EliminarBiendeVersionAnteproyecto(EliminarBienVersionAnteproyectoRequest request)
    {
        _logger.LogInformation("Inicio del Proceso de eliminar la version de anteproyecto bienes");

        string queryBien = @"DELETE FROM VERSIONES_ANTEPROYECTOS_BIENES                          

                         WHERE CODIGO_ANTEPROYECTO_BIEN = @codigoAnteproyectoBien
                               AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ
                               AND CODIGO_VERSION = @codigoVersion";

        string estaVacio = @"SELECT Count(*) FROM VERSIONES_ANTEPROYECTOS_BIENES 
                                WHERE CODIGO_VERSION = @codigoVersion 
                                AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ";

        string queryObjeto = @"DELETE FROM VERSIONES_ANTEPROYECTOS_OBJETOS                 
                                 WHERE CODIGO_VERSION = @codigoVersion
                                 AND CODIGO_CONFIGURACION_PRESUPUESTARIA = @codigoConfigPresupuestaria
                                 AND CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ";

        string queryEliminarVERSIONES_CONTRATOS = @" DELETE FROM VERSIONES_CONTRATOS 
                                                        where CODIGO_VERSION = @codigoVersion 
                                                        and CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ";

        string queryEliminarVERSIONES_ANTEPROYECTO_PLANIFICACION = @" DELETE FROM VERSIONES_ANTEPROYECTO_PLANIFICACION 
                                                    where CODIGO_VERSION = @codigoVersion 
                                                    and CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ";

        var parametros = new DynamicParameters();

        parametros.Add("@codigoAnteproyectoBien", request.CodigoAnteproyectoBien);
        parametros.Add("@codigoAnteproyectoOBJ", request.CodigoAnteproyectoObjeto);
        parametros.Add("@codigoVersion", request.CodigoVersion);
        parametros.Add("@codigoConfigPresupuestaria", request.CodigoConfiguracionPresupuestaria);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                //Primeramente Eliminamos el Bien
                var resultado = await connection.ExecuteAsync(queryBien, parametros);

                //Eliminar Contrato
                var eliminarContrato = await connection.ExecuteAsync(queryEliminarVERSIONES_CONTRATOS, parametros);

                //Eliminar Planificacion
                var eliminarPlanificacion = await connection.ExecuteAsync(queryEliminarVERSIONES_ANTEPROYECTO_PLANIFICACION, parametros);

                //Despues que Eliminamos el Bien vemos la cantidad de REgistros que tenemos en Versiones Objetos
                int cantidadEnVersionesObjeto = await connection.ExecuteScalarAsync<int>(estaVacio, parametros);

                if (cantidadEnVersionesObjeto == 0)
                {
                    var resultadoObjeto = await connection.ExecuteAsync(queryObjeto, parametros);
                }              

                _logger.LogInformation("Fin del Proceso de eliminar la version de anteproyecto bienes");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error al eliminar los datos en la tabla Versiones Anteproyecto Bienes" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<IEnumerable<ObjetodeGastoVersionAnteproyectoDTO>> ListarObjetosdeGastosdeVersionesAnteproyectos(ObjetodeGastoVersionAnteproyectoRequest request)
    {
        _logger.LogInformation("Inicio de Proceso de obtener el listado de Objeto deGasto de Versiones Anteproyecto");     

        try
        {
            var query = $@"
                           SELECT  
                                v.CODIGO_ANTEPROYECTO_OBJETO as CodigoAnteproyectoObjeto, 
                                v.CODIGO_VERSION as CodigoVersion 

                            FROM vlistaAnteproyectoPresupuestarioCircunscripcion v 

                            WHERE  
                                v.codigo_version = @codigoVersion AND 
                                v.version_consolidado = @versionConsolidado AND 
                                v.CODIGO_OBJETO_GASTO = @codigoOBG AND 
                                v.CODIGO_FUENTE_FINANCIAMIENTO = @codigoFF AND 
                                v.CODIGO_ORGANISMO_FINANCIADOR = @codigoOG AND 
                                v.codigo_circunscripcion_origen = @codigoCircunscripcion";        

            // Definición de parámetros
            var parametros = new DynamicParameters();
            parametros.Add("@codigoVersion", request.CodigoVersion);
            parametros.Add("@versionConsolidado", request.VersionConsolidado);
            parametros.Add("@codigoOBG", request.CodigoObjetoGasto);
            parametros.Add("@codigoFF", request.CodigoFF);
            parametros.Add("@codigoOG", request.CodigoOG);          
            parametros.Add("@codigoCircunscripcion", request.CodigoCircunscripcion);

            using (var connection = this._conexion.CreateSqlConnection())
            {                

                var resultado = await connection.QueryAsync<ObjetodeGastoVersionAnteproyectoDTO>(query, parametros);            

                _logger.LogInformation("Fin de Proceso de obtener el listado de Objeto deGasto de Versiones Anteproyecto");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error inesperado al obtener el listado de objeto de gasto de versiones anteproyecto" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> EliminarBiendeVersionAnteproyectoporOBGVersiones(ObjetodeGastoVersionAnteproyectoDTO request)
    {
        _logger.LogInformation("Inicio del Proceso de eliminar el bien de la version de anteproyecto bienes");

        string query = @"DELETE FROM VERSIONES_ANTEPROYECTOS_BIENES                          

                         WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ
                               AND CODIGO_VERSION = @codigoVersion";

        var parametros = new DynamicParameters();
       
        parametros.Add("@codigoAnteproyectoOBJ", request.CodigoAnteproyectoObjeto);
        parametros.Add("@codigoVersion", request.CodigoVersion);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de eliminar el bien de la version de anteproyecto bienes");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error al eliminar los datos en la tabla Versiones Anteproyecto Bienes" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> EliminardeVersionesContratoporOBGVersiones(ObjetodeGastoVersionAnteproyectoDTO request)
    {
        _logger.LogInformation("Inicio del Proceso de eliminar registros de version de contratos");

        string query = @"DELETE FROM VERSIONES_CONTRATOS                     
                         WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ
                               AND CODIGO_VERSION = @codigoVersion";

        var parametros = new DynamicParameters();

        parametros.Add("@codigoAnteproyectoOBJ", request.CodigoAnteproyectoObjeto);
        parametros.Add("@codigoVersion", request.CodigoVersion);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de eliminar registros de version de contratos");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error al eliminar los datos en la tabla Versiones Contrato" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> EliminardeVersionesPlanificacionporOBGVersiones(ObjetodeGastoVersionAnteproyectoDTO request)
    {
        _logger.LogInformation("Inicio del Proceso de eliminar registros de version de anteproyecto planificacion");

        string query = @"DELETE FROM VERSIONES_ANTEPROYECTO_PLANIFICACION                     
                         WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ
                               AND CODIGO_VERSION = @codigoVersion";

        var parametros = new DynamicParameters();

        parametros.Add("@codigoAnteproyectoOBJ", request.CodigoAnteproyectoObjeto);
        parametros.Add("@codigoVersion", request.CodigoVersion);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de eliminar registros de version de anteproyecto planificacion");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error al eliminar los datos en la tabla Versiones anteproyecto Planificacion" + "||-->" + ex.Message + "<--||");
        }
    }
    public async Task<int> EliminarObjetodeVersionesAnteproyectoporOBGVersiones(ObjetodeGastoVersionAnteproyectoDTO request)
    {
        _logger.LogInformation("Inicio del Proceso de eliminar registros de version de anteproyecto objeto");

        string query = @"DELETE FROM VERSIONES_ANTEPROYECTOS_OBJETOS                     
                         WHERE CODIGO_ANTEPROYECTO_OBJETO = @codigoAnteproyectoOBJ
                               AND CODIGO_VERSION = @codigoVersion";

        var parametros = new DynamicParameters();

        parametros.Add("@codigoAnteproyectoOBJ", request.CodigoAnteproyectoObjeto);
        parametros.Add("@codigoVersion", request.CodigoVersion);

        try
        {
            using (var connection = this._conexion.CreateSqlConnection())
            {
                var resultado = await connection.ExecuteAsync(query, parametros);

                _logger.LogInformation("Fin del Proceso de eliminar registros de version de anteproyecto objeto");
                return resultado;
            }
        }
        catch (Exception ex)
        {
            throw new ConsolidadoAnteproyectodePresupuestosException("Ocurrió un error al eliminar los datos en la tabla Versiones anteproyecto Objeto" + "||-->" + ex.Message + "<--||");
        }
    }

}
