using Sii.RegistroCompraVenta.Helper;
using Sii.RegistroCompraVenta.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<SiiAuthenticator>();
builder.Services.AddSingleton<LibroResumenService>();

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
        IConfiguration config = serviceProvider.GetRequiredService<IConfiguration>();
        return DigitalCertLoader.LoadCertificateAsync(config).GetAwaiter().GetResult();
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
