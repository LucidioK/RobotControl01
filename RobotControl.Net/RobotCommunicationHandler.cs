

namespace RobotControl.Net
{
    using System;
    using System.IO.Ports;
    using System.Threading;

    internal class RobotCommunicationHandler : IRobotCommunicationHandler
    {
        SerialPort serialPort;
        IState state;
        public RobotCommunicationHandler(IState state)
        {
            this.state = state;
            bool foundPort = false;
            while (!foundPort)
            {
                for (int i = 1; i < 32 && !foundPort; i++)
                {
                    if (tryOpenSerialPort(i))
                    {
                        string s = serialPort.ReadExisting();
                        if (s.StartsWith("{") && s.EndsWith("}"))
                        {
                            foundPort = true;
                        }
                    }
                }

                Thread.Sleep(500);
            }

            if (!foundPort)
            {
                throw new Exception("Could not find serial port...");
            }

            thread = new Thread(new ThreadStart(RobotListenerThread));
            thread.Start();
        }

        private void RobotListenerThread()
        {
            while (true)
            {
                var s = serialPort.ReadLine();
                pubSub.Publish(new EventDescriptor { Name = EventName.RobotDataDetected, Detail = s });
            }
        }

        private PubSub pubSub = new PubSub();
        private readonly Thread thread;

        public void Subscribe(IPublishTarget publisherTarget) => pubSub.Subscribe(publisherTarget);

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

        private bool tryOpenSerialPort(int i)
        {
            try
            {
                serialPort = new SerialPort($"COM{i}", 9600);
                serialPort.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}