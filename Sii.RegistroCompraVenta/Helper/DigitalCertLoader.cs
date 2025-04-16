using System.Security.Cryptography.X509Certificates;
using Azure.Storage.Blobs;

namespace Sii.RegistroCompraVenta.Helper;

public class DigitalCertLoader
{
    private readonly IConfiguration _config;
    private readonly BlobServiceClient _blobClient;

    public DigitalCertLoader(IConfiguration config, BlobServiceClient blobClient)
    {
        _config = config;
        _blobClient = blobClient;
    }

    public async Task<X509Certificate2> LoadCertificateAsync()
    {
        try
        {
            string containerName = _config.GetValue<string>("StorageConnection:containerName")!;
            string blobName = _config.GetValue<string>("StorageConnection:blobName")!;
            string password = _config.GetValue<string>("StorageConnection:certPassword")!;

            BlobContainerClient containerClient = _blobClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            using MemoryStream ms = new();
            await blobClient.DownloadToAsync(ms);

            return new X509Certificate2(ms.ToArray(), password, X509KeyStorageFlags.MachineKeySet);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al cargar el certificado digital: {ex.Message}", ex);
        }
    }
}
