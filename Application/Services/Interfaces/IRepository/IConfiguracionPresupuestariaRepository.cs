using Domain.Entities.ConfiguracionPresyupuestaria;
using Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;
using Domain.Entities.Request;
using Domain.Entities.Response;
using DatosparaConfiguracionPresupuestariaDTO = Domain.Entities.ConfiguracionPresyupuestaria.DatosparaConfiguracionPresupuestariaDTO;

namespace Application.Services.Interfaces.IRepository;

public interface IConfiguracionPresupuestariaRepository
{
   
    Task<Datos<IEnumerable<ConfiguracionPresupuestariaPorObjetosGastosDTO>>> ObtenerObjetosdeGastosparalaConfiguracionPresupuestaria(ConfiguracionPresupuestariaPorObjetosGastosRequest request);
    Task<Datos<DatosdeConfiguracionPresupuestariaDTO>> InsertarCabeceraparaConfiguracionPresupuestaria(DatosparaCabeceraConfiguracionPresupuestariaRequest request);
    Task<Datos<int>> InsertarAnteproyectoObjetos(DatosparaAnteproyectoObjetosRequest request, PresupuestoInicialyModificacionesConsolidadoDTO montos);
    Task<Datos<int>> InsertarBienesporConfiguracionPresupuestariaConsolidado(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion);
    Task<Datos<int>> InsertarBienesporConfiguracionPresupuestariaNoConsolidado(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion);
    Task<List<BienesparaConfiguracionPresupuestariaDTO>> ObtenerBienesparaAgregarFinanciamiento(BienesparaConfiguracionPresupuestariaRequest request);
    Task<DatosparaConfiguracionPresupuestariaDTO> ObtenerDatosparaInsertarConfiguracionPresupuestaria(DatosparaConfiguracionPresupuestariaRequest request);
    Task<Datos<IEnumerable<OrganismoFinanciadorDTO>>> ObtenerDatosOrganismoFinanciador(OrganismoFinanciadorRequest request);
    Task<Datos<IEnumerable<FuenteFinanciamientoDTO>>> ObtenerDatosFuenteFinanciamiento(FuenteFinanciamientoRequest request);
    Task<int> EliminarObjetodeAnteproyectoObjetoyBienes(VerionesAnteproyectoObjetosEliminarRequest request);
    Task<int> ModificarFinanciamientoAnteproyectoObjeto(ModificarFinanciamientoAnteproyectoObjetosRequest request);
    Task<int> ActualizarCantidadBiendelConsolidadoBienes(ConfiguracionPresupuestariadeBienesRequest request, int CodigoAnteproyectoObj, int CodigoVersion);
    Task<int> EliminarBienesporFuentedeFinanciamiento(int cabVersion, int cabObjeto, int cabBienes, int codigoMateria, int codigoCentroResponsabilidad);
    Task<int> EliminarBienesporFuentedeFinanciamientoModif(int cabVersion, int cabObjeto, int cabBienes, int codigoMateria, int codigoCentroResponsabilidad);
    Task<Datos<IEnumerable<BienesparaConfiguracionPresupuestariaDTO>>> ObtenerBienesparaFuenteFinanciamiento(BienesparaConfiguracionPresupuestariaRequest request);
    Task<IEnumerable<FinanciamientoVersionAnteproyectoObjetosResponseDTO>> ObtenerDatosVersionesAnteproyectosObjetos(FinanciamientoVersionAnteproyectoObjetosRequest request);
    Task<IEnumerable<DatosdeSolicitudporObjetoGastoResponseDTO>> ObtenerDatosdelaSolicitud(DatosdeSolicitudporObjetoGastoRequest request);
    Task<int> ModificarMontoPlanificadoAnteproyectoObjeto(int versionAnteproyecto, int configuracionPresupuestaria, int versionAnteproyectoObjeto
        ,int codigoMateria, int codigoCentroResp);

    Task<int> ModificarMontoPlanificadoAnteproyectoObjetoModif(int versionAnteproyecto, int configuracionPresupuestaria, int versionAnteproyectoObjeto,
    int codigoMateria, int codigoCentroResp);
    Task<int> VerificarExisteVersiondeAnteproyectoCerrado(int ejercicio, int codigoCircunscripcion);

}
