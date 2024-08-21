using Domain.Entities.PlanificacionFinanciera;
using Domain.Entities.Request;

namespace Application.Services.Interfaces.IRepository;

public interface IPlanificacionFinancieraRepository
{
    Task<Datos<IEnumerable<VersionesAnteproyectoDTO>>> ListadoVersionesanteproyectos(VersionesAnteproyectoRequest request);
    Task<int> ObtenerDiasRestantesCierre();
    Task<Datos<IEnumerable<PlanificacionFinancieraDTO>>> ListadoPlanificacionFinanciera(PlanificacionFinancieraRequest request);
    Task<IEnumerable<PlanificacionFinancieraporObjetoGastoDTO>> ListadoPlanificacionFinancieraporObjetoGasto(PlanificacionFinancieraporObjetoGastoRequest request);
    Task<IEnumerable<PlanificacionMensualDTO>> ListadoPlanificacionMensual(PlanificacionMensualRequest request);
    Task<CodigoAnteproyectoOBJparaPlanificacionFinancieraDTO> ObtenerCodigoAnteproyectoObjeto(CodigoAnteproyectoOBJparaPlanificacionFinancieraRequest request);
    Task<IEnumerable<MesesparaPlanificacionFinancieraDTO>> ObtenerMesesParalaPlanificacionFinanciera(MesesparaPlanificacionFinancieraRequest request);
    Task<int> InsertarPlanificacionFinanciera(InsertarPlanificacionFinancieraRequest request);
    Task<decimal> ValidarMontodePlanificacionFinanciera(MesesparaPlanificacionFinancieraRequest request);
    Task<int> ValidarCantidadMesesporEvento(MesesparaPlanificacionFinancieraRequest request);
    Task<int> EditarPlanificacionFinanciera(InsertarPlanificacionFinancieraRequest request);
    Task<IEnumerable<PlanificacionMensualDTO>> ListadoPlanificacionMensualporItem(PlanificacionMensualporItemRequest request);
    Task<int> EliminarPlanificacionFinanciera(EliminarPlanificacionFinancieraRequest request);
}
