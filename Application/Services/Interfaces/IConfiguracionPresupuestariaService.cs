using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.Request;
using Domain.Entities.Request.ConfiguracionPresupuestaria;
using Domain.Entities.Response;

namespace Application.Services.Interfaces;

public interface IConfiguracionPresupuestariaService
{
   
    Task<Datos<IEnumerable<ConfiguracionPresupuestariaPorObjetosGastosDTO>>> ObtenerConfiguracionPresupuestariaObjetosGastos(ConfiguracionPresupuestariaPorObjetosGastosRequest request);
    Task<Datos<DatosdeConfiguracionPresupuestariaDTO>> InsertarCabeceraparaConfiguracionPresupuestaria(DatosparaCabeceraConfiguracionPresupuestaria2Request request);
    Task<Datos<int>> InsertarBienesporConfiguracionPresupuestariaNoConsolidado(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion);
    Task<Datos<int>> InsertarBienesporConfiguracionPresupuestariaConsolidado(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion);
    Task<DatosparaConfiguracionPresupuestariaDTO> ObtenerDatosparaInsertarConfiguracionPresupuestaria(DatosparaConfiguracionPresupuestariaRequest request);
    Task<Datos<IEnumerable<OrganismoFinanciadorDTO>>> ObtenerDatosOrganismoFinanciador(OrganismoFinanciadorRequest request);
    Task<Datos<IEnumerable<FuenteFinanciamientoDTO>>> ObtenerDatosFuenteFinanciamiento(FuenteFinanciamientoRequest request);
    Task<int> EliminarObjetodeAnteproyectoObjetoyBienes(VerionesAnteproyectoObjetosEliminarRequest request);
    Task<int> ModificarFuenteFinanciamientoyBienes(ModificarFuenteFinanciamientoyBienesRequest request);
    Task<Datos<IEnumerable<BienesparaConfiguracionPresupuestariaDTO>>> ObtenerBienesparaFuenteFinanciamiento(BienesparaConfiguracionPresupuestariaRequest request);
    Task<int> GestionarFuentesdeFinanciamientoConsusBienes(DatosparaAnteproyectoObjetosyBienesRequest request);
    Task<IEnumerable<FinanciamientoVersionAnteproyectoObjetosResponseDTO>> ObtenerDatosVersionesAnteproyectosObjetos(FinanciamientoVersionAnteproyectoObjetosRequest request);
    Task<IEnumerable<DatosdeSolicitudporObjetoGastoResponseDTO>> ObtenerDatosdelaSolicitud(DatosdeSolicitudporObjetoGastoRequest request);
    Task<int> VerificarExisteVersiondeAnteproyectoCerrado(int ejercicio);
    

}
