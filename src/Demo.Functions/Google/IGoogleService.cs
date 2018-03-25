using System.Threading.Tasks;
using Refit;

namespace Demo.Functions.Google
{
    public interface IGoogleService
    {
        [Get("/maps/api/place/nearbysearch/json")]
        Task<PlacesResponse> NearBy(string location, int radius, string types, string key);

        [Get("/maps/api/geocode/json")]
        Task<GeocodingResponse> ReverseGeocoding(string latlng, string key);
    }
}
