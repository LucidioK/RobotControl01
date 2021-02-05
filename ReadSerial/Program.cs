using RobotControl.ClassLibrary;

using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;

namespace ReadSerial
{
    class Program
    {
        static void Main(string[] args)
        {
            var re = new Regex("^([0-9]+)$");
            if (args == null || args.Length != 2 || !re.IsMatch(args[0]) || !re.IsMatch(args[1]))
            {
                Console.WriteLine("ReadSerial SerialPort BaudRate\nExample:\nReadSerial 3 115200\n");
                Environment.Exit(1);
            }
            ISerialPort sp = new SerialPortImpl();
            sp.Open(int.Parse(args[0]), int.Parse(args[1]), onDataReceived, onException);

            while (true)
            {
                Thread.Sleep(10);
            }
        }

        private static void onException(Exception ex) =>
            Console.WriteLine($"EX> {ex.Message}");

        private static void onDataReceived(string data) =>
            Console.WriteLine($"DT> {data}");
    }
}
