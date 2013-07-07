using System;
using AgentWatchApplication1.Hardware.Bluetooth;
using Microsoft.SPOT;

namespace AgentWatchApplication1
{
    public class CoordinateChannel : BaseChannel
    {
        public override object Read(System.IO.Ports.SerialPort port)
        {
            var c = new Coordinate();
            c.Action = Action.Hello;
            
            var csv = new CSVChannel();
            string raw = (csv.Read(port) as string);
            string[] parts = raw.Split(',');
            double val = 0;
            var action = parts[0];
            if (action == "Data" || action == "Start" || action == "Stop")
            {
                if (parts[1] != "NaN" && double.TryParse(parts[1], out val)) c.Altitude = val;
                if (parts[2] != "NaN" && double.TryParse(parts[2], out val)) c.Course = val;
                if (parts[3] != "NaN" && double.TryParse(parts[3], out val)) c.HorizontalAccuracy = val;
                if (parts[4] != "NaN" && double.TryParse(parts[4], out val)) c.Latitude = val;
                if (parts[5] != "NaN" && double.TryParse(parts[5], out val)) c.Longitude = val;
                if (parts[6] != "NaN" && double.TryParse(parts[6], out val)) c.Speed = val;
                if (parts[7] != "NaN" && double.TryParse(parts[7], out val)) c.VerticalAccuracy = val;

                c.Pace = parts[8];
                c.Distance = parts[9];
                c.Calories = parts[10];
                c.Time = parts[11];

                if (action == "Start")
                {
                    c.Action = Action.Start;
                }
                else if (action == "Stop")
                {
                    c.Action = Action.Stop;
                } else if (action == "Data")
                {
                    c.Action = Action.Data;
                }
            }
            else if (action == "Hello")
            {
                c.Action = Action.Hello;
            }
            
            return c;
        }
    }
}
