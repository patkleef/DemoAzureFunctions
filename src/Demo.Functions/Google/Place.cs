using System.Collections.Generic;

namespace Demo.Functions.Google
{
    public class Place
    {
        public string Name { get; set; }
        public IEnumerable<PlaceImage> Photos { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Rating { get; set; }
        public string[] Types { get; set; }
        public string Address { get; set; }
    }
}
