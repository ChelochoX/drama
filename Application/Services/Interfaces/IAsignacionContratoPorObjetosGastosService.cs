using Domain.DTOs;
using Domain.Entities.Request;
using Domain.Entities.AsignacionContratos;
using Domain.Entities.Request.AsignacionContratos;

namespace Application.Services.Interfaces;

public interface IAsignacionContratoPorObjetosGastosService
{
    Task<Datos<IEnumerable<ObjetosdeGastoPorContratoAsignadoDTO>>> ObtenerObjetosdeGastoPorContrato(int codigoAnteproyectoObjeto, int codigoVersion);
    Task<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>> ObtenerContrato(string numeroContrato);
    Task<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>> ObtenerContratoAsignado(ContratoAsignadoRequest request);
    Task<Datos<int>> InsertarContrato(ContratoPorObjetosdeGasto request);
    Task<Datos<int>> ModificarContrato(ContratoporObjetosdeGastoRequest request);
    Task<Datos<int>> EliminarContrato(int codigoAnteproyectoContrato, int codigoVersion, int codigoAnteproyectoObjeto);
    Task<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>> ListarContratos(ContratosRequest request);
    Task<Datos<int>> RegistroFechaCierreResolucion(FechaCierreResolucion FechaCierreResolucion);
    Task<Datos<IEnumerable<ListarFechaCierreResolucion>>> ListadoFechaCierreResolucion(ListarFechaCierreResolucionRequest request);
    Task<Datos<IEnumerable<ListadoNotificacionesUsuarioDTO>>> ObtenerListadoNotificacionesUsuario(ListadoNotificacionesUsuarioRequest request);

    Task<int> CantidadNotificacionesAbiertas(int usuario);
    
}
