using System;
using Microsoft.SPOT;

namespace AgentWatchApplication1
{
    public class Coordinate
    {
        public double Altitude { get; set; }
        public double Course { get; set; }
        public double HorizontalAccuracy { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public double VerticalAccuracy { get; set; }

        public string Pace { get; set; }
        public string Distance { get; set; }
        public string Calories { get; set; }
        public string Time { get; set; }

        public bool HasData { get; set; }
    }
}
