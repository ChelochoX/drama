using Application.Services.Interfaces.IRepository;
using Dapper;
using Domain.DTOs;
using Domain.Entities.Request;
using Domain.Entities.AsignacionContratos;
using Domain.Entities.Request.AsignacionContratos;
using Domain.Exceptions.AsignacionContratoPorObjetosGastos;
using Microsoft.Extensions.Logging;
using Domain.Exceptions.ImportarArchivoSIPOIExcepcions;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

namespace Persistence.Repositories
{
    public class AsignacionContratoPorObjetosGastosRepository : IAsignacionContratoPorObjetosGastosRepository
    {
        private readonly DbConnections _conexion;
        private readonly ILogger<AsignacionContratoPorObjetosGastosRepository> _logger;

        public AsignacionContratoPorObjetosGastosRepository(DbConnections conexion, ILogger<AsignacionContratoPorObjetosGastosRepository> logger)
        {
            _conexion = conexion;
            _logger = logger;
        }

        public async Task<Datos<IEnumerable<ObjetosdeGastoPorContratoAsignadoDTO>>> ObtenerObjetosdeGastoPorContrato(int codigoAnteproyectoObjeto, int codigoVersion)
        {
            _logger.LogInformation("Inicio de Proceso de obtener objetos de gasto por asignación de contrato.");

            try
            {
                string query = @"
                SELECT  
                    v.numero_og AS NumeroObjetoGasto,  
                    v.numero_ff AS NumeroFuenteFinanciamiento,  
                    v.numero_of AS NumeroOrganismoFinanciador, 
                    v.numero_dpto AS NumeroDepartamento,  
                    v.fundamentacion AS Fundamentacion,  
                    v.descripcion_centro_responsabilidad AS CentroResponsabilidad, 
                    v.descripcion_materia as DescripcionMateria, 
                    v.proyecto_presupuesto as MontoObjetoGasto,  
                    (v.proyecto_presupuesto-v.total_contrato) AS Saldo, 
                    v.total_contrato AS TotalContrato 
                FROM vlistaAnteproyectoPresupuestarioCircunscripcion v 
                WHERE v.codigo_version = @codigoVersion
                    AND v.codigo_anteproyecto_objeto = @codigoAnteproyectoObjeto";

                string queryCantidadTotalRegistros = @"SELECT COUNT(*) FROM (" + query + ") AS COUNT";

                var parametros = new DynamicParameters();
                parametros.Add("@codigoAnteproyectoObjeto", codigoAnteproyectoObjeto);
                parametros.Add("@codigoVersion", codigoVersion);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                    var resultado = await connection.QueryAsync<ObjetosdeGastoPorContratoAsignadoDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<ObjetosdeGastoPorContratoAsignadoDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalRegistros
                    };

                    _logger.LogInformation("Fin de Proceso de obtener listado de objetos de gasto por asignación de contrato.");

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al obtener el listado de objetos de gasto por asignación. Detalles: " + ex.Message);
            }
        }
        public async Task<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>> ObtenerContrato(string numeroContrato)
        {
            _logger.LogInformation("Inicio de Proceso de obtener datos del contrato.");

            var query = string.Empty;

            try
            {
                query = @"
                SELECT DISTINCT
                    c.numero_contratacion AS NumeroContrato,  
                    c.numero_pac AS PacPrepac,  
                    c.administrador_contrato AS AdministradorContrato, 
                    CASE  
                        WHEN c.plurianual = 'SI' THEN 'Continuo' 
                        WHEN c.plurianual = 'NO' THEN 'Nuevo' 
                    END AS TipoContrato, 
                    c.monto_total AS MontoContrato  
                FROM  
                    vContratos c 
                WHERE  
                c.numero_contratacion = @numeroContrato";

                //query = @"SELECT DISTINCT 
                //            v.codigo_contrato AS CodigoContrato, 
                //            v.numero_contrato AS NumeroContrato,
                //            v.numero_pac AS PacPrepac, 
                //            v.descripcion AS DescripcionContrato, 
                //            v.administrador_contrato AS AdministradorContrato,
                //            CASE  
                //                WHEN plurianual = 'SI' THEN 'Continuo'
                //                WHEN plurianual = 'NO' THEN 'Nuevo' 
                //            END AS TipoContrato,  
                //            v.monto_disponible AS MontoContrato 
                //            FROM   
                //                vContratos v 
                //            WHERE v.estado = 0 and v.vigencia_hasta <= GETDATE() and v.numero_contrato = @numeroContrato";

                var parametros = new DynamicParameters();
                parametros.Add("@numeroContrato", numeroContrato);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var resultado = await connection.QueryAsync<ContratoPorObjetosdeGastoDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = 1
                    };

                    _logger.LogInformation("Fin de Proceso de obtener listado de objetos de gasto por asignación de contrato.");

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al obtener los datos del contrato. Detalles: " + ex.Message);
            }
        }
        public async Task<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>> ObtenerContratoAsignado(ContratoAsignadoRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de obtener datos del contrato por asignación.");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;

            var query = string.Empty;

            try
            {
                query = @"
                SELECT DISTINCT
                    c.codigo_contrato AS CodigoContrato,
                    vc.codigo_version AS CodigoVersion,
                    vc.codigo_anteproyecto_objeto AS CodigoAnteproyectoObjeto,
                    c.numero_contratacion AS NumeroContrato,  
                    c.numero_pac AS PacPrepac,  
                    c.descripcion AS DescripcionContrato,  
                    c.administrador_contrato AS AdministradorContrato,  
                    CASE  
                        WHEN c.plurianual = 'si' THEN 'continuo' 
                        WHEN c.plurianual = 'no' THEN 'nuevo' 
                    END AS TipoContrato, 
                    vc.monto_contrato AS MontoContrato,
                    vc.codigo_anteproyecto_contrato as CodigoAnteproyectoContrato,
                    c.monto_total AS MontoContratoInicial
                FROM  
                    versiones_contratos vc 
                JOIN  
                    vcontratos c ON c.codigo_contrato = vc.codigo_contrato 
                WHERE  
                    codigo_version = @codigoVersion
                    AND codigo_anteproyecto_objeto = @codigoAnteproyectoObjeto";

                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                        !string.IsNullOrEmpty(request.NumeroContrato) ||
                        !string.IsNullOrEmpty(request.PacPrepac) ||
                        !string.IsNullOrEmpty(request.DescripcionContrato) ||
                        !string.IsNullOrEmpty(request.AdministradorContrato) ||
                        !string.IsNullOrEmpty(request.TipoContrato) ||
                        !string.IsNullOrEmpty(request.MontoContrato)
                        )
                {
                    if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                    {
                        query += @"
                                    AND (
                                        numero_contratacion LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR numero_pac LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR descripcion LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR administrador_contrato LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR monto_total LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR  vc.monto_contrato LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '')  + '%' 
	                                    OR CASE  
                                            WHEN plurianual = 'SI' THEN 'Continuo' 
                                            WHEN plurianual = 'NO' THEN 'Nuevo' 
                                        END LIKE '%' + @terminoDeBusqueda + '%' 

                                    )";
                    }
                    else
                    {
                        query += @"
                                    AND (
                                        (@numeroContrato IS NULL OR numero_contratacion LIKE '%' + @numeroContrato + '%')
	                                    AND (@pacPrepac IS NULL OR numero_pac LIKE '%' + @pacPrepac + '%')
                                        AND (@descripcionContrato IS NULL OR descripcion LIKE '%' + @descripcionContrato + '%')
	                                    AND (@administradorContrato IS NULL OR administrador_contrato LIKE '%' + @administradorContrato + '%')
                                        AND (@montoContrato IS NULL OR vc.monto_contrato LIKE '%' + REPLACE(@montoContrato, '.', '')  + '%')                                       
	                                    AND (@tipoContrato IS NULL OR (CASE  
                                            WHEN plurianual = 'SI' THEN 'Continuo' 
                                            WHEN plurianual = 'NO' THEN 'Nuevo' 
                                        END LIKE '%' + @tipoContrato + '%'))
                                    )";
                    }
                }

                string queryCantidadTotalRegistros = @"
                SELECT 
                    COUNT(*)
                FROM 
                    (" + query + @") TotalRegistros
                WHERE 1 = 1";

                query += @"
                    ORDER BY c.numero_contratacion
                    OFFSET @saltarRegistros ROWS
                    FETCH NEXT @cantidadRegistros ROWS ONLY";

                var parametros = new DynamicParameters();
                parametros.Add("@codigoAnteproyectoObjeto", request.CodigoAnteproyectoObjeto);
                parametros.Add("@codigoVersion", request.CodigoVersion);
                parametros.Add("@terminoDeBusqueda", $"%{request.TerminoDeBusqueda}%");
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.CantidadRegistros);

                parametros.Add("@numeroContrato", request.NumeroContrato);
                parametros.Add("@pacPrepac", request.PacPrepac);
                parametros.Add("@descripcionContrato", request.DescripcionContrato);
                parametros.Add("@administradorContrato", request.AdministradorContrato);
                parametros.Add("@montoContrato", request.MontoContrato);
                parametros.Add("@tipoContrato", request.TipoContrato);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                    var resultado = await connection.QueryAsync<ContratoPorObjetosdeGastoDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalRegistros
                    };

                    _logger.LogInformation("Fin de Proceso de obtener listado de objetos de gasto por asignación de contrato.");

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al obtener el listado de objetos de gasto por asignación. Detalles: " + ex.Message);
            }
        }
        public async Task<int> ObtenerCodigoUsuario(string cedula)
        {
            _logger.LogInformation("Inicio de Proceso de obtener el nombre del usuario con cedula {@cedula}", cedula);

            string queryUsuarioSesion = "SELECT codigo_usuario FROM vListaUsuariosPorCentrosResponsabilidad WHERE cedula_identidad LIKE '%' + @cedula + '%'";

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
                throw new AsignacionContratoPorObjetosGastosException("Ocurrio un error al obtener el codigo de usuario de la tabla usuarios_poder_judicial" + "||-->" + ex.Message + "<--|| cedula: " + cedula);
            }
        }
        public async Task<Datos<int>> InsertarContrato(ContratoPorObjetosdeGasto contrato)
        {
            _logger.LogInformation("Inicio de Proceso de insertar el contrato por objeto de gasto {@contratoPorObjetosdeGasto}", contrato);

            string queryUltimoValorCodigo = "SELECT ISNULL(MAX(codigo_anteproyecto_contrato),0) FROM versiones_contratos";

            string query = @"
                            INSERT INTO versiones_contratos
                            (codigo_anteproyecto_contrato, codigo_anteproyecto_objeto, codigo_version, codigo_contrato, monto_contrato, usuario_inserto, fecha_inserto)
                            VALUES
                            (@codigoAnteproyectoContrato, @codigoAnteproyectoObjeto, @codigoVersion, @codigoContrato, @montoContrato, @UsuarioInserto, GETDATE())";

            string validarExiste = @"SELECT COUNT(*) FROM versiones_contratos WHERE codigo_version = @codigoVersion AND codigo_anteproyecto_objeto = @codigoAnteproyectoObjeto AND codigo_contrato = @codigoContrato ";
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    int existe = await connection.ExecuteScalarAsync<int>(validarExiste, contrato);
                    int nuevoCodigoAnteproyecto = 0;
                    int totalRegistro = 0;

                    if (existe == 0)
                    {
                        int ultimoValorCodigo = await connection.ExecuteScalarAsync<int>(queryUltimoValorCodigo);
                        nuevoCodigoAnteproyecto = ultimoValorCodigo + 1;

                        contrato.CodigoAnteproyectoContrato = nuevoCodigoAnteproyecto;

                        var resultado = await connection.ExecuteAsync(query, contrato);
                        totalRegistro = 1;
                    }
                    else
                    {
                        totalRegistro = -1;
                    }
                    

                    var listado = new Datos<int>
                    {
                        Items = nuevoCodigoAnteproyecto,
                        TotalRegistros = totalRegistro
                    };

                    _logger.LogInformation("Fin de Proceso de insertar contrato {@contratoPorObjetosdeGasto}", contrato);

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al insertar el contrato. Detalles: " + ex.Message);
            }
        }
        public async Task<Datos<int>> ModificarContrato(ContratoporObjetosdeGastoRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de modificar el contrato por objeto de gasto {@contratoPorObjetosdeGasto}", request);

            try
            {
                string query = @"
                            UPDATE versiones_contratos
                            SET
                                monto_contrato = @montoContrato,
                                usuario_modificacion = @usuarioModificacion,
                                fecha_modificacion = GETDATE()
                            WHERE codigo_anteproyecto_contrato = @codigoAnteproyectoContrato";

                var parametros = new DynamicParameters();
                parametros.Add("@montoContrato", request.MontoContrato);
                parametros.Add("@usuarioModificacion", request.UsuarioModificacion);
                parametros.Add("@codigoAnteproyectoContrato", request.CodigoAnteproyectoContrato);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var resultado = await connection.ExecuteAsync(query, parametros);

                    var listado = new Datos<int>
                    {
                        Items = resultado,
                        TotalRegistros = 1
                    };

                    _logger.LogInformation("Fin de Proceso de modificar contrato {@contratoPorObjetosdeGasto}", request);

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al modificar el contrato. Detalles: " + ex.Message);
            }
        }
        public async Task<Datos<int>> EliminarContrato(int codigoAnteproyectoContrato, int codigoVersion, int codigoAnteproyectoObjeto)
        {
            _logger.LogInformation("Inicio de Proceso de eliminar el contrato de la lista");

            try
            {
                string query = @"
                                DELETE FROM versiones_contratos 
                                    WHERE codigo_anteproyecto_contrato = @codigoAnteproyectoContrato
                                    AND codigo_version = @codigoVersion
                                    AND codigo_anteproyecto_objeto = @codigoAnteproyectoObjeto";

                var parametros = new DynamicParameters();
                parametros.Add("@codigoAnteproyectoContrato", codigoAnteproyectoContrato);
                parametros.Add("@codigoVersion", codigoVersion);
                parametros.Add("@codigoAnteproyectoObjeto", codigoAnteproyectoObjeto);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    int filasAfectadas = await connection.ExecuteAsync(query, parametros);

                    var retorno = new Datos<int>
                    {
                        Items = filasAfectadas,
                        TotalRegistros = filasAfectadas
                    };

                    _logger.LogInformation("Fin de Proceso de eliminar el contrato de la lista");

                    return retorno;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al intentar eliminar el registro del contrato. Detalles: " + ex.Message);
            }
        }
        public async Task<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>> ListarContratos(ContratosRequest request)
        {
            _logger.LogInformation("Inicio del Proceso de obtener listado de Contratos");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;

           string query = @"SELECT DISTINCT 
                        v.codigo_contrato AS CodigoContrato, 
                        v.numero_contrato AS NumeroContrato,
                        v.numero_pac AS PacPrepac, 
                        v.descripcion AS DescripcionContrato, 
                        v.administrador_contrato AS AdministradorContrato,
                        CASE  
                            WHEN v.plurianual = 'SI' THEN 'Continuo'
                            WHEN v.plurianual = 'NO' THEN 'Nuevo' 
                        END AS TipoContrato,  
                        v.monto_disponible AS MontoContrato 
                        FROM   
                            vContratos v 
                        WHERE v.estado = 0 and v.vigencia_hasta <= GETDATE()";


            if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                        !string.IsNullOrEmpty(request.NumeroContratacion) ||
                        !string.IsNullOrEmpty(request.PacPrepac) ||
                        !string.IsNullOrEmpty(request.DescripcionContrato) ||
                        !string.IsNullOrEmpty(request.AdministradorContrato) ||
                        !string.IsNullOrEmpty(request.CodigoContrato) ||
                        !string.IsNullOrEmpty(request.MontoContrato) ||
                        !string.IsNullOrEmpty(request.NumeroContrato) ||
                        !string.IsNullOrEmpty(request.TipoContrato)
                        )
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    query += @"
                                    AND (
                                        v.numero_contratacion LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.numero_pac LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.descripcion LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.administrador_contrato LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.codigo_contrato LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.numero_contrato LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR v.monto_total LIKE '%' + REPLACE(@terminoDeBusqueda, '.', '')  + '%' 
	                                    OR CASE  
                                            WHEN v.plurianual = 'SI' THEN 'Continuo' 
                                            WHEN v.plurianual = 'NO' THEN 'Nuevo' 
                                        END LIKE '%' + @terminoDeBusqueda + '%' 
                                    )";
                }
                else
                {
                    query += @"
                                    AND (
                                        (@numeroContratacion IS NULL OR (v.numero_contratacion LIKE '%' + @numeroContratacion + '%'))
	                                    AND (@pacPrepac IS NULL OR (v.numero_pac LIKE '%' + @pacPrepac + '%'))
                                        AND (@descripcionContrato IS NULL OR (v.descripcion LIKE '%' + @descripcionContrato + '%'))
	                                    AND (@administradorContrato IS NULL OR (v.administrador_contrato LIKE '%' + @administradorContrato + '%'))
                                        AND (@montoContrato IS NULL OR (v.monto_total LIKE '%' + REPLACE(@montoContrato, '.', '') + '%'))    
                                        AND (@codigoContrato IS NULL OR (v.codigo_contrato LIKE '%' + @codigoContrato + '%'))
                                        AND (@numeroContrato IS NULL OR (v.numero_contrato LIKE '%' + @numeroContrato + '%'))
	                                    AND (@tipoContrato IS NULL OR (CASE  
                                            WHEN v.plurianual = 'SI' THEN 'Continuo' 
                                            WHEN v.plurianual = 'NO' THEN 'Nuevo' 
                                        END LIKE '%' + @tipoContrato + '%'))
                                    )";
                }
            }

            string queryCantidadTotalRegistros = @"
                SELECT 
                    COUNT(*)
                FROM 
                    (" + query + @") TotalRegistros
                WHERE 1 = 1";

            query += @"
                    ORDER BY v.numero_contrato
                    OFFSET @saltarRegistros ROWS
                    FETCH NEXT @cantidadRegistros ROWS ONLY";

            var parametros = new DynamicParameters();
            parametros.Add("@terminoDeBusqueda", $"%{request.TerminoDeBusqueda}%");
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);

            parametros.Add("@numeroContrato", request.NumeroContrato);
            parametros.Add("@pacPrepac", request.PacPrepac);
            parametros.Add("@descripcionContrato", request.DescripcionContrato);
            parametros.Add("@administradorContrato", request.AdministradorContrato);
            parametros.Add("@montoContrato", request.MontoContrato);
            parametros.Add("@tipoContrato", request.TipoContrato);
            parametros.Add("@numeroContrato", request.NumeroContrato);
            parametros.Add("@numeroContratacion", request.NumeroContratacion);
            parametros.Add("@codigoContrato", request.CodigoContrato);

            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {

                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                    var resultado = await connection.QueryAsync<ContratoPorObjetosdeGastoDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    _logger.LogInformation("Fin del Proceso de obtener listado de Contratos");

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al intentar obtener listado de Contratos. Detalles: " + ex.Message);
            }
        }

        public async Task<Datos<int>> RegistroFechaCierreResolucion(FechaCierreResolucion fecha)
        {
            _logger.LogInformation("Inicio de Proceso de Registro de Fecha de Cierre de Resolución");

            string queryUltimoValorCodigo = "SELECT ISNULL(MAX(CODIGO_FECHA_CIERRE),0) FROM VERSION_FECHA_CIERRE";

            string query = @"
                   INSERT INTO VERSION_FECHA_CIERRE
                   (CODIGO_FECHA_CIERRE, FECHA_CIERRE, ACTIVO, USUARIO_INSERTO, FECHA_INSERTO, EJERCICIO, DESCRIPCION, USUARIO_MODIFICACION, FECHA_MODIFICACION)
                   VALUES
                   (@CodigoFechaCierre, @FechaCierre, @Estado, @UsuarioInserto, GETDATE(), @Ejercicio, @Descripcion, @UsuarioModificacion, @FechaModificacion )";
              
            
            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    int ultimoValorCodigo = await connection.ExecuteScalarAsync<int>(queryUltimoValorCodigo);
                    int nuevoCodigoFecha = ultimoValorCodigo + 1;
                    string QueryUpdate = "Update VERSION_FECHA_CIERRE set ACTIVO=0, USUARIO_MODIFICACION=@UsuarioUltimaModificacion, " +
                        " FECHA_MODIFICACION= GETDATE()" +
                        " where CODIGO_FECHA_CIERRE= " + ultimoValorCodigo.ToString()+"";
                    fecha.CodigoFechaCierre = nuevoCodigoFecha;
                    fecha.Estado =1;
                    int usuario;
                    string queryUsuario = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad WHERE cedula_identidad =@UsuarioUltimaModificacion";
                    usuario = await connection.QueryFirstOrDefaultAsync<int>(queryUsuario, new { UsuarioUltimaModificacion = fecha.UsuarioInserto });
                    fecha.UsuarioInserto = usuario;

                    if (ultimoValorCodigo != 0)
                    {
                        await connection.ExecuteAsync(QueryUpdate, new { UsuarioUltimaModificacion = usuario });
                    }

                    var resultado = await connection.ExecuteAsync(query, fecha);
                    var listado = new Datos<int>
                    {
                        Items = nuevoCodigoFecha,
                        TotalRegistros = 1
                    };

                    _logger.LogInformation("Fin de Proceso de Registro de Fecha de Cierre de Resolución ", fecha);

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al insertar el contrato. Detalles: " + ex.Message);
            }
        }

        public async Task<Datos<IEnumerable<ListarFechaCierreResolucion>>> ListadoFechaCierreResolucion(ListarFechaCierreResolucionRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de Listado de Fecha de Cierre de Resolución");

            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;          

            string filtrosAdicionales = "";

            if (!string.IsNullOrEmpty(request.Descripcion) ||
                request.FechaCierre.HasValue ||
                request.Ejercicio.HasValue ||
                !string.IsNullOrEmpty(request.EstadoDescripcion) ||
                !string.IsNullOrEmpty(request.UsuarioModificacion) ||
                request.FechaModificacion.HasValue ||
                !string.IsNullOrEmpty(request.TerminodeBusqueda))
            {
                if (!string.IsNullOrEmpty(request.TerminodeBusqueda))
                {
                    filtrosAdicionales += @"
                        AND (
                            vf.Descripcion LIKE '%' + @terminoDeBusqueda + '%'
                            OR r.valor_alfanumerico LIKE @terminoDeBusqueda + '%'
                            OR v.username LIKE '%' + @terminoDeBusqueda + '%'
                            OR CONVERT(NVARCHAR(MAX), vf.Fecha_cierre, 126) LIKE '%' + @terminoDeBusqueda + '%'
                            OR CONVERT(NVARCHAR(MAX), vf.Ejercicio) LIKE '%' + @terminoDeBusqueda + '%'
                            OR CONVERT(NVARCHAR(MAX), vf.fecha_modificacion, 126) LIKE '%' + @terminoDeBusqueda + '%'
                        )";
                }
                else
                {
                    filtrosAdicionales += @"
                        AND (@descripcion IS NULL OR vf.Descripcion LIKE '%' + @descripcion + '%')
                        AND (@fechaCierre IS NULL OR FORMAT(vf.Fecha_cierre, 'yyyy-MM-dd') LIKE '%' + @fechaCierre + '%')
                        AND (@ejercicio IS NULL OR CONVERT(NVARCHAR(MAX), vf.Ejercicio) LIKE '%' + CONVERT(NVARCHAR(MAX), @ejercicio) + '%')
                        AND (@estadoDescripcion IS NULL OR r.valor_alfanumerico =  @estadoDescripcion)
                        AND (@usuarioModificacion IS NULL OR v.username LIKE '%' + @usuarioModificacion + '%')
                        AND (@fechaModificacion IS NULL OR CONVERT(NVARCHAR(MAX), vf.fecha_modificacion, 126) LIKE '%' + CONVERT(NVARCHAR(MAX), @fechaModificacion, 126) + '%')";
                }
            }

            string query = $@"
                    SELECT 
                        vf.Descripcion, 
                        vf.Fecha_cierre AS FechaCierre, 
                        vf.Ejercicio,
                        r.valor_alfanumerico AS EstadoDescripcion, 
                        v.username AS UsuarioModificacion,
                        vf.fecha_modificacion AS FechaModificacion
                    FROM 
                        VERSION_FECHA_CIERRE vf
                        JOIN referencias r ON r.valor_decimal = vf.ACTIVO
                        JOIN tipo_referencias tr ON tr.codigo_tipo_referencia = r.codigo_tipo_referencia
                        LEFT JOIN vListaUsuariosPorCentrosResponsabilidad v ON v.codigo_usuario = vf.usuario_modificacion 
                    WHERE 
                        tr.dominio_tipo_referencia = 'ESTADO_FECHA_CIERRE' 
                        {filtrosAdicionales}
                    ORDER BY 
                        vf.CODIGO_FECHA_CIERRE DESC  
                    OFFSET @saltarRegistros ROWS 
                    FETCH NEXT @cantidadRegistros ROWS ONLY";

            string queryCantidadTotalRegistros = $@"
                SELECT 
                    COUNT(*) as TotalRegistros
                FROM 
                    VERSION_FECHA_CIERRE vf
                    JOIN referencias r ON r.valor_decimal = vf.ACTIVO
                    JOIN tipo_referencias tr ON tr.codigo_tipo_referencia = r.codigo_tipo_referencia
                    LEFT JOIN vListaUsuariosPorCentrosResponsabilidad v ON v.codigo_usuario = vf.usuario_modificacion 
                WHERE 
                    tr.dominio_tipo_referencia = 'ESTADO_FECHA_CIERRE' 
                    {filtrosAdicionales}";

            try
            {

                // Función para convertir fechas a formato SQL
                string ConvertirFechaASQL(string fechaStr)
                {
                    DateTime fecha;
                    if (DateTime.TryParse(fechaStr, out fecha))
                    {
                        return fecha.ToString("yyyy-MM-dd");
                    }
                    return null;
                }
                
                var fechaCierreFormateada = request.FechaCierre?.ToString("yyyy-MM-dd");

                var fechaModificacionFormateada = request.FechaModificacion?.ToString("yyyy-MM-dd");
                


                // Convertir el término de búsqueda
                string terminoDeBusquedaSQL = ConvertirFechaASQL(request.TerminodeBusqueda?.Trim()) ?? request.TerminodeBusqueda?.Trim();

                var parametros = new DynamicParameters();
                parametros.Add("@saltarRegistros", saltarRegistros);
                parametros.Add("@cantidadRegistros", request.CantidadRegistros);
                parametros.Add("@descripcion", request.Descripcion?.Trim());
                parametros.Add("@fechaCierre", fechaCierreFormateada);
                parametros.Add("@ejercicio", request.Ejercicio);
                parametros.Add("@estadoDescripcion", request.EstadoDescripcion?.Trim());
                parametros.Add("@usuarioModificacion", request.UsuarioModificacion?.ToString());
                if (request.FechaModificacion != null)
                {
                    parametros.Add("@fechaModificacion", fechaModificacionFormateada);
                }
                else
                {
                    parametros.Add("@fechaModificacion", request.FechaModificacion);
                }
                //parametros.Add("@fechaModificacion", request.FechaModificacion);
                parametros.Add("@terminoDeBusqueda", terminoDeBusquedaSQL);

                using (var connection = this._conexion.CreateSqlConnection())
                {
                    var totalRegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);
                    var resultado = await connection.QueryAsync<ListarFechaCierreResolucion>(query, parametros);

                    var listado = new Datos<IEnumerable<ListarFechaCierreResolucion>>
                    {
                        Items = resultado,
                        TotalRegistros = totalRegistros
                    };

                    _logger.LogInformation("Fin de Proceso de Listado de Fecha de Cierre de Resolución");

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al listar las fechas de cierre de resolución. Detalles: " + ex.Message);
            }
        }

        //NOTIFICACIONES DE USUARIO

        public async Task<Datos<IEnumerable<ListadoNotificacionesUsuarioDTO>>> ObtenerListadoNotificacionesUsuario(ListadoNotificacionesUsuarioRequest request)
        {
            _logger.LogInformation("Inicio de Proceso de Listado de Fecha de Cierre de Resolución");
            int saltarRegistros = (request.Pagina - 1) * request.CantidadRegistros;

            string query = @"SELECT nc.tipo_notificacion AS TipoNotificacion,  nc.mensaje AS Mensaje,      
                                    FORMAT(nc.fecha_recepcion, 'dd/MM/yyyy HH:mm:ss') AS FechaNotificacion,
                                    ucr.username AS usuario,  c.nombre_circunscripcion AS CircunscripcionOrigen 
                                    FROM notificaciones_consultas nc 
                                    JOIN circunscripciones c  ON c.codigo_circunscripcion = nc.codigo_circunscripción_enviado
                                    JOIN vListaUsuariosPorCentrosResponsabilidad ucr ON ucr.codigo_usuario = nc.codigo_usuario_recepcion
                                    WHERE nc.codigo_usuario_recepcion= @p_codigo_usuario_logueado
                                    "; 


           string QueryUpdate = @"UPDATE notificaciones_consultas SET leido = 1 
                                WHERE leido = 0 AND codigo_usuario_recepcion = @p_codigo_usuario_logueado";



            if (!string.IsNullOrEmpty(request.TerminoDeBusqueda) ||
                        !string.IsNullOrEmpty(request.TipoNotificacion) ||
                        !string.IsNullOrEmpty(request.Mensaje) ||
                        !string.IsNullOrEmpty(request.FechaNotificacion)|| 
                        !string.IsNullOrEmpty(request.CircunscripcionOrigen)||
                        !string.IsNullOrEmpty(request.UserName)) 
                        
            {
                if (!string.IsNullOrEmpty(request.TerminoDeBusqueda))
                {
                    query += @"
                                    AND (
                                        nc.tipo_notificacion LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR nc.mensaje LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR FORMAT(nc.fecha_recepcion, 'dd/MM/yyyy HH:mm:ss') LIKE '%' + @terminoDeBusqueda + '%' 
                                        OR c.nombre_circunscripcion LIKE '%' + @terminoDeBusqueda + '%'                                          

                                    )";
                }
                else
                {
                    query += @"
                                    AND (
                                        nc.tipo_notificacion LIKE '%' + @TipoNotificacion + '%' 
                                        OR nc.mensaje LIKE '%' + @Mensaje + '%' 
                                        OR (@FechaNotificacion IS NULL OR CONVERT(VARCHAR, nc.fecha_recepcion, 120) LIKE '%' + @FechaNotificacion + '%' )
                                        OR c.nombre_circunscripcion LIKE '%' + @CircunscripcionOrigen + '%'  
                                        OR ucr.username LIKE '%' + @userName + '%'             
                                    )";
                }
            }

            string queryCantidadTotalRegistros = @"
                SELECT 
                    COUNT(*)
                FROM 
                    (" + query + @") TotalRegistros
                WHERE 1 = 1";

            query += @"
                    ORDER BY nc.codigo_notificaciones_enviadas DESC
                    OFFSET @saltarRegistros ROWS
                    FETCH NEXT @cantidadRegistros ROWS ONLY";

            var parametros = new DynamicParameters();
            parametros.Add("@terminoDeBusqueda", $"%{request.TerminoDeBusqueda}%");
            parametros.Add("@saltarRegistros", saltarRegistros);
            parametros.Add("@cantidadRegistros", request.CantidadRegistros);

            parametros.Add("@TipoNotificacion", request.TipoNotificacion);
            parametros.Add("@Mensaje", request.Mensaje);
            parametros.Add("@FechaNotificacion", request.FechaNotificacion);
            parametros.Add("@CircunscripcionOrigen", request.CircunscripcionOrigen);
            parametros.Add("@userName", request.UserName);


            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string queryUsuarioInt = @"SELECT codigo_usuario as CodigoUsuario 
                                       FROM vListaUsuariosPorCentrosResponsabilidad 
                                       WHERE cedula_identidad = @UsuarioUltimaModificacion";
                    int usuarioInt = await connection.QueryFirstOrDefaultAsync<int>(queryUsuarioInt, new { UsuarioUltimaModificacion = request.Usuario });

                    parametros.Add("@p_codigo_usuario_logueado", usuarioInt);
                    await connection.ExecuteAsync(QueryUpdate, new { p_codigo_usuario_logueado = usuarioInt });

                    var totalTegistros = await connection.ExecuteScalarAsync<int>(queryCantidadTotalRegistros, parametros);

                    var resultado = await connection.QueryAsync<ListadoNotificacionesUsuarioDTO>(query, parametros);

                    var listado = new Datos<IEnumerable<ListadoNotificacionesUsuarioDTO>>
                    {
                        Items = resultado,
                        TotalRegistros = totalTegistros
                    };

                    _logger.LogInformation("Fin del Proceso de obtener listado de Contratos");

                    return listado;
                }
            }
            catch (Exception ex)
            {
                throw new AsignacionContratoPorObjetosGastosException("Ocurrió un error al intentar obtener listado de Contratos. Detalles: " + ex.Message);
            }



        }

        public async Task<int> CantidadNotificacionesAbiertas(int usuario)
        {
            _logger.LogInformation("Inicio de Proceso de contar Notificaciones ");


            string Query = @"SELECT  COUNT(*) AS totalNoLeidas FROM notificaciones_consultas nc 
                               WHERE nc.leido = 0   AND nc.codigo_usuario_recepcion = @p_codigo_usuario_logueado GROUP BY nc.codigo_usuario_recepcion";


            try
            {
                using (var connection = this._conexion.CreateSqlConnection())
                {
                    string queryUsuarioInt = @"SELECT codigo_usuario as CodigoUsuario FROM vListaUsuariosPorCentrosResponsabilidad WHERE cedula_identidad =@UsuarioUltimaModificacion";

                    int usuarioInt = await connection.QueryFirstOrDefaultAsync<int>(queryUsuarioInt, new { UsuarioUltimaModificacion = usuario });
                   
                  


                    var resultado = await connection.QuerySingleOrDefaultAsync<int>(Query, new { p_codigo_usuario_logueado = usuarioInt });

                    _logger.LogInformation("Fin de Proceso de Obtener valores de Bienes de solicitudes_objetos_bienes_detalle");

                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new GeneracionSolicitudesException("Ocurrio un error al contar las notificaciones abiertas");
            }
        }

    }
}
