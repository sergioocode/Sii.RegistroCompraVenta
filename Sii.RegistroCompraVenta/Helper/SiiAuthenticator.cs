namespace Sii.RegistroCompraVenta.Helper;

public class SiiAuthenticator
{
    private bool _isAuthenticated;
    private readonly IHttpClientFactory _httpClientFactory;

    public SiiAuthenticator(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task AutenticarAsync(string referenciaUrl)
    {
        if (_isAuthenticated)
            return;

        HttpClient client = _httpClientFactory.CreateClient("SII");
        HttpResponseMessage response = await client.PostAsync(
            "https://herculesr.sii.cl/cgi_AUT2000/CAutInicio.cgi",
            new FormUrlEncodedContent(
                [new KeyValuePair<string, string>("referencia", referenciaUrl)]
            )
        );
        response.EnsureSuccessStatusCode();
        _isAuthenticated = true;
    }
}
