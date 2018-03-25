using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Demo.Functions.ImageBlobStorage
{
    public class StorageService : IStorageService
    {
        private readonly string _containerName = "place-images";

        private CloudStorageAccount _storageAccount => CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

        public async Task<string> UploadImageToBlobStorage(byte[] bytes, string blobName)
        {
            var cloudBlobClient = _storageAccount.CreateCloudBlobClient();
            
            var cloudBlobContainer = await CreateBlobContainerIfNotExists(cloudBlobClient, _containerName);

            var blockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
            await blockBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);

            var filePathOrg = $"{cloudBlobClient.BaseUri}{_containerName}/{blobName}";

            return filePathOrg;
        }

        public async Task<CloudBlobContainer> CreateBlobContainerIfNotExists(CloudBlobClient cloudBlobClient, string name)
        {
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(name);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            var permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            await cloudBlobContainer.SetPermissionsAsync(permissions);

            return cloudBlobContainer;
        }

        public CloudBlockBlob GetBlob(string fileName)
        {
            var cloudBlobClient = _storageAccount.CreateCloudBlobClient();

            var container = cloudBlobClient.GetContainerReference(_containerName);

            return container.GetBlockBlobReference(fileName);
        }
    }
}
