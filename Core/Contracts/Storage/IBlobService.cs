namespace AppServices.Contracts.Storage;

public interface IBlobService
{
    Task<string> UploadAsync(string containerName, string blobName, Stream data, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
}
