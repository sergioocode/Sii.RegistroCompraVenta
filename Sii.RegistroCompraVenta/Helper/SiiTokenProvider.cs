using System.Net;

namespace Sii.RegistroCompraVenta.Helper;

public class SiiTokenProvider
{
    private readonly CookieContainer _cookieContainer;

    public SiiTokenProvider(CookieContainer cookieContainer)
    {
        _cookieContainer = cookieContainer;
    }

    public static class SiiHosts
    {
        public static readonly string[] CookieSources =
        {
            "www4.sii.cl",
            "herculesr.sii.cl",
            "palena.sii.cl",
            "zeusr.sii.cl",
        };
    }

    public string? ObtenerToken()
    {
        foreach (string host in SiiHosts.CookieSources)
        {
            CookieCollection cookies = _cookieContainer.GetCookies(new Uri($"https://{host}"));
            string? token = cookies["TOKEN"]?.Value;
            if (!string.IsNullOrEmpty(token))
                return token;
        }

        return null;
    }
}
