using System;
using System.IO.Ports;

namespace RobotControl.ClassLibrary
{
    public interface ISerialPort
    {
        bool Open(
            int portNumber, 
            int baudRate, 
            Action<string> onDataReceivedCallback,
            Action<Exception> onExceptionCallback);
        void Write(string s);
    }
}
