using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDayFrontend.Services
{
    public class AzureBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "athlete-profiles"; // Distinct container name

        public AzureBlobService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadProfilePictureAsync(IFormFile file, string athleteEmail)
        {
            if (file == null || file.Length == 0) return null!;

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Create a unique file name using the athlete's email and original extension
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{athleteEmail.Replace("@", "-").Replace(".", "-")}-profile{extension}";

            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            using (var stream = file.OpenReadStream())
            {
                // Overwrite if it exists
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                });
            }

            return blobClient.Uri.ToString();
        }
    }
}