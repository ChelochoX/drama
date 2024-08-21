using Domain.Entities.Anteproyecto;
using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.GenerarSolicitudporCircunscripcion;
using Domain.Entities.Request;
using Domain.Entities.VerificacionSolicitudes;
namespace Application.Services.Interfaces.IRepository;

public interface IGenerarSolicitudporCircunscripcionRepository
{
    
    Task<Datos<IEnumerable<SolicitudCircunscripcionesEstadoAbierto>>> ListarSolicitudesconEstadosAbiertos(SolicitudBienesCircunscripcionRequest request);
    Task<CentroResponsabilidadyMateria> ObtenerCentrodeResponsabilidadyMateriaJuridica(string usuarioLogeado);
    Task<UsuarioCircunscripcion> ObtenerCentroResponsabilidadporUsuario(UsuarioCircunscipcionRequest2 request);
    Task<UsuarioCircunscripcion> ObtenerMateriaporUsuarioyCentroResponsabilidad(UsuarioCircunscipcionRequest2 request);

    Task<Datos<IEnumerable<ObjetodeGastosDTO>>> ObtenerObjetosGasto(ObjetosdeGastosRequest request);
    Task<Datos<IEnumerable<DetalleObjetoGastoporNroSolicitudDTO>>> ListarObjetodeGastosporCodigoSolicitud(ListaObjetoGastosdeSolicitudRequest request);
    Task<Datos<IEnumerable<SolicitudesCabeceraInsertResponse>>> InsertarSolicitudBienesCircunscripcion(SolicitudBienesCircunscripcion solicitudBienes);
    Task<Datos<int>> InsertarSolicitudObjetosDetalle(SolicitudObjetoDetalle solicitudObjetoDetalle);
    Task<int> ObtenerCodigoUsuario(string cedula);
    Task InsertarSolicitudObjetosBienesDetalle(SolicitudObjetoBienesDetalle solicitudObjetoBienesDetalle);
    Task<int> ActualizarEstadoSolicitudesysusObjetosGastos(ActualizarEstadoSolicitudes2Request request);
    Task<int> ModificarSolicitudBienesCircunscripcion(ModificarSolicitudV2Request request);
    Task<int> ActualizarEstadoObjetodeGasto(int codigoSolicitud, int codigoObjetoGasto, int estado);
    Task<Datos<int>> EliminarObjetodeGasto(int codigoSolicitudObjeto, int codigoSolicitud);
    Task<Datos<ObjetosdeGastosyBienesporNroSolicitud>> VisualizarObjetosdeGastosyBienesporNroSolicitud(int codigoSolicitud, int pagina, int cantidadRegistros, int codigoSolicitudObjeto, int? estado);
    Task<int> ObtenerCodigoObjetoGastoapartirdeNumeroObjetoGasto(int numeroObjetoGasto);
    Task<string> ObtenerDescripcionBien(string codigoCatalogo);
    Task<List<RegistrodesdeExcel>> ObtenerDatosValidacionUsuario(string cedulaUsuario);
    Task<Datos<IEnumerable<VerificacionSolicitudes>>> ListadoVerificacionSolicitudes(VerificacionSolicitudesRequest request);
    Task<int> ValidarVersionAbierta(int ejercicio);


    #region DESARROLLO DE LA INCIDENCIA CSJ-138    
    Task<string> Bienprocesado(string CodigoBienDetalle, int CodigoSolicitud, int CodigoSolicitudObjeto, bool eliminar);
    Task<int> EliminarSolicitudObjetosBienesDetalle(int codigoBienDetalle, int codigoSolicitud, int codigoObjetoGasto, string descripcionBienesNacional, int ejercicio);
    Task<List<SolicitudObjetoBienesDetalleDTO>> ObtenerDatosSolicitudObjetoBienesDetallePorCodigoSolicitud(int codigoSolicitud, int pagina, int cantidadRegistros);
    Task<int> ActualizarSolicitudObjetoBienesDetalle(SolicitudObjetoBienesDetalleDTO solicitudObjetoBienesDetalle);

    Task<List<SolicitudObjetoBienesDetalleDTO>> ListadoBienesPorCriterioBusqueda(int ejercicio, int codigoSolicitud, int codigoSolicitudObjeto, int codigo, string descripcion, int pagina, int cantidadRegistros);
    Task<Datos<IEnumerable<SolicitudObjetoBienesDetalleDTO>>> ListadoBienesPorCriterioBusquedaPorFiltros(SolicitudObjetoBienesDetalleRequest request);

    // Task<List<SolicitudObjetoCodigoDescripcionDTO>> ListadoBienesPorCodigoDescripcion(double costoUnitario, int ejercicio, int codigo, int codigoObjetoGasto, int codigoSolicitudGasto, string descripcionSolicitudObjetoBienesDetalle, int pagina, int cantidadRegistros);
    Task<Datos<IEnumerable<SolicitudObjetoCodigoDescripcionDTO>>> ListadoBienesPorCodigoDescripcion(SolicitudObjetoBienesDetalleRequest request);
    Task<int> CantidadRegistrosBienes(double costoUnitario, int ejercicio, int codigoSolicitud, int codigoSolicitudObjeto, int codigo, string descripcionSolicitudObjetoBienesDetalle);
    Task<int> EjercicioSolicitud();
    Task<int> CrearSolicitudObjetosBienesDetalle(SolicitudObjetoBienesDetalleDTO bienDetalle);
    Task<CantidadTotalGenericaDTO> TotalesCatidadBienesSolicitud(int codigoSolicitud, int codigoSolicitudObjeto);
    Task<int> ExisteSolicitudObjetoBienesDetalle(Boolean eliminar, string codigoBienDetalle, int codigoSolicitud, int codigoObjetoGasto, string descripcionBienesNacional, string NumeroBienesNacional, int ejercicio);
    #endregion


}

