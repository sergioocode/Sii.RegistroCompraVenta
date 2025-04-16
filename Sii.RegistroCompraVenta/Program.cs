using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Azure;
using Sii.RegistroCompraVenta.Helper;
using Sii.RegistroCompraVenta.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DigitalCertLoader>();
builder.Services.AddSingleton<SiiAuthenticator>();
builder.Services.AddSingleton<SiiTokenProvider>();
builder.Services.AddSingleton<LibroResumenService>();

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnection"]);
});

CookieContainer sharedCookieContainer = new();
builder.Services.AddSingleton(sharedCookieContainer);
builder
    .Services.AddHttpClient(
        "SII",
        c =>
        {
            c.BaseAddress = new Uri("https://www4.sii.cl/");
        }
    )
    .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
    {
        DigitalCertLoader certLoader = serviceProvider.GetRequiredService<DigitalCertLoader>();
        X509Certificate2 cert = certLoader.LoadCertificateAsync().GetAwaiter().GetResult();
        CookieContainer container = serviceProvider.GetRequiredService<CookieContainer>();
        return new HttpClientHandler { CookieContainer = container, ClientCertificates = { cert } };
    });

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Sii.RegistroCompraVenta", Version = "v1" });
    c.EnableAnnotations();
});

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
