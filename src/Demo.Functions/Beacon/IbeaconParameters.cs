using System;

namespace Demo.Functions.Beacon
{
    public class IbeaconParameters
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        
        public string MajorHex => Major.ToString("X4");
        public string MinorHex => Minor.ToString("X4");

        public Guid Uuid { get; set; }
    }
}
