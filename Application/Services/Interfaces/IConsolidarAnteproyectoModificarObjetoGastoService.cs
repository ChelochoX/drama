﻿using Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;
using Domain.Entities.Request;

namespace Application.Services.Interfaces;

public interface IConsolidarAnteproyectoModificarObjetoGastoService
{
    Task<IEnumerable<CentroResponsabilidadyMateriaDTO>> ObtenerCentroResponsabilidadyMateriaporUsuario(string cedulaUsuario);
    Task<IEnumerable<CircunscripcionesparaConsolidadoAnteproyectoDTO>> ObtenerCircunscripciones();
    Task<IEnumerable<ObjetosGastosConsolidacionAnteproyectoDTO>> ObtenerObjetosGastos(int ejercicio);
    Task<IEnumerable<DatosparaConfiguracionPresupuestariaConsolidadoDTO>> ObtenerDatosparalaConfiguracionPresupuestaria(DatosparaConfiguracionPresupuestariaConsolidadoRequest request);
    Task<IEnumerable<BienesdeAnteproyectoBienesConsolidadoDTO>> ObtenerBienesdeAnteproyectoObjeto(BienesdeAnteproyectoBienesConsolidadoRequest request);
    Task<Datos<IEnumerable<BienesparaConsolidadoDTO>>> ObtenerBienesparaConsolidado(BienesparaConsolidadoRequest request);
    Task<int> InsertarConfiguracionPresupuestariaDesdeConsolidado(ConfiguracionPresupuestariaConsolidadoRequest request);
    Task<int> GestionarInsertarVersionadoObjetoGastoConsolidado(DatosparaVersionesAnteproyectoObjetoConsolidadoRequest request);
    Task<int> GestionarInsertarVersionadoBienesConsolidado(DatosparaVersionesAnteproyectoBienesConsolidadoRequest request);
    Task<int> ModificarAnteproyectoObjetoConsolidado(ModificarOBGConsolidadoRequest request);
    Task<int> ModificarAnteproyectoBienesConsolidado(ModificarBienesConsolidadoRequest request);
    Task<PresupuestoInicialyModificacionesConsolidadoDTO> ObtenerPresupuestoInicialyModificaciones(PresupuestoInicialyModificacionesConsolidadoRequest request);
    Task<int> EliminarBiendesdeConsolidadoModif(int CodigoVersionAnteproyecto, int CodigoVersionAnteproyectoObjeto, int CodigoVersionAnteproyectoBien, int CodigoConfiguracionPresupuestaria,
        int codigoMateria, int codigoCentroResponsabilidad);
    Task<int> ValidarExistenciaenAnteproyectoObjetoConsolidado(ValidarExistenciaenAnteproyectoObjetoConsolidadoRequest request);
    Task<int> ObtenerEjercicioActivo();
    Task<int> ObtenerMontoPlanificado(MontoPlanificadoRequest request);
    Task<Datos<IEnumerable<ObjetosGastosPendientesaConfigurarDTO>>> ValidarOBGPerteneceaConfiguracion(int codigoVersion);
}
