using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo.Functions.Logging;
using Demo.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Refit;

namespace Demo.Functions.Google
{
    public static class GooglePlacesFunction
    {
        private static readonly string _placesTypes = "point_of_interest";
        private static readonly int _placesRadius = 100;
        private static string _googleApiKey => Environment.GetEnvironmentVariable("GoogleApiKey", EnvironmentVariableTarget.Process);

        private static IGoogleService _googleService => RestService.For<IGoogleService>("https://maps.googleapis.com");

        [FunctionName("GooglePlacesFunction")]
        [return: Queue("search-images-queue", Connection = "AzureWebJobsStorage")]
        public static async Task<LocationInfo> Run(
            [QueueTrigger("location-queue", Connection = "AzureWebJobsStorage")]LocationInfo locationInfo,
            TraceWriter log)
        {
            var result = await _googleService.NearBy(locationInfo.Coordinates.ToString(), _placesRadius, _placesTypes, _googleApiKey);

            var list = new List<Place>();
            foreach (var item in result.results.Take(5))
            {
                var place = new Place();
                place.Name = item.name;
                place.Latitude = item.geometry.location.lat;
                place.Longitude = item.geometry.location.lng;
                place.Rating = item.rating;
                place.Address = item.vicinity;
                place.Types = item.types;

                list.Add(place);
            }
            locationInfo.Places = list;
            
            LogHelper.Log(log, $"GooglePlacesFunction called: {Newtonsoft.Json.JsonConvert.SerializeObject(list)}");

            return locationInfo;
        }
    }
}
