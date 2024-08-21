using DocumentFormat.OpenXml.Drawing.Charts;
using Domain.Entities;
using Domain.Entities.Anteproyecto;
using Domain.Entities.Referencias;
using Domain.Entities.Request;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Interfaces;

public interface ISolicitudObjetosBienesService
{

    

    #region DESARROLLO DE LA INCIDENCIA CSJ-152
    Task<Datos<IEnumerable<VersionAnteproyectoDTO>>> VersionAnteproyectoRequest(VersionAnteproyectoRequest request);
    Task<Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>> AnteproyectoPresupuestarioRequest(AnteproyectoPresupuestarioRequest request);
    Task<CantidadTotalDiasDTO> CantidadTotalDias();
    Task<int> ActualizarEstadoVersionAnteproyecto(ActualizarEstadoVersionAnteproyectoRequest version);
    Task<int> CerrarEstadoVersionAnteproyecto(ActualizarEstadoVersionAnteproyectoRequest version); 
    Task<int> ActualizarVersionAnteproyectoObjeto(ActualizarVersionAnteproyectoObjetoRequest version);

    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-154
    Task<Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>> AnteproyectoPresupuestarioVerificacionesRequest(AnteproyectoPresupuestarioRequest request);
    Task<Datos<IEnumerable<AnteproyectoPresupuestarioCABDTO>>> AnteproyectoPresupuestarioCAB(int pagina, int cantidadRegistros, string? terminoDeBusqueda, int? numeroVersion, int? ejercicio, string? estado);
    Task<int> AbrirVersionAnteproyecto(int version, string user);
    Task<int> EliminarObjetodeAnteproyectoyBienes(int version, int codigoVersionAnteproyecto, int codigoConfigPresupuestaria);
    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-158
    Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCentroResponsabilidad(string user);

    Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> ObtenerCentroResponsabilidadReporte(int cedula); 
    Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGDependenciaCircuscripcion(string user);
    
        Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGMateria(string user);
    Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarCircuscripcion(string user); 
    Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGDepartamento(string user);
    Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOG(int ejercicio);
    Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCodigoOrganismoFinanciador();
    Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCodigoFuenteFinanciamiento();
    Task<Datos<IEnumerable<ClaseCentroResponsabilidadDTO>>> DatosOGCentroResponsabilidad(int codigo);
    Task<Datos<IEnumerable<ClaseDatosOGGrupoSubGrupoDTO>>> DatosOGGrupoSubGrupo(int codigo, int ejercicio);
    Task<Datos<IEnumerable<ClaseDatosOGBienesprioritariosDTO>>> DatosOGBienesprioritarios(int version, int codigo);
    Task<Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>> BuscarBien(int codigoObjetoGasto, int ejercicio, int pagina, int cantidadRegistros, string MontoUnitario, string TerminoDeBusqueda, string catalogo, string descripcionCatalogo);
    Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> BuscarBienesPrioritarios(int codigo);
    Task<Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>> BuscarBienesGrilla(int version, int codigoAnteproyectoObjeto);
    Task<int> EliminarBienAnteproyecto(int codigoAnteProyectoBien, int codigoObjetoGasto);
    Task<CantidadTotalGenericaDTO> DatosOGBienesMonto(int version, int codigo);
    Task<ClaseLlavePrimaria> AgregarConfiguracionPresupuestaria(ClaseConfiguracionPresupuestaria configuracion);
    Task<Datos<IEnumerable<VersionesAnteproyectosDTO>>>ObtenerVersiones();   
    Task<int> ObtenerVersionAbierta(); 

    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-ARCHIVOS
    Task<string> PersistirArchivo(int codigoVersion, int usuarioInserto, string dominio, string referencia, string nombre, string extension);
    Task<ClaseBajarArchivo> BajarArchivo(int codigoVersion, string usuarioInserto, string dominio);
    
    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-312

    Task<int> SincronizarDBCapital(int codigoVersion, int codigoCircunscripcion, string usuarioEjecucion, int codigoTarea);

    #endregion
}
