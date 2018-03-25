using System;
using System.Collections.Generic;
using Demo.Functions.Beacon;
using Demo.Functions.Google;

namespace Demo.Functions.Models
{
    public class LocationInfo
    {
        public BeaconData Beacon { get; set; }
        public string Name { get; set; }
        public GeoCoordinates Coordinates { get; set; }
        public string Address { get; set; }
        public IEnumerable<Place> Places { get; set; }
        public DateTime Date { get; set; }
    }
}
