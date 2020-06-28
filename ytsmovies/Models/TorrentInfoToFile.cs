using System;
using System.Collections.Generic;
using System.Text;

namespace ytsmovies.Models
{
    public class TorrentInfoToFile
    {
        public string dateAdded { get; set; } = "";
        public string status { get; set; } = "";
        public string imageUrl { get; set; } = "";
        public string genre { get; set; } = "";
        public string year { get; set; } = "";
        public string duration { get; set; } = "";
        public string resolution { get; set; } = "";
        public string rating { get; set; } = "";
        public string language { get; set; } = "";
        public string torrentFileName { get; set; } = "";
        public string torrentInfoFileName { get; set; } = "";
    }
}
