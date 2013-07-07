using System;
using System.IO.Ports;
using Agent.Contrib.Drawing;
using Agent.Contrib.Hardware;
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
        static Font font = Resources.GetFont(Resources.FontResources.SegoeUIPaceFont);
            
        public static void Main()
        {
            ButtonHelper.ButtonSetup = new Buttons[]{Buttons.BottomRight, Buttons.MiddleRight, Buttons.TopRight,  };
            ButtonHelper.Current.OnButtonPress += Current_OnButtonPress;
            // initialize display buffer
            _display = new Bitmap(Bitmap.MaxWidth, Bitmap.MaxHeight);

            // sample "hello world" code
            _display.Clear();
            var drawing = new Agent.Contrib.Drawing.Drawing();
            drawing.DrawAlignedText(_display, Color.White, font, "Connect...", HAlign.Center, 0, VAlign.Middle, 0);
            _display.Flush();
            

            p = new SerialPort("COM1");
            p.DataReceived += p_DataReceived;
            p.Open();




            // go to sleep; all further code should be timer-driven or event-driven
            Thread.Sleep(Timeout.Infinite);
        }

        private static void Current_OnButtonPress(Buttons button, InterruptPort port, ButtonDirection direction,
                                                  DateTime time)
        {
            if (direction == ButtonDirection.Up)
            {
                if (button == Buttons.TopRight)
                {
                    //send toggle pause/resume
                    p.Write(new byte[] { 80 }, 0, 1);
                }
                if (button == Buttons.BottomRight)
                {
                    //send toggle start/stop
                    p.Write(new byte[] { 83 }, 0, 1);
                }
                
            }
        }

        private static Action _startStopState = Action.Stop;
        private static SerialPort p;
        private static CoordinateChannel channel = new CoordinateChannel();
        static Coordinate LastResult = new Coordinate();
        static void p_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            LastResult = (channel.Read(p) as Coordinate);
            _display.Clear();
            var drawing = new Agent.Contrib.Drawing.Drawing();
            if (LastResult.Action == Action.Data || LastResult.Action == Action.Start || LastResult.Action == Action.Stop)
            {
                drawing.DrawAlignedText(_display, Color.White, font, LastResult.Time, HAlign.Center, 0, VAlign.Top, 0);
                drawing.DrawAlignedText(_display, Color.White, font, LastResult.Distance, HAlign.Center, 0, VAlign.Middle, 0);
                drawing.DrawAlignedText(_display, Color.White, font, LastResult.Pace, HAlign.Center, 0, VAlign.Bottom, 0);

                //_display.DrawText("Calories:" + LastResult.Calories, fontNinaB, Color.White, 1, fontNinaB.Height*2);
                if (LastResult.Action == Action.Start)
                {
                    _startStopState = Action.Start;
                }
                if (LastResult.Action == Action.Stop)
                {
                    _startStopState = Action.Stop;
                }

            }
            else if (LastResult.Action == Action.Hello)
            {
                drawing.DrawAlignedText(_display, Color.White, font, "Connected!", HAlign.Center, 0, VAlign.Middle, 0);
                
            }
            if (_startStopState == Action.Start)
            {
                //_display.DrawText("State: Started", font, Color.White, 1, top);
            }
            if (_startStopState == Action.Stop)
            {
                //_display.DrawText("State: Stopped", font, Color.White, 1, top);
            }

            _display.Flush();


        }

    }
}
