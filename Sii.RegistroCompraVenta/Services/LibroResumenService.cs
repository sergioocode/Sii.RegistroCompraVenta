using System.Globalization;
using System.Text.Json;
using Sii.RegistroCompraVenta.Helper;

namespace Sii.RegistroCompraVenta.Services;

public class LibroResumenService
{
    private const string NamespaceResumen =
        "cl.sii.sdi.lob.diii.consdcv.data.api.interfaces.FacadeService/getResumen";
    private const string EndPointResumen =
        "consdcvinternetui/services/data/facadeService/getResumen";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SiiAuthenticator _authenticator;
    private readonly SiiTokenProvider _tokenProvider;

    private const string _clientName = "SII";

    public LibroResumenService(
        IHttpClientFactory httpClientFactory,
        SiiAuthenticator authenticator,
        SiiTokenProvider tokenProvider
    )
    {
        _httpClientFactory = httpClientFactory;
        _authenticator = authenticator;
        _tokenProvider = tokenProvider;
    }

    public async Task<Dictionary<string, JsonElement>> GetResumen(
        string rutEmisor,
        DateOnly periodo,
        string operacion,
        CancellationToken token = default
    )
    {
        try
        {
            await _authenticator.AutenticarAsync("https://palena.sii.cl/cgi_dte/UPL/DTEauth?1");
            string? siiToken = _tokenProvider.ObtenerToken();
            HttpClient client = _httpClientFactory.CreateClient(_clientName);
            (string rut, string dv) = ParseRut(rutEmisor);
            string[] estados =
                operacion == "VENTA" ? ["REGISTRO"] : ["REGISTRO", "RECLAMADO", "PENDIENTE"];
            IEnumerable<Task<(string estado, JsonElement parsed)>> tareas = estados.Select(
                async estado =>
                {
                    object payload = BuildPayload(rut, dv, periodo, siiToken, estado, operacion);
                    HttpResponseMessage response = await client.PostAsJsonAsync(
                        EndPointResumen,
                        payload,
                        cancellationToken: token
                    );
                    response.EnsureSuccessStatusCode();

                    string raw = await response.Content.ReadAsStringAsync(token);
                    JsonElement parsed = JsonSerializer.Deserialize<JsonElement>(raw);
                    return (estado, parsed);
                }
            );
            (string estado, JsonElement parsed)[] resultados = await Task.WhenAll(tareas);
            return resultados.ToDictionary(x => x.estado, x => x.parsed);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error de comunicación con SII: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Respuesta inválida del SII: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en GetResumen: {ex.Message}", ex);
        }
    }

    private static (string Rut, string Dv) ParseRut(string rutEmisor)
    {
        string[] parts = rutEmisor.Split('-');
        return (parts[0], parts[1]);
    }

    private static object BuildPayload(
        string rut,
        string dv,
        DateOnly periodo,
        string? token,
        string estado,
        string operacion
    )
    {
        return new
        {
            metaData = new
            {
                @namespace = NamespaceResumen,
                conversationId = token ?? string.Empty,
                transactionId = Guid.NewGuid().ToString(),
            },
            data = new
            {
                RutEmisor = rut,
                DvEmisor = dv,
                EstadoContab = estado,
                Ptributario = periodo.ToString("yyyyMM", new CultureInfo("es-CL")),
                Operacion = operacion.ToUpperInvariant(),
            },
        };
    }
}
