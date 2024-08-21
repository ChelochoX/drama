
using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using DocumentFormat.OpenXml.Office2016.Excel;
using Domain.Entities.Referencias;
using Domain.Entities.Request;
using Domain.Exceptions.Referencias;

namespace Application.Services
{
    public class ReferenciaService : IReferenciaService
    {
        private readonly IReferenciasRepository _repository;
        private readonly IMapper _mapper;

        public ReferenciaService(IReferenciasRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<Datos<IEnumerable<TipoReferenciaDTO>>> ObtenerTiposReferencias(TipoReferenciasRequest request)
        {  
            return await _repository.ObtenerTiposReferencias(request);
        }
        public async Task<List<TipoReferenciaDTO>> ObtenerDescripcionTiposReferencias(string? descripcionTipoReferencia, int codigoTipoReferencia, int estado)
        {
            if (string.IsNullOrEmpty(descripcionTipoReferencia) || descripcionTipoReferencia == "null")
            {
                descripcionTipoReferencia = null;
            }

            return await _repository.ObtenerDescripcionTiposReferencias(descripcionTipoReferencia, codigoTipoReferencia, estado);
        }
        public async Task<List<TipoReferenciaDTO>> ObtenerDominioTiposReferencias(string? dominioTipoReferencia, int codigoTipoReferencia, int estado)
        {
            if (string.IsNullOrEmpty(dominioTipoReferencia) || dominioTipoReferencia == "null")
            {
                dominioTipoReferencia = null;
            }

            return await _repository.ObtenerDominiosTiposReferencias(dominioTipoReferencia, codigoTipoReferencia, estado);
        }
        public async Task<List<ReferenciasDTO>> ObtenerReferencias(int pagina, int cantidadRegistros, string? dominioReferencia, int codigoTipoReferencia,
                                                                      string? descripcionReferencia, int estado, DateTime valorFecha, string valorAlfanumerico,
                                                                      string descripcionLarga, decimal? valorDecimal)
        {
            if (string.IsNullOrEmpty(dominioReferencia) || dominioReferencia == "null")
            {
                dominioReferencia = null;
            }
            if (string.IsNullOrEmpty(descripcionReferencia) || descripcionReferencia == "null")
            {
                descripcionReferencia = null;
            }
            if (string.IsNullOrEmpty(valorAlfanumerico) || valorAlfanumerico == "null")
            {
                valorAlfanumerico = null;
            }
            if (string.IsNullOrEmpty(descripcionLarga) || descripcionLarga == "null")
            {
                descripcionLarga = null;
            }

            return await _repository.ObtenerReferencias(pagina, cantidadRegistros, dominioReferencia, codigoTipoReferencia, descripcionReferencia, estado, valorFecha, valorAlfanumerico, descripcionLarga, valorDecimal);
        }
        public async Task<List<ReferenciasDTO>> ObtenerDominioReferencias(string? dominioReferencia, int codigoReferencia, int estado)
        {
            if (string.IsNullOrEmpty(dominioReferencia) || dominioReferencia == "null")
            {
                dominioReferencia = null;
            }

            return await _repository.ObtenerDominioReferencias(dominioReferencia, codigoReferencia, estado);
        }
        public async Task<List<ReferenciasDTO>> ObtenerDescripcionReferencias(string? descripcionReferencia, int codigoReferencia, int estado)
        {
            if (string.IsNullOrEmpty(descripcionReferencia) || descripcionReferencia == "null")
            {
                descripcionReferencia = null;
            }

            return await _repository.ObtenerDescripcionReferencias(descripcionReferencia, codigoReferencia, estado);
        }
        public async Task<Datos<int>> InsertarTipoReferencia(TipoReferencias tipoReferenciaObj)
        {
            TipoReferencias tipoReferencia = new TipoReferencias();
            tipoReferencia.DescripcionTipoReferencia = tipoReferenciaObj.DescripcionTipoReferencia;
            tipoReferencia.DominioTipoReferencia = tipoReferenciaObj.DominioTipoReferencia;
            tipoReferencia.Estado = 1;
            tipoReferencia.UsuarioInserto = await _repository.ObtenerCodigoUsuario(tipoReferenciaObj.UsuarioInserto.ToString());
            
            return await _repository.InsertarTipoReferencia(tipoReferencia);
        }
        public async Task<Datos<int>> InsertarReferencia(Referencias referenciaObj)
        {
            Referencias referencias = new Referencias();
            referencias.DominioReferencia = referenciaObj.DominioReferencia;
            referencias.CodigoTipoReferencia = referenciaObj.CodigoTipoReferencia;
            referencias.DescripcionReferencia = referenciaObj.DescripcionReferencia;
            referencias.Estado = 1;
            referencias.ValorFecha = referenciaObj.ValorFecha;
            referencias.ValorAlfanumerico = referenciaObj.ValorAlfanumerico;
            referencias.DescripcionLarga = referenciaObj.DescripcionLarga;
            referencias.ValorDecimal = referenciaObj.ValorDecimal;
            referencias.UsuarioInserto = await _repository.ObtenerCodigoUsuario(referenciaObj.UsuarioInserto.ToString());
            
            return await _repository.InsertarReferencia(referencias);
        }
            public async Task<Datos<int>> ModificarTipoReferencia(TipoReferencias tipoReferenciaObj)
        {
            tipoReferenciaObj.UsuarioModificacion = await _repository.ObtenerCodigoUsuario(tipoReferenciaObj.UsuarioModificacion.ToString());

            return await _repository.ModificarTipoReferencia(tipoReferenciaObj);
        }
        public async Task<Datos<int>> ModificarReferencia(Referencias referenciasObj)
        { 
            referenciasObj.UsuarioModificacion = await _repository.ObtenerCodigoUsuario(referenciasObj.UsuarioModificacion.ToString());

            return await _repository.ModificarReferencia(referenciasObj);
        }
        public async Task<Datos<IEnumerable<ReferenciasDTO>>> ObtenerListadoReferencias(ReferenciasRequest request)
        {
            return await _repository.ObtenerListadoReferencias(request);
        }

    }
}
