using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Anteproyecto;
using Domain.Entities.Request;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public class SolicitudObjetosBienesService : ISolicitudObjetosBienesService
{

    private readonly IGenerarSolicitudCircunscripcionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConectarCircunscripcionRepository _repositoryConectar;
    private readonly IGenerarSolicitudporCircunscripcionRepository _userData;

    public SolicitudObjetosBienesService(IGenerarSolicitudCircunscripcionRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConectarCircunscripcionRepository repositoryConectar, IGenerarSolicitudporCircunscripcionRepository userData)
    {
        _repository = repository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _repositoryConectar = repositoryConectar;
        _userData = userData;
    }

    #region DESARROLLO DE LA INCIDENCIA CSJ-152
    public async Task<Datos<IEnumerable<VersionAnteproyectoDTO>>> VersionAnteproyectoRequest(VersionAnteproyectoRequest request)
    {
        return await _repository.VersionAnteproyectoRequest(request);
    }

    public async Task<Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>> AnteproyectoPresupuestarioRequest(AnteproyectoPresupuestarioRequest request)
    {
        return await _repository.AnteproyectoPresupuestarioRequest(request);
    }
    public async Task<CantidadTotalDiasDTO> CantidadTotalDias()
    {
        return await _repository.CantidadTotalDias();
    }
    public async Task<int> ActualizarEstadoVersionAnteproyecto(ActualizarEstadoVersionAnteproyectoRequest version)
    {
        string stringCir = _httpContextAccessor.HttpContext.Request.Headers["codCircunscripcion"].ToString().Trim();
        int codCircunscripcion = 1;
        if (!string.IsNullOrEmpty(stringCir))
        {
            codCircunscripcion = int.Parse(stringCir);
        }

        //Buscamos el codigo de la circunscripcion correcta.
        var valorCodigo = await _repositoryConectar.ObtenerDatosparaConectarCircunscripcion(codCircunscripcion);
        var codigoRealCircunscripcion = valorCodigo.CodigoCircunscripcion;


        var resultado = await _repository.ActualizarEstadoVersionAnteproyecto(version);


        await _repository.SincronizarDBCapital(version.CodigoVersion, codigoRealCircunscripcion, version.CodigoUsuarioLoggeado, 0);

        await _repository.NotificacionesCierre(version.CodigoVersion, version.CodigoUsuarioLoggeado, codigoRealCircunscripcion);

        return resultado;
    }
    public async Task<int> CerrarEstadoVersionAnteproyecto(ActualizarEstadoVersionAnteproyectoRequest version)
    {
        var resultado = await _repository.CerrarEstadoVersionAnteproyecto(version);        

        return resultado;
    }
    

    public async Task<int> ActualizarVersionAnteproyectoObjeto(ActualizarVersionAnteproyectoObjetoRequest version)

    {
        return await _repository.ActualizarVersionAnteproyectoObjeto(version);
    }
    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-154 
    public async Task<Datos<IEnumerable<AnteproyectoPresupuestarioDTO>>> AnteproyectoPresupuestarioVerificacionesRequest(AnteproyectoPresupuestarioRequest request)
    {
        return await _repository.AnteproyectoPresupuestarioVerificacionesRequest(request);
    }

    public async Task<Datos<IEnumerable<AnteproyectoPresupuestarioCABDTO>>> AnteproyectoPresupuestarioCAB(int pagina, int cantidadRegistros, string? terminoDeBusqueda, int? numeroVersion, int? ejercicio, string? estado)
    {

        return await _repository.AnteproyectoPresupuestarioCAB(pagina, cantidadRegistros, terminoDeBusqueda, numeroVersion, ejercicio, estado);
    }
    public async Task<int> AbrirVersionAnteproyecto(int version, string user)
    {
        var usuarioModificacion = await _userData.ObtenerCodigoUsuario(user);
        return await _repository.AbrirVersionAnteproyecto(version, usuarioModificacion);
    }

    public async Task<int> EliminarObjetodeAnteproyectoyBienes(int version, int codigoVersionAnteproyecto, int codigoConfigPresupuestaria)
    {
        return await _repository.EliminarObjetodeAnteproyectoyBienes(version, codigoVersionAnteproyecto, codigoConfigPresupuestaria);
    }

    public async Task<Datos<IEnumerable<VersionesAnteproyectosDTO>>> ObtenerVersiones()
    {
        return await _repository.ObtenerVersiones();

    }
    public async Task<int> ObtenerVersionAbierta()
    {
        return await _repository.ObtenerVersionAbierta();

    }
  

    #endregion


    #region DESARROLLO DE LA INCIDENCIA CSJ-158

    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> ObtenerCentroResponsabilidadReporte(int cedula)
    {
        return await _repository.ObtenerCentroResponsabilidadReporte(cedula);
    }
    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCentroResponsabilidad(string codigoCircunscripcion)
    {
        return await _repository.AgregarOGCentroResponsabilidad(codigoCircunscripcion);
    }
    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGDependenciaCircuscripcion(string user)
    {
        return await _repository.AgregarOGDependenciaCircuscripcion(user);
    }
    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGMateria(string user)
    {
        return await _repository.AgregarOGMateria(user);
    }
    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarCircuscripcion(string user)
    {
        return await _repository.AgregarCircuscripcion(user);
    }
    
    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGDepartamento(string codigoCentroResponsabilidad)
    {
        return await _repository.AgregarOGDepartamento(codigoCentroResponsabilidad);
    }
    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOG(int ejercicio)
    {
        return await _repository.AgregarOG(ejercicio);
    }
    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCodigoOrganismoFinanciador()
    {
        return await _repository.AgregarOGCodigoOrganismoFinanciador();
    }
    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> AgregarOGCodigoFuenteFinanciamiento()
    {
        return await _repository.AgregarOGCodigoFuenteFinanciamiento();
    }
    public async Task<Datos<IEnumerable<ClaseCentroResponsabilidadDTO>>> DatosOGCentroResponsabilidad(int codigo)
    {
        return await _repository.DatosOGCentroResponsabilidad(codigo);
    }
    public async Task<Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>> BuscarBien(int codigoObjetoGasto, int ejercicio, int pagina, int cantidadRegistros, string MontoUnitario, string TerminoDeBusqueda, string catalogo, string descripcionCatalogo)
    {
        return await _repository.BuscarBien(codigoObjetoGasto, ejercicio, pagina, cantidadRegistros, MontoUnitario, TerminoDeBusqueda, catalogo, descripcionCatalogo);
    
     }
    public async Task<Datos<IEnumerable<VersionAnteproyectoBienSoloDTO>>> BuscarBienesGrilla(int version, int codigoAnteproyectoObjeto)
    {
        return await _repository.BuscarBienesGrilla(version, codigoAnteproyectoObjeto);

    }    
    public async Task<Datos<IEnumerable<ClaseDatosOGGrupoSubGrupoDTO>>> DatosOGGrupoSubGrupo(int codigo, int ejercicio)
    {
        return await _repository.DatosOGGrupoSubGrupo(codigo, ejercicio);
    }
    public async Task<Datos<IEnumerable<ClaseDatosOGBienesprioritariosDTO>>> DatosOGBienesprioritarios(int version, int codigo)
    {
        return await _repository.DatosOGBienesprioritarios(version, codigo);
    }
    public async Task<CantidadTotalGenericaDTO> DatosOGBienesMonto(int version, int codigo)
    {
        return await _repository.DatosOGBienesMonto(version, codigo);
    }
    public async Task<Datos<IEnumerable<ClaseGenericaCodigoDescripDTO>>> BuscarBienesPrioritarios(int codigo)
    {
        return await _repository.BuscarBienesPrioritarios(codigo);
    }
   public async Task<int> EliminarBienAnteproyecto(int codigoAnteProyectoBien, int codigoObjetoGasto)
    {
        return await _repository.EliminarBienAnteproyecto(codigoAnteProyectoBien, codigoObjetoGasto);
    }
    public async Task<ClaseLlavePrimaria> AgregarConfiguracionPresupuestaria(ClaseConfiguracionPresupuestaria configuracion)
    {
        var cedula = configuracion.UsuarioInserto.ToString();
        var codigoUsuario = await _userData.ObtenerCodigoUsuario(cedula);
        configuracion.UsuarioInserto = codigoUsuario;
        return await _repository.AgregarConfiguracionPresupuestaria(configuracion);
    }
    #endregion



    #region DESARROLLO DE LA INCIDENCIA CSJ-ARCHIVOS
    public async Task<string> PersistirArchivo(int codigoVersion, int usuarioInserto,string dominio, string referencia, string nombre, string extension)
    {
        return await _repository.PersistirArchivo( codigoVersion , usuarioInserto, dominio, referencia, nombre, extension);
    }

    public async Task<ClaseBajarArchivo> BajarArchivo(int codigoVersion, string usuarioInserto, string dominio)
    {
        return await _repository.BajarArchivo(codigoVersion, usuarioInserto, dominio);
    }

    
    #endregion

    #region DESARROLLO DE LA INCIDENCIA CSJ-312

    public async Task<int> SincronizarDBCapital(int codigoVersion, int codigoCircunscripcion, string usuarioEjecucion, int codigoTarea)
    {
        var codigoUsuario = await _userData.ObtenerCodigoUsuario(usuarioEjecucion);
        return await _repository.SincronizarDBCapital(codigoVersion, codigoCircunscripcion, codigoUsuario.ToString(), codigoTarea);
    }

    #endregion

}

