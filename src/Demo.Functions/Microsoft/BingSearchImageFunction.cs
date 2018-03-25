using System;
using System.Threading.Tasks;
using Demo.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Refit;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Demo.Functions.Google;
using Demo.Functions.ImageBlobStorage;
using Demo.Functions.Logging;

namespace Demo.Functions.Microsoft
{
    public static class BingSearchImageFunction
    {
        private static string _bingImageServiceApiKey => Environment.GetEnvironmentVariable("BingImageServiceApiKey", EnvironmentVariableTarget.Process);
        private static readonly string _cognitiveServiceBaseUrl = "https://api.cognitive.microsoft.com";
        private static IImageService _imageService => RestService.For<IImageService>(_cognitiveServiceBaseUrl);
        private static IStorageService _storageService = new StorageService();

        [FunctionName("BingSearchImageFunction")]
        [return: Queue("image-vision-queue", Connection = "AzureWebJobsStorage")]
        public static async Task<LocationInfo> Run(
            [QueueTrigger("search-images-queue", Connection = "AzureWebJobsStorage")]LocationInfo locationInfo, 
            TraceWriter log)
        {
            var number = 0;
            foreach (  var place in locationInfo.Places)
            {
                var result = await _imageService.Search(place.Address, _bingImageServiceApiKey);

                var list = new List<string>();
                if (result != null && result.value != null)
                {
                    foreach (var item in result.value.Take(3))
                    {
                        var extension = Path.GetExtension(item.contentUrl);
                        var fileName = $"{item.imageId}{extension}";

                        var data = await DownloadImage(item.contentUrl);

                        var res = await _storageService.UploadImageToBlobStorage(data, fileName);
                        number++;

                        list.Add(res);
                    }
                    place.Photos = list.Select(x => new PlaceImage { Url = x });
                }
            }
            LogHelper.Log(log, $"BingSearchImageFunction called: {number} images uploaded to blob storage");

            return locationInfo;
        }

        private static async Task<byte[]> DownloadImage(string url)
        {
            using (var client = new WebClient())
            {
                return await client.DownloadDataTaskAsync(url);
            }
        }
    }
}
