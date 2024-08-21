using Domain.Entities.PlanificacionFinanciera;
using Domain.Entities.Request;

namespace Application.Services.Interfaces;

public interface IPlanificacionFinancieraService
{
    Task<Datos<IEnumerable<VersionesAnteproyectoDTO>>> ListadoVersionesanteproyectos(VersionesAnteproyectoRequest request);
    Task<int> ObtenerDiasRestantesCierre();
    Task<Datos<IEnumerable<PlanificacionFinancieraDTO>>> ListadoPlanificacionFinanciera(PlanificacionFinancieraRequest request);
    Task<IEnumerable<PlanificacionFinancieraporObjetoGastoDTO>> ListadoPlanificacionFinancieraporObjetoGasto(PlanificacionFinancieraporObjetoGastoRequest request);
    Task<IEnumerable<PlanificacionMensualDTO>> ListadoPlanificacionMensual(PlanificacionMensualRequest request);
    Task<CodigoAnteproyectoOBJparaPlanificacionFinancieraDTO> ObtenerCodigoAnteproyectoObjeto(CodigoAnteproyectoOBJparaPlanificacionFinancieraRequest request);
    Task<IEnumerable<MesesparaPlanificacionFinancieraDTO>> ObtenerMesesParalaPlanificacionFinanciera(MesesparaPlanificacionFinancieraRequest request);
    Task<int> GestionarInsertarPlanificacionFinanciera(GestionarInsertarPlanificacionFinancieraRequest request);
    Task<int> GestionarEditarPlanificacionFinanciera(GestionarEditartarPlanificacionFinancieraRequest request);
    Task<IEnumerable<PlanificacionMensualDTO>> ListadoPlanificacionMensualporItem(PlanificacionMensualporItemRequest request);
    Task<int> EliminarPlanificacionFinanciera(EliminarPlanificacionFinancieraRequest request);
}
