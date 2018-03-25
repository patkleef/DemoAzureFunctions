using System.Globalization;

namespace Demo.Functions.Models
{
    public class GeoCoordinates
    {
        public GeoCoordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override string ToString()
        {
            var nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            return $"{Latitude.ToString(nfi)},{Longitude.ToString(nfi)}";
        }
    }
}
