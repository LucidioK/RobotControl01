

namespace RobotControl.ClassLibrary
{
    using System;
    using System.Threading;

    internal class RobotCommunicationHandler : RobotControlBase, IRobotCommunicationHandler
    {
        ISerialPort serialPort;
        public EventName[] HandledEvents => new EventName[] { EventName.NeedToMoveDetected };
        public RobotCommunicationHandler(IMediator mediator, ISerialPort serialPort, int baudRate)
            : base(mediator)
        {
            this.serialPort = serialPort;

            bool foundPort = false;
            for (int i = 0; i < 4 && !foundPort; i++)
            {
                for (int j = 1; j < 32 && !foundPort; j++)
                {
                    if (this.serialPort.Open(j, baudRate, OnValidDataReceivedCallback, OnExceptionCallback))
                    {
                        foundPort = true;
                    }
                }

                Thread.Sleep(100);
            }

            if (!foundPort)
            {
                PublishException(new Exception("Could not find serial port. Open the Arduino IDE, then open Tools/Serial Monitor and close the monitor..."));
            }
        }

        private void OnExceptionCallback(Exception ex)
            => PublishException(ex);

        private void OnValidDataReceivedCallback(string s)
        {
            s = GetValidatedString(s);
            if (s != null)
            {
                Publish(new EventDescriptor { Name = EventName.RawRobotDataDetected, Detail = s });
            }
        }

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            switch (eventDescriptor.Name)
            {
                case EventName.NeedToMoveDetected:
                    serialPort.Write(eventDescriptor.Detail);
                    break;
                default:
                    return;
            }
        }

        private static string GetValidatedString(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {

                s = s.Trim(trimChars: new char[] { ' ', '\r', '\n', '\t' });
                if (s != null && s.StartsWith("{") && s.EndsWith("}"))
                {
                    return s;
                }
            }
            return null;
        }
    }
}