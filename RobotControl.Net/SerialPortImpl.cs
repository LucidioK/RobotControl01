using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl.Net
{
    class SerialPortImpl : ISerialPort
    {
        SerialPort serialPort;
        public bool Open(int portNumber)
        {
            try
            {
                serialPort = new SerialPort($"COM{portNumber}", 9600);
                serialPort.Open();
                return true;
            }
            catch (Exception)
            {
                serialPort = null;
                return false;
            }
        }

        public string ReadExisting() => serialPort?.ReadExisting();

        public string ReadLine() => serialPort?.ReadLine();

        public void Write(string s) => serialPort?.Write(s);
    }
}
