using Domain.Entities.Referencias;
using Domain.Entities.Request;

namespace Application.Services.Interfaces
{
    public interface IReferenciaService
    {
        Task<Datos<IEnumerable<TipoReferenciaDTO>>> ObtenerTiposReferencias(TipoReferenciasRequest request);
        Task<List<TipoReferenciaDTO>> ObtenerDescripcionTiposReferencias(string? descripcionTipoReferencia, int codigoTipoReferencia, int estado);
        Task<List<TipoReferenciaDTO>> ObtenerDominioTiposReferencias(string? dominioTipoReferencia, int codigoTipoReferencia, int estado);
        Task<List<ReferenciasDTO>> ObtenerReferencias(int pagina, int cantidadRegistros, string? dominioReferencia, int codigoTipoReferencia,
                                                                      string? descripcionReferencia, int estado, DateTime valorFecha, string valorAlfanumerico,
                                                                      string descripcionLarga, decimal? valorDecimal);
        Task<List<ReferenciasDTO>> ObtenerDominioReferencias(string? dominioReferencia, int codigoReferencia, int estado);
        Task<List<ReferenciasDTO>> ObtenerDescripcionReferencias(string? descripcionReferencia, int codigoReferencia, int estado);
        Task<Datos<int>> InsertarTipoReferencia(TipoReferencias tipoReferenciaObj);
        Task<Datos<int>> InsertarReferencia(Referencias referenciaObj);
        Task<Datos<int>> ModificarTipoReferencia(TipoReferencias tipoReferenciaObj);
        Task<Datos<int>> ModificarReferencia(Referencias referenciasObj);
        Task<Datos<IEnumerable<ReferenciasDTO>>> ObtenerListadoReferencias(ReferenciasRequest request);

    }
}
