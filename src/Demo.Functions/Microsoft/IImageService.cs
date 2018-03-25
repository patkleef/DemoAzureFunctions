using System.IO;
using System.Threading.Tasks;
using Refit;

namespace Demo.Functions.Microsoft
{
    public interface IImageService
    {
        [Get("/bing/v7.0/images/search")]
        Task<SearchImagesResponse> Search(string q, [Header("Ocp-Apim-Subscription-Key")] string key);
            
        [Post("/vision/v1.0/analyze?visualFeatures=Categories,Description,Color&language=en")]
        [Headers("Content-Type: application/octet-stream; charset=UTF-8")]
        Task<ImageVisionResponse> GetInfo(Stream stream, [Header("Ocp-Apim-Subscription-Key")] string key);
    }
}
