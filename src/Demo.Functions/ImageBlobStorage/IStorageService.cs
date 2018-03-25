using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Demo.Functions.ImageBlobStorage
{
    public interface IStorageService
    {
        Task<string> UploadImageToBlobStorage(byte[] bytes, string blobName);
        Task<CloudBlobContainer> CreateBlobContainerIfNotExists(CloudBlobClient cloudBlobClient, string name);

        CloudBlockBlob GetBlob(string fileName);
    }
}