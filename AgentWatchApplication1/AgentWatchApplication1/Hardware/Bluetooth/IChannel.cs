using System;
using System.IO.Ports;
using Microsoft.SPOT;

namespace AgentWatchApplication1.Hardware.Bluetooth
{
    public interface IChannel
    {
        object Read(SerialPort port);

    }
}
