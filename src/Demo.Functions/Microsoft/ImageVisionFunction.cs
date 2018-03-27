using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Demo.Functions.ImageBlobStorage;
using Demo.Functions.Logging;
using Demo.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Refit;

namespace Demo.Functions.Microsoft
{
    public static class ImageVisionFunction
    {
        private static string _imageVisionServiceApiKey => Environment.GetEnvironmentVariable("ImageVisionServiceApiKey", EnvironmentVariableTarget.Process);
        private static IImageService _imageService => RestService.For<IImageService>(_cognitiveServiceBaseUrl);
        private static readonly string _cognitiveServiceBaseUrl = "https://westeurope.api.cognitive.microsoft.com";
        private static readonly IStorageService _storageService = new StorageService();

        [FunctionName("ImageVisionApi")]
        [return: Queue("completed-queue", Connection = "AzureWebJobsStorage")]
        public static async Task<LocationInfo> Run(
            [QueueTrigger("image-vision-queue", Connection = "AzureWebJobsStorage")]LocationInfo locationInfo, 
            TraceWriter log)
        {
            foreach (var place in locationInfo.Places)
            {
                foreach (var image in place.Photos)
                {
                    var cloudBlockBlob = _storageService.GetBlob(image.Filename);
                    if (cloudBlockBlob != null)
                    {
                        using (var stream = new MemoryStream())
                        {
                            cloudBlockBlob.DownloadToStream(stream);

                            stream.Seek(0, SeekOrigin.Begin);

                            try
                            {
                                var response = await _imageService.GetInfo(stream, _imageVisionServiceApiKey);
                                if (response != null)
                                {
                                    image.Tags = response.description?.tags;
                                    image.Captions = response.description?.captions?.Select(x => x.text).ToArray();
                                    image.Categories = response.categories?.Select(x => x.name).ToArray();
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Log(log, $"ImageVisionFunction function exception occurred: {ex.Message}");

                                return null;
                            }
                        }
                    }
                }
            }
            LogHelper.Log(log, $"ImageVisionFunction function processed: {Newtonsoft.Json.JsonConvert.SerializeObject(locationInfo)}");

            return locationInfo;
        }
    }
}
