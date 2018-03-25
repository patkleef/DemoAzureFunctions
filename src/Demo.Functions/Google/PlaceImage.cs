using System.IO;

namespace Demo.Functions.Google
{
    public class PlaceImage
    {
        public string Filename => !string.IsNullOrEmpty(Url) ? Path.GetFileName(Url) : string.Empty;
        public string Url { get; set; }
        public string[] Tags { get; set; }
        public string[] Captions { get; set; }
        public string[] Categories { get; set; }
    }
}
