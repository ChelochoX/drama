using Application.Services.Interfaces;
using Application.Services.Interfaces.IRepository;
using Domain.Entities.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class ReportesService : IReportesService
{
    private readonly HttpClient _httpClient;
    private readonly JasperServer _data;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConectarCircunscripcionRepository _repository;
    private string _base_url;

    public ReportesService(IHttpClientFactory httpClientFactory, IOptions<JasperServer> data, IHttpContextAccessor httpContextAccessor, IConectarCircunscripcionRepository repository)
    {
        _httpClient = httpClientFactory.CreateClient("JasperClient");
        _data = data.Value;
        _httpContextAccessor = httpContextAccessor;
        _repository = repository;
    }
    public async Task<string> GetBaseUrlAsync()
    {
        _base_url = null;
        string dbUser = string.Empty;
        dbUser = _httpContextAccessor.HttpContext.Request.Headers["codCircunscripcion"].ToString().Trim();
        int codCircunscripcion = int.Parse(dbUser);
        //_base_url = await _repository.ObtenerBaseDeDatosCircunscripcion(codCircunscripcion);
        _base_url = "test_administracion"; // PARA HACER PRUEBAS EN LA BD DE TEST, BORRAR PARA USAR LA BD DE VERDAD

        _base_url = $"{_data.BaseUrl}/{_data.ReportsFolder}/{_base_url}";
        return _base_url;
    }
    public async Task<byte[]> GenerarReporteFG03(FG03Request request)
    {
        

        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_codigo_version={request.CodigoVersion}",
            $"p_usuario={request.Usuario}",
            $"p_ejercicio={request.Ejercicio}"
        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/FG03_ResumenMateria.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {    
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }

    public async Task<byte[]> GenerarReporteFG07(FG07Request request)
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_codigo_version={request.CodigoVersion}",
            $"p_usuario={request.Usuario}",
            $"p_ejercicio={request.Ejercicio}"
        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/FG07.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }

    public async Task<byte[]> GenerarReporteSolicitudesBienes(SolicitudesBienesRequest request)
    {
        var baseUrl = await GetBaseUrlAsync();

        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_codigo_solicitud={request.CodigoSolicitud}",
            $"p_usuario={request.Usuario}",
            $"p_ejercicio={request.Ejercicio}"
        };

        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/SolicitudBienes.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}" + " " +url);
        }
    }

    public async Task<byte[]> GenerarReporteFG03FF(FG03FFRequest request)
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_codigo_fuente_financiamiento={request.CodigoFuenteFinanciamiento}",
            $"p_usuario={request.Usuario}",
            $"p_ejercicio={request.Ejercicio}"
            
        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/FG03_FuenteFinanciacion_Circunscripcion.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }

    public async Task<byte[]> GenerarReporteFG03FFGeneral(FG03FFGeneralRequest request)
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_codigo_fuente_financiamiento={request.CodigoFuenteFinanciamiento}",
            $"p_usuario={request.Usuario}",
            $"p_ejercicio={request.Ejercicio}",
            $"p_codigo_circunscripcion={request.CodigoCircunscripcion}"

        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/FG03_FuenteFinanciacion_General.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }

    public async Task<byte[]> GenerarReporteFG02(FG02Request request)
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_codigo_version={request.CodigoVersion}",
            $"p_usuario={request.Usuario}"
        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/FG02.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }

    public async Task<byte[]> GenerarReporteFG03_Consolidado(FG03Consolidado request)
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_codigo_version={request.CodigoVersion}",
            $"p_usuario={request.Usuario}",
            $"p_codigo_fuente_financiamiento={request.CodigoFF}",
            $"p_ejercicio={request.Ejercicio}"

        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/FG03_Consolidado.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }

    public async Task<byte[]> GenerarReporteFG02_Consolidado(FG03Consolidado request)
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_codigo_version={request.CodigoVersion}",
            $"p_usuario={request.Usuario}",
            $"p_codigo_fuente_financiamiento={request.CodigoFF}",
            $"p_ejercicio={request.Ejercicio}"

        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/FG02_Consolidado.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }

    public async Task<byte[]> GenerarReporteVersionadoAnteproyecto(ConsultaVersionadoAnteproyecto request)
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_usuario={request.Usuario}",
            $"p_numero_version1={request.NumeroVersion1}",
            $"p_numero_version2={request.NumeroVersion2}",
            $"p_ejercicio={request.Ejercicio}"

        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/consultaVersionadoAnteproyecto.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();

        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }




    public async Task<byte[]> GenerarReportePorGrupoGasto(ReportePorGrupoGasto request)
  
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",            
            $"p_usuario={request.Usuario}",            

            $"p_ejercicio={request.Ejercicio}"

        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/GrupoGastos.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }
    public async Task<byte[]> GenerarFG03FuenteFinanciacionCircunscripcion(FG03FuenteFinanciacionCircunscripcion request)
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_usuario={request.Usuario}",
            $"p_ejercicio={request.Ejercicio}",
            $"p_codigo_fuente_financiamiento={request.codigoFFC}"

        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/FG03_FuenteFinanciacion_Circunscripcion.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }

    public async Task<byte[]> GenerarJustificacionCreditosPresupuestarios(JustificacionCreditosPresupuestarios request)
    {
        var queryParams = new List<string>
        {
            $"j_username={_data.Username}",
            $"j_password={_data.Password}",
            $"p_usuario={request.Usuario}",
            $"p_ejercicio={request.Ejercicio}",
            $"p_centroResposabilidad={request.codigoCR}",
            $"p_codigoCircunscripcion={request.codigoCircunscripcion}",
            $"p_codigoMateria={request.materia}"

        };
        var baseUrl = await GetBaseUrlAsync();
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/JustificacionCreditosPresupuestarios.pdf?{queryString}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al generar el reporte: {errorMessage}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ocurrió un error al generar el reporte: {ex.Message}");
        }
    }

}
