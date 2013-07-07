using System;
using Microsoft.SPOT;

namespace AgentWatchApplication1.Hardware.Bluetooth
{
    public abstract class BaseChannel : IChannel
    {
        /// <summary>
        /// Get a string from the serial port.
        /// NOTE: This may not be the most efficient way of receiving strings from the serial port.
        /// </summary>
        /// <returns>The string that has been sent by the other application</returns>
        protected string GetString(System.IO.Ports.SerialPort port)
        {
            var ret = string.Empty;

            var latestBype = port.ReadByte();

            //Keep getting data until the latest byte is a zero byte
            while (latestBype != 0)
            {
                var character = (char) latestBype;

                ret += character;

                latestBype = port.ReadByte();
            }
            return ret;
        }

        public abstract object Read(System.IO.Ports.SerialPort port);
    }
}