using RobotControl.ClassLibrary;

using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;

namespace RobotControl.ClassLibrary
{
    class SerialPortImpl : ISerialPort
    {

        SerialPort serialPort = null;

        Action<string> onDataReceivedCallback;
        private Action<Exception> onExceptionCallback;
        object serialPortLock = new object();

        int portNumber;
        int baudRate;
        private Thread thread;
        private ConcurrentQueue<string> writeBuffer = new ConcurrentQueue<string>();

        public bool Open(int portNumber, int baudRate, Action<string> onDataReceivedCallback, Action<Exception> onExceptionCallback)
        {
            this.onDataReceivedCallback = onDataReceivedCallback;
            this.onExceptionCallback = onExceptionCallback;
            try
            {
                this.portNumber = portNumber;
                this.baudRate = baudRate;
                lock (serialPortLock)
                {
                    GetSerialPort();
                }

                thread = new Thread(SerialThread);
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

        private void SerialThread()
        {
            string input = string.Empty;
            string output = string.Empty;
            while (true)
            {
                try
                {
                    lock (serialPortLock)
                    {
                        //var serialPort = GetSerialPort();
                        if (writeBuffer.TryDequeue(out output))
                        {
                            serialPort.WriteLine(output);
                        }
                        input = serialPort.ReadLine();
                    }

                    if (!string.IsNullOrEmpty(input))
                    {
                        onDataReceivedCallback.Invoke(input);
                    }
                }
                catch (TimeoutException)
                {
                    lock (serialPortLock)
                    {
                        GetSerialPort();
                    }
                    System.Diagnostics.Debug.WriteLine("-->SerialPortImpl.SerialReadThread TIMEOUT");
                }
                catch (Exception ex)
                {
                    if (onExceptionCallback != null)
                    {
                        onExceptionCallback(ex);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
        }


        public void Write(string s)
        {
            writeBuffer.Enqueue(s);
        }

        public SerialPort GetSerialPort()
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
                    Thread.Sleep(100);
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
