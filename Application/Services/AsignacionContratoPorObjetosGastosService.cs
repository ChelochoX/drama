using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using DocumentFormat.OpenXml.Office2016.Excel;
using Domain.DTOs;
using Domain.Entities.AsignacionContratos;
using Domain.Entities.Request;
using Domain.Entities.Request.AsignacionContratos;

namespace Application.Services;

public class AsignacionContratoPorObjetosGastosService : IAsignacionContratoPorObjetosGastosService
{
    private readonly IAsignacionContratoPorObjetosGastosRepository _repository;
    private readonly IMapper _mapper;
    private readonly IGenerarSolicitudporCircunscripcionRepository _user;

    public AsignacionContratoPorObjetosGastosService(IAsignacionContratoPorObjetosGastosRepository repository, IMapper mapper, IGenerarSolicitudporCircunscripcionRepository user)
    {
        _repository = repository;
        _mapper = mapper;
        _user = user;
    }
    public async Task<Datos<IEnumerable<ObjetosdeGastoPorContratoAsignadoDTO>>> ObtenerObjetosdeGastoPorContrato(int codigoAnteproyectoObjeto, int codigoVersion)
    {
        return await _repository.ObtenerObjetosdeGastoPorContrato(codigoAnteproyectoObjeto, codigoVersion);
    }
    public async Task<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>> ObtenerContrato(string numeroContrato)
    {
        return await _repository.ObtenerContrato(numeroContrato);
    }
    public async Task<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>> ObtenerContratoAsignado(ContratoAsignadoRequest request)
    {
        return await _repository.ObtenerContratoAsignado(request);
    }
    public async Task<Datos<int>> InsertarContrato(ContratoPorObjetosdeGasto contratoObj)
    {

        ContratoPorObjetosdeGasto contrato = new ContratoPorObjetosdeGasto();
        contrato.CodigoAnteproyectoObjeto = contratoObj.CodigoAnteproyectoObjeto;
        contrato.CodigoVersion = contratoObj.CodigoVersion;
        contrato.CodigoContrato = contratoObj.CodigoContrato;
        contrato.MontoContrato = contratoObj.MontoContrato;
        contrato.UsuarioInserto = await _repository.ObtenerCodigoUsuario(contratoObj.UsuarioInserto.ToString());

        return await _repository.InsertarContrato(contrato);
    }
    public async Task<Datos<int>> ModificarContrato(ContratoporObjetosdeGastoRequest request)
    {
        request.UsuarioModificacion = await _repository.ObtenerCodigoUsuario(request.UsuarioModificacion.ToString());

        return await _repository.ModificarContrato(request);
    }
    public async Task<Datos<int>> EliminarContrato(int codigoAnteproyectoContrato, int codigoVersion, int codigoAnteproyectoObjeto)
    {
        return await _repository.EliminarContrato(codigoAnteproyectoContrato, codigoVersion, codigoAnteproyectoObjeto);
    }
    public async Task<Datos<IEnumerable<ContratoPorObjetosdeGastoDTO>>> ListarContratos(ContratosRequest request)
    {
        return await _repository.ListarContratos(request);
    }
    public async Task<Datos<int>> RegistroFechaCierreResolucion(FechaCierreResolucion fechaCierreResolucion)
    {
        var cedula = fechaCierreResolucion.UsuarioInserto.ToString();
        fechaCierreResolucion.UsuarioInserto = await _repository.ObtenerCodigoUsuario(cedula);

        return await _repository.RegistroFechaCierreResolucion(fechaCierreResolucion);
    }

    public async Task<Datos<IEnumerable<ListarFechaCierreResolucion>>> ListadoFechaCierreResolucion(ListarFechaCierreResolucionRequest request)
    {
        return await _repository.ListadoFechaCierreResolucion(request);
    }
    public async Task<Datos<IEnumerable<ListadoNotificacionesUsuarioDTO>>> ObtenerListadoNotificacionesUsuario(ListadoNotificacionesUsuarioRequest request)
    {
        return await _repository.ObtenerListadoNotificacionesUsuario(request);
    }
    public async Task<int> CantidadNotificacionesAbiertas(int usuario)
    {
        return await _repository.CantidadNotificacionesAbiertas(usuario);
    }

}

