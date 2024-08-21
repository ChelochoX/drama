using Domain.Entities.Server;

namespace Application.Services.Interfaces;

public interface IReportesService
{
    Task<byte[]> GenerarReporteFG03(FG03Request request);
    Task<byte[]> GenerarReporteFG07(FG07Request request);
    Task<byte[]> GenerarReporteSolicitudesBienes(SolicitudesBienesRequest request);
    Task<byte[]> GenerarReporteFG03FF(FG03FFRequest request);
    Task<byte[]> GenerarReporteFG03FFGeneral(FG03FFGeneralRequest request);
    Task<byte[]> GenerarReporteFG02(FG02Request request);
    Task<byte[]> GenerarReporteFG03_Consolidado(FG03Consolidado request);
    Task<byte[]> GenerarReporteFG02_Consolidado(FG03Consolidado request);
    Task<byte[]> GenerarReporteVersionadoAnteproyecto(ConsultaVersionadoAnteproyecto request);
    Task<byte[]> GenerarReportePorGrupoGasto(ReportePorGrupoGasto request);
    Task<byte[]> GenerarFG03FuenteFinanciacionCircunscripcion(FG03FuenteFinanciacionCircunscripcion request);
    Task<byte[]> GenerarJustificacionCreditosPresupuestarios(JustificacionCreditosPresupuestarios request);

}
