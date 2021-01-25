using System;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace ReadSerial
{
    class Program
    {
        static void Main(string[] args)
        {
            var re = new Regex("^(COM[0-9]+|com[0-9]+)$");
            if (args == null || args.Length == 0 || !re.IsMatch(args[0]))
            {
                Console.WriteLine("ReadSerial SerialPort\nExample:\nReadSerial COM3\n");
                Environment.Exit(1);
            }
            var sp = new SerialPort(args[0]);
            sp.ReadTimeout = 1000;
            sp.Open();
            while (true)
            {
                string s = sp.ReadLine();
                Console.WriteLine(s);
            }
        }
    }
}
