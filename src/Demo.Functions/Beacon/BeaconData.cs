using Newtonsoft.Json;

namespace Demo.Functions.Beacon
{
    public class BeaconData
    {
        public string BeaconAddress { get; set; }
        public string BeaconType { get; set; }
        [JsonProperty("ibeaconData")]
        public Paramaters Data { get; set; }
        public bool IsBlocked { get; set; }
        public long LastMinuteSeen { get; set; }
        public long LastSeen { get; set; }
        public decimal Distance { get; set; }
        public int Rssi { get; set; }
        public int Hashcode { get; set; }
        public int Manufacturer { get; set; }
        public int TxPower { get; set; }
    }
}
