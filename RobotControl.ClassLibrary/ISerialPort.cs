using System;
using System.IO.Ports;

namespace RobotControl.Net
{
    public interface ISerialPort
    {
        bool Open(int portNumber, int baudRate, Action<string> onDataReceivedCallback);
        void Write(string s);
    }
}
