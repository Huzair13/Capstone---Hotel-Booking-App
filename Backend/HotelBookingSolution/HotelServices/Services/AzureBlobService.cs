using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HotelServices.Interfaces;

namespace HotelServices.Services
{
    public class AzureBlobService : IAzureBlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobService(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _containerName = containerName;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            await blobContainerClient.CreateIfNotExistsAsync();
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "image/jpeg" });

            return blobClient.Uri.ToString();
        }
    }
}
