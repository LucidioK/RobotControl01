﻿using System;
using System.IO.Ports;
using System.Threading;

namespace RobotControl.Net
{
    class SerialPortImpl : ISerialPort
    {
        SerialPort serialPort = null;

        Action<string> onDataReceivedCallback;
        object serialPortLock = new object();
        object getSerialPortLock = new object();
        int portNumber;
        int baudRate;
        private Thread thread;

        public bool Open(int portNumber, int baudRate, Action<string> onDataReceivedCallback)
        {
            this.onDataReceivedCallback = onDataReceivedCallback;
            try
            {
                this.portNumber = portNumber;
                this.baudRate = baudRate;
                lock (serialPortLock)
                {
                    GetSerialPort();
                }

                thread = new Thread(SerialReadThread);
                thread.Start();

                return true;
            }
            catch (Exception)
            {
                serialPort?.Close();
                serialPort = null;
                return false;
            }
        }

        private void SerialReadThread()
        {
            string s = string.Empty;

            while (true)
            {
                try
                {
                    lock (serialPortLock)
                    {
                        s = GetSerialPort().ReadLine();
                    }
                    onDataReceivedCallback.Invoke(s);
                }
                catch (TimeoutException)
                {
                    System.Diagnostics.Debug.WriteLine("-->SerialPortImpl.SerialReadThread TIMEOUT");
                }
            }
        }


        public void Write(string s)
        {
            lock (serialPortLock)
            {
                GetSerialPort()?.Write(s);
            }
        }

        public SerialPort GetSerialPort()
        {
            lock (getSerialPortLock)
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    serialPort.Close();
                }
                serialPort = new SerialPort($"COM{portNumber}", baudRate);

                // this seems to be important for Arduino:
                serialPort.RtsEnable = true;
                serialPort.ReadTimeout = 5000;
                for (var i = 0; !serialPort.IsOpen && i < 8; i++)
                {
                    try
                    {
                        serialPort.Open();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        System.Diagnostics.Debug.WriteLine("-->SerialPortImpl.GetSerialPort UnauthorizedAccessException, retrying");
                    }
                }

                if (serialPort.IsOpen)
                {
                    return serialPort;
                }
                else
                {
                    throw new Exception($"Cannot open COM{portNumber}");
                }
            }
        }
    }
}
