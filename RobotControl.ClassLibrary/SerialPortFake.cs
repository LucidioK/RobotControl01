using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;

namespace RobotControl.ClassLibrary
{
    class SerialPortFake : ISerialPort
    {
        private readonly string FakeSerialPortPath = "FakeSerialPort.json";
        private readonly int count;
        private int position = 0;
        List<string> fakeData;
        private Action<string> onDataReceivedCallback;
        private Action<Exception> onExceptionCallback;

        public SerialPortFake()
        {
            if (File.Exists(FakeSerialPortPath))
            {
                JsonConvert
                    .DeserializeObject<List<RobotData>>(
                        File.ReadAllText(FakeSerialPortPath))
                    .ForEach(r => fakeData.Add(JsonConvert.SerializeObject(r)));
            }
            else
            {
                throw new Exception($"Cannot find file {FakeSerialPortPath}");
            }
            count = (int)fakeData.Count;
        }

        public bool Open(int portNumber, int baudRate, Action<string> onDataReceivedCallback, Action<Exception> onExceptionCallback)
        {
            this.onDataReceivedCallback = onDataReceivedCallback;
            this.onExceptionCallback = onExceptionCallback;
            return true;
        }

        public void Write(string s)      =>
            Console.WriteLine($"{nameof(SerialPortFake)} would have written {s}");

        private int NextPosition()       =>
            (position = position < count - 1 ? position + 1 : 0);
    }
}
