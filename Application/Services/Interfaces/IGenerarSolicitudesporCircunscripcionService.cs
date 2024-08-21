using Domain.Entities.Anteproyecto;
using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.GenerarSolicitudporCircunscripcion;
using Domain.Entities.Request;
using Domain.Entities.VerificacionSolicitudes;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Interfaces;

public interface IGenerarSolicitudesporCircunscripcionService
{  

    Task<Datos<IEnumerable<SolicitudCircunscripcionesEstadoAbierto>>> ListarSolicitudesconEstadosAbiertos(SolicitudBienesCircunscripcionRequest request);
    Task<CentroResponsabilidadyMateria> ObtenerCentrodeResponsabilidadyMateriaJuridica(string usuarioLogeado);
    Task<UsuarioCircunscripcion> ObtenerCentroResponsabilidadporUsuario(UsuarioCircunscipcionRequest2 request);
    Task<UsuarioCircunscripcion> ObtenerMateriaporUsuarioyCentroResponsabilidad(UsuarioCircunscipcionRequest2 request);
    Task<Datos<IEnumerable<ObjetodeGastosDTO>>> ObtenerObjetosGasto(ObjetosdeGastosRequest request);
    Task<Datos<IEnumerable<DetalleObjetoGastoporNroSolicitudDTO>>> ListarObjetodeGastosporCodigoSolicitud(ListaObjetoGastosdeSolicitudRequest request);
    Task<Datos<IEnumerable<SolicitudesCabeceraInsertResponse>>> InsertarCabeceraSolicitudCircunscripcion(SolicitudCabeceraporCircunscripcionRequest request);
    Task<Datos<int>> InsertarObjetosdeGastosPorNrodeSolicitud(ObjetosdeGastospornroSolicitudRequest request);
    Task<int> ModificarSolicitudBienCircunscripcion(ModificarSolicitudRequest request);
    Task<int> ActualizarEstadoSolicitudesysusObjetosGastos(ActualizarEstadoSolicitudesRequest request);
    Task<int> ActualizarEstadoObjetodeGasto(int codigoSolicitud, int codigoObjetoGasto, int estado);
    Task<Datos<int>> EliminarObjetodeGasto(int codigoSolicitudObjeto, int codigoSolicitud);
    Task<Datos<ObjetosdeGastosyBienesporNroSolicitud>> VisualizarObjetosdeGastosyBienesporNroSolicitud(int codigoSolicitud, int pagina, int cantidadRegistros, int codigoSolicitudObjeto, int? estado);
    Task<(List<RegistrodesdeExcel> registros, List<string> errores, List<string> erroresValidacion)> ImportarDocumento(IFormFile archivo, string ciUsuarioSesion);
    Task<Datos<IEnumerable<VerificacionSolicitudes>>> ListadoVerificacionSolicitudes(VerificacionSolicitudesRequest request);
    Task<int> ValidarVersionAbierta(int ejercicio);


    #region DESARROLLO DE LA INCIDENCIA CSJ-138 
    Task<string> Bienprocesado(string CodigoBienDetalle, int CodigoSolicitud, int CodigoSolicitudObjeto, bool eliminar);
    Task<int> EliminarSolicitudObjetosBienesDetalle(int codigoBienDetalle, int codigoSolicitud, int codigoObjetoGasto, string descripcionBienesNacional, int ejercicio);
    Task<List<SolicitudObjetoBienesDetalleDTO>> ObtenerDatosSolicitudObjetoBienesDetallePorCodigoSolicitud(int codigoSolicitud, int pagina, int cantidadRegistros);
    Task<List<SolicitudObjetoBienesDetalleDTO>> ListadoBienesPorCriterioBusqueda(int ejercicio, int codigoSolicitud, int codigoSolicitudObjeto, int codigo, string descripcion, int pagina, int cantidadRegistros);
    Task<Datos<IEnumerable<SolicitudObjetoBienesDetalleDTO>>> ListadoBienesPorCriterioBusquedaPorFiltros(SolicitudObjetoBienesDetalleRequest request);

    // Task<List<SolicitudObjetoCodigoDescripcionDTO>> ListadoBienesPorCodigoDescripcion(double costoUnitario, int ejercicio, int numeroCatalogo, int codigoObjetoGasto, int codigoSolicitudGasto, string descripcionSolicitudObjetoBienesDetalle, int pagina, int cantidadRegistros);
    Task<Datos<IEnumerable<SolicitudObjetoCodigoDescripcionDTO>>> ListadoBienesPorCodigoDescripcion(SolicitudObjetoBienesDetalleRequest request);


    Task<int> CantidadRegistrosBienes(double costoUnitario, int ejercicio, int codigoSolicitud, int codigoSolicitudObjeto, int codigo, string descripcionSolicitudObjetoBienesDetalle);

    Task InsertarSolicitudObjetosBienesDetalle(SolicitudObjetoBienesDetalle solicitudObjetoBienesDetalle);
    Task<int> CrearSolicitudObjetosBienesDetalle(SolicitudObjetoBienesDetalleDTO solicitudObjetoBienesDetalle);
    Task<int> ActualizarSolicitudObjetoBienesDetalle(SolicitudObjetoBienesDetalleDTO solicitudObjetoBienesDetalle);

    Task<CantidadTotalGenericaDTO> TotalesCatidadBienesSolicitud(int codigoSolicitud, int codigoSolicitudObjeto);
    Task<int> EjercicioSolicitud();


    #endregion


}

