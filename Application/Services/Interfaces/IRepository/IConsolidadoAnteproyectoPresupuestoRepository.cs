using Domain.Entities.ConsolidadoAnteproyectoPresupuesto;
using Domain.Entities.Request;

namespace Application.Services.Interfaces.IRepository;

public interface IConsolidadoAnteproyectoPresupuestoRepository
{
    Task<IEnumerable<ObjetoGastosConsolidadoAnteproyectoDTO>> ListarConsolidadoAnteproyectoporObjetoGasto(ObjetoGastosConsolidadoAnteproyectoRequest request);
    Task<Datos<IEnumerable<BienesdelConsolidadoAnteproyectoDTO>>> ListarBienesdelConsolidadoAnteproyectoPresupuestario(BienesdelConsolidadoAnteproyectoRequest request);
    Task<int> ModificarBiendeVersionAnteproyecto(ModificarBienesVersionAnteproyectoRequest request, int usuarioModificacion);
    Task<int> EliminarBiendeVersionAnteproyecto(EliminarBienVersionAnteproyectoRequest request);
    Task<IEnumerable<ObjetodeGastoVersionAnteproyectoDTO>> ListarObjetosdeGastosdeVersionesAnteproyectos(ObjetodeGastoVersionAnteproyectoRequest request);
    Task<int> EliminarBiendeVersionAnteproyectoporOBGVersiones(ObjetodeGastoVersionAnteproyectoDTO request);
    Task<int> EliminardeVersionesContratoporOBGVersiones(ObjetodeGastoVersionAnteproyectoDTO request);
    Task<int> EliminardeVersionesPlanificacionporOBGVersiones(ObjetodeGastoVersionAnteproyectoDTO request);
    Task<int> EliminarObjetodeVersionesAnteproyectoporOBGVersiones(ObjetodeGastoVersionAnteproyectoDTO request);    
}
