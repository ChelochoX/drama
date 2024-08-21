using Domain.Entities.ConsolidadoAnteproyectoPresupuesto;
using Domain.Entities.Request;

namespace Application.Services.Interfaces;

public interface IConsolidadoAnteproyectoPresupuestoService
{
    Task<IEnumerable<ObjetoGastosConsolidadoAnteproyectoDTO>> ListarConsolidadoAnteproyectoporObjetoGasto(ObjetoGastosConsolidadoAnteproyectoRequest request);
    Task<Datos<IEnumerable<BienesdelConsolidadoAnteproyectoDTO>>> ListarBienesdelConsolidadoAnteproyectoPresupuestario(BienesdelConsolidadoAnteproyectoRequest request);
    Task<int> ModificarBiendeVersionAnteproyecto(ModificarBienesVersionAnteproyectoRequest request);
    Task<int> EliminarBiendeVersionAnteproyecto(EliminarBienVersionAnteproyectoRequest request);
    Task<IEnumerable<ObjetodeGastoVersionAnteproyectoDTO>> ListarObjetosdeGastosdeVersionesAnteproyectos(ObjetodeGastoVersionAnteproyectoRequest request);
    Task<int> GestionarEliminarObjetoGastoVersionesAnteproyecto(List<ObjetodeGastoVersionAnteproyectoDTO> items);
}
