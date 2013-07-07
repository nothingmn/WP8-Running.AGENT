using System;
using Microsoft.SPOT;

namespace AgentWatchApplication1.Hardware.Bluetooth
{
    public class CSVChannel : BaseChannel
    {

        public override object Read(System.IO.Ports.SerialPort port)
        {
            string result = (base.GetString(port) as string);
            Debug.Print("Received CSV:" + result);
            return result;

        }
    }
}
