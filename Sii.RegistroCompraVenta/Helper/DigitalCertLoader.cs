using System.Net;
using System.Security.Cryptography.X509Certificates;
using Azure.Storage.Blobs;

namespace Sii.RegistroCompraVenta.Helper;

public class DigitalCertLoader
{
    public static async Task<HttpClientHandler> LoadCertificateAsync(IConfiguration config)
    {
        string connectionString = config["StorageConnection"]!;
        string containerName = config["StorageConnection:containerName"]!;
        string blobName = config["StorageConnection:blobName"]!;
        string password = config["StorageConnection:certPassword"]!;
        try
        {
            BlobServiceClient blobClient = new(connectionString);
            BlobContainerClient containerClient = blobClient.GetBlobContainerClient(containerName);
            BlobClient blob = containerClient.GetBlobClient(blobName);

            using MemoryStream ms = new();
            await blob.DownloadToAsync(ms);

            X509Certificate2 cert = new(ms.ToArray(), password, X509KeyStorageFlags.Exportable);

            CookieContainer cookieContainer = new();
            HttpClientHandler handler = new() { CookieContainer = cookieContainer };
            handler.ClientCertificates.Add(cert);
            return handler;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
