using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;

namespace RobotControl.Net
{
    class SerialPortFake : ISerialPort
    {
        private readonly string FakeSerialPortPath = "FakeSerialPort.json";
        private readonly int count;
        private int position = 0;
        List<string> fakeData;
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

        public bool Open(int portNumber) => true;


        public string ReadExisting()     => 
            fakeData[NextPosition()];

        public string ReadLine()         => 
            ReadExisting();

        public void Write(string s)      => 
            Console.WriteLine($"{nameof(SerialPortFake)} would have written {s}");

        private int NextPosition()       => 
            (position = position < count - 1 ? position + 1 : 0);
    }
}
