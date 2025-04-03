using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Sii.RegistroCompraVenta.Services;

namespace Sii.RegistroCompraVenta.Controllers;

[ApiController]
[Route("api/RegistroCompraVenta")]
public class RegistroCompraVenta : Controller
{
    private readonly LibroCompraServie libroCompra;

    public RegistroCompraVenta(LibroCompraServie libroCompra)
    {
        this.libroCompra = libroCompra;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen(
        [FromQuery] string? rut,
        [FromQuery] int? anio,
        [FromQuery] int? mes,
        [FromQuery] string? operacion,
        CancellationToken ct = default
    )
    {
        object validacion = ValidarParametros(rut, anio, mes, operacion);
        if (validacion is string error)
            return BadRequest(error);

        (string rutOk, DateOnly periodoOk, string operacionOk) = ((
            string,
            DateOnly,
            string
        ))validacion;

        Dictionary<string, JsonElement> data = await libroCompra.GetResumen(
            rutOk,
            periodoOk,
            operacionOk,
            ct
        );
        return Ok(data);
    }

    private object ValidarParametros(string? rut, int? anio, int? mes, string? operacion)
    {
        if (string.IsNullOrWhiteSpace(rut) || !Regex.IsMatch(rut, @"^\d{7,8}-[0-9kK]$"))
            return "El parámetro 'rut' es obligatorio y debe tener el formato ########-X.";

        if (anio is null || mes is null)
            return "Debe especificar los parámetros 'anio' y 'mes'.";

        DateOnly periodo;
        try
        {
            periodo = new(anio.Value, mes.Value, 1);
        }
        catch
        {
            return "El período especificado no es válido.";
        }

        DateOnly hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly limiteInferior = hoy.AddYears(-2).AddDays(1 - hoy.Day);

        if (periodo < limiteInferior || periodo > hoy)
            return $"El período debe estar entre {limiteInferior:yyyy-MM} y {hoy:yyyy-MM}.";

        if (string.IsNullOrWhiteSpace(operacion))
            return "El parámetro 'operacion' es obligatorio.";

        string op = operacion.ToUpperInvariant();
        return op is not "COMPRA" and not "VENTA"
            ? "El parámetro 'operacion' solo puede ser 'COMPRA' o 'VENTA'."
            : (rut.Trim(), periodo, op);
    }
}
