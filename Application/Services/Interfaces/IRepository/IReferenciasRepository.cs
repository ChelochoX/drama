using Domain.Entities.Referencias;
using Domain.Entities.Request;

namespace Application.Services.Interfaces.IRepository
{
    public interface IReferenciasRepository
    {
        Task<Datos<IEnumerable<TipoReferenciaDTO>>> ObtenerTiposReferencias(TipoReferenciasRequest request);
        Task<List<TipoReferenciaDTO>> ObtenerDescripcionTiposReferencias(string? descripcionTipoReferencia, int codigoTipoReferencia, int estado);
        Task<List<TipoReferenciaDTO>> ObtenerDominiosTiposReferencias(string? dominioTipoReferencia, int codigoTipoReferencia, int estado);
        Task<List<ReferenciasDTO>> ObtenerReferencias(int pagina, int cantidadRegistros, string? dominioReferencia, int codigoTipoReferencia,
                                                      string? descripcionReferencia, int estado, DateTime valorFecha, string valorAlfanumerico,
                                                      string descripcionLarga, decimal? valorDecimal);
        Task<List<ReferenciasDTO>> ObtenerDominioReferencias(string? dominioReferencia, int codigoReferencia,
                                                                          int estado);
        Task<List<ReferenciasDTO>> ObtenerDescripcionReferencias(string? descripcionReferencia, int codigoReferencia,
                                                                          int estado);
        Task<Datos<int>> InsertarTipoReferencia(TipoReferencias tipoReferencia);
        Task<Datos<int>> InsertarReferencia(Referencias referencia);
        Task<Datos<int>> ModificarTipoReferencia(TipoReferencias tipoReferencia);
        Task<Datos<int>> ModificarReferencia(Referencias referencia);
        Task<List<TipoReferenciaDTO>> ObtenerTipoReferenciaDescripcion(string dominioTipoReferencia, int? codigoTipoReferencia = null);
        Task<List<TipoReferenciaDTO>> ObtenerReferenciaDominio(string dominioReferencia, int? codigoReferencia = null);
        Task<string> ObtenerNombreUsuario(string cedula);
        Task<int> ObtenerCodigoUsuario(string cedula);
        Task<Datos<IEnumerable<ReferenciasDTO>>> ObtenerListadoReferencias(ReferenciasRequest request);

    }

}