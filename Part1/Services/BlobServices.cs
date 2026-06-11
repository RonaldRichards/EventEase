using Azure.Storage.Blobs;

public class BlobService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobService(IConfiguration config)
    {
        _connectionString = config["AzureBlobStorage:ConnectionString"];
        _containerName = config["AzureBlobStorage:ContainerName"];
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var container = new BlobContainerClient(_connectionString, _containerName);
        await container.CreateIfNotExistsAsync();

        var blob = container.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

        using (var stream = file.OpenReadStream())
        {
            await blob.UploadAsync(stream);
        }

        return blob.Uri.ToString();
    }
}