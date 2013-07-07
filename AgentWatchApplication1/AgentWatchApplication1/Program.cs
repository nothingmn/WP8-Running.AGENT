using System;
using System.IO.Ports;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using System.Threading;

namespace AgentWatchApplication1
{
    public class Program
    {
        static Bitmap _display;
        static Font fontNinaB = Resources.GetFont(Resources.FontResources.NinaB);
            
        public static void Main()
        {
            // initialize display buffer
            _display = new Bitmap(Bitmap.MaxWidth, Bitmap.MaxHeight);

            // sample "hello world" code
            _display.Clear();
            _display.DrawText("Waiting on connect...", fontNinaB, Color.White, 10, 64);
            _display.Flush();
            

            p = new SerialPort("COM1");
            p.DataReceived += p_DataReceived;
            p.Open();




            // go to sleep; all further code should be timer-driven or event-driven
            Thread.Sleep(Timeout.Infinite);
        }

        private static SerialPort p;
        private static CoordinateChannel channel = new CoordinateChannel();
        static Coordinate LastResult = new Coordinate();
        static void p_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            LastResult = (channel.Read(p) as Coordinate);

            _display.Clear();
            if (LastResult.HasData)
            {
                _display.DrawText("Pace:" + LastResult.Pace, fontNinaB, Color.White, 1, 1);
                _display.DrawText("Distance:" + LastResult.Distance, fontNinaB, Color.White, 1, fontNinaB.Height);
                _display.DrawText("Calories:" + LastResult.Calories, fontNinaB, Color.White, 1, fontNinaB.Height*2);
                _display.DrawText("Time:" + LastResult.Time, fontNinaB, Color.White, 1, fontNinaB.Height*3);
            }
            else
            {
                _display.DrawText("Connected!" + LastResult.Pace, fontNinaB, Color.White, 1, 1);
                
            }
            _display.Flush();


        }

    }
}
