using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

using Newtonsoft.Json;

using OpenCvSharp;

using RobotControl.ClassLibrary;
namespace RobotControl
{
    class Ctl : IPublishTarget
    {
        class DataDisplay
        {
            public string Name;
            public string Value = string.Empty;
            public ConsoleColor Color = Console.ForegroundColor;
            public Point Point;
            public void Display()
            {
                Console.SetCursorPosition(Point.X, Point.Y);
                Console.Write($"{Name}: ");
                Console.ForegroundColor = Color;
                Console.Write($"{Value}    ");
                Console.ResetColor();
            }
        }

        List<EventName> handledEvents = new List<EventName>();
        public EventName[] HandledEvents => handledEvents.ToArray();
        private static global::RobotControl.ClassLibrary.RobotControl RobotControl;
        List<List<string>> dataFields = new List<List<string>>()
        {
            new List<string>{ "AccelX",   "AccelY",  "AccelZ" },
            new List<string>{ "Distance", "Compass" },
            new List<string>{ "MotorL", "MotorR", "Voltage" },
            new List<string>{ "PleaseSay" },
            new List<string>{ "Data" },
            new List<string>{ "Message" }
        };

        List<DataDisplay> dataDisplays = new List<DataDisplay>();

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            switch (eventDescriptor.Name)
            {
                case EventName.VoiceCommandDetected:
                    break;
                case EventName.RainDetected:
                    break;
                case EventName.ObjectDetected:
                    SetDisplay("Data", eventDescriptor.Detail);
                    break;
                case EventName.SensorValueDetected:
                    break;
                case EventName.NeedToMoveDetected:
                    var details = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventDescriptor.Detail);
                    SetDisplay("MotorL", details["l"].ToString());
                    SetDisplay("MotorR", details["r"].ToString());
                    break;
                //{
                //    var details = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventDescriptor.Detail);
                //    this.Dispatcher.Invoke(() =>
                //    {
                //        this.lblMotorL.Content = details.ContainsKey("l") ? details["l"].ToString() : "_";
                //        this.lblMotorR.Content = details.ContainsKey("r") ? details["r"].ToString() : "_"; ;
                //    });
                //    break;
                //}
                case EventName.NewImageDetected:
                    break;
                case EventName.RawRobotDataDetected:

                    break;
                case EventName.RobotData:
                    var s = eventDescriptor.State;
                    SetDisplay("AccelX", s.XAcceleration.ToString("0.0"), GetColorByThresholds(s.XAcceleration, 2f, 3.5f));
                    SetDisplay("AccelY", s.YAcceleration.ToString("0.0"), GetColorByThresholds(s.YAcceleration, 2f, 3.5f));
                    SetDisplay("AccelZ", s.ZAcceleration.ToString("0.0"), GetColorByThresholds((s.ZAcceleration -10)%10 , 2f, 3.5f));
                    SetDisplay("Distance", s.ObstacleDistance.ToString("0.0"), GetColorByThresholds(s.ObstacleDistance, 20f, 10f));
                    SetDisplay("Voltage", s.BatteryVoltage.ToString("0.0"), GetColorByThresholds(s.BatteryVoltage, 12f, 10f));
                    SetDisplay("Compass", s.CompassHeading.ToString("0.0"));
                    break;
                case EventName.PleaseSay:
                    SetDisplay("PleaseSay", eventDescriptor.Detail);
                    break;
                case EventName.Exception:
                    SetDisplay("Message", eventDescriptor.Detail);
                    break;
            }
            RefreshDisplay();
        }

        private int refreshCount = 0;
        private void RefreshDisplay()
        {
            refreshCount++;
            if (refreshCount > 16)
            {
                refreshCount = 0;
                Console.Clear();
            }

            foreach (var dataDisplay in dataDisplays)
            {
                dataDisplay.Display();
            }
        }

        private ConsoleColor? GetColorByThresholds(float acceleration, float yellowThreshold, float redThreshold)
        {
            acceleration = Math.Abs(acceleration);
            var color = ConsoleColor.Green;
            if (yellowThreshold < redThreshold)
            {
                if (acceleration > redThreshold)
                {
                    color = ConsoleColor.Red;
                }
                else if (acceleration > yellowThreshold)
                {
                    color = ConsoleColor.Yellow;
                }
            }
            else
            {
                if (acceleration <= redThreshold)
                {
                    color = ConsoleColor.Red;
                }
                else if (acceleration <= yellowThreshold)
                {
                    color = ConsoleColor.Yellow;
                }
            }
            return color;
        }

        public Ctl()
        {
            foreach (var eventName in typeof(EventName).GetEnumValues())
            {
                handledEvents.Add((EventName)eventName);
            }
            var incrY = Console.WindowHeight / (dataFields.Count+1) ;
            var posY = 0;
            foreach (var fieldLine in dataFields)
            {
                posY += incrY;
                var incrX = Console.WindowWidth / (fieldLine.Count + 1);
                for (var i = 0; i < fieldLine.Count; i++)
                {
                    var posX = (i * incrX) + 1;
                    dataDisplays.Add(new DataDisplay { Name = fieldLine[i], Point = new Point(posX, posY) });
                }
            }
        }

        public async System.Threading.Tasks.Task StartAsync(string[] labelsOfObjectsToDetect, int baudRate)
        {
            RobotControl = new global::RobotControl.ClassLibrary.RobotControl();
            await RobotControl.InitializeAsync(
                labelsOfObjectsToDetect: labelsOfObjectsToDetect,
                baudRate: baudRate,
                UseFakeSpeechCommandListener: true,
                UseFakeSpeaker: true);
            RobotControl.Subscribe(this);
            Console.Clear();
        }

        private void SetDisplay(string dataField, string detail, ConsoleColor? dataColor = null)
        {
            var dataDisplay = dataDisplays.FirstOrDefault(d => d.Name == dataField);
            dataDisplay.Value = detail;
            dataDisplay.Color = dataColor ?? Console.ForegroundColor;
        }
    }



    class ConsoleRobotControl 
    {
        private static string[] labelsOfObjectsToDetect;
        private static int[] acceptableBaudRates = { 2400, 4800, 9600, 19200, 38400, 57600, 74880, 115200, 230400 };
        private static int baudRate = 115200;

        public EventName[] HandledEvents => throw new NotImplementedException();

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            HandleArgs(args);

            var ctl = new Ctl();
            await ctl.StartAsync(labelsOfObjectsToDetect, baudRate);
            while (true)
            {
                Thread.Sleep(10);
            }
        }

        private static void HandleArgs(string[] args)
        {
            if (args == null || args.Length == 0 || GetOptionFlag(args, "?") || GetOptionFlag(args, "help"))
            {
                ExplainUsageThenExit();
            }

            if (GetOptionFlag(args, "ListLabels"))
            {
                ListAvailableLabelsThenExit(0);
            }

            var baudRateIndex = ArgIndex(args, "BaudRate");
            if (baudRateIndex >= 0)
            {
                if (baudRateIndex + 1 >= args.Length || 
                    !int.TryParse(args[baudRateIndex+1], out baudRate) ||
                    !acceptableBaudRates.Contains(baudRate))
                {
                    ExplainUsageThenExit();
                }
            }

            if (!GetLabelsOfObjectsToDetect(args))
            {
                ExplainUsageThenExit();
            }

        }

        private static void ListAvailableLabelsThenExit(int exitCode)
        {
            Console.WriteLine(string.Join(",", ObjectDetector.AvailableLabels));
            Environment.Exit(exitCode);
        }

        private static bool GetLabelsOfObjectsToDetect(string[] args)
        {
            var labelsIndex = ArgIndex(args, "Labels");
            if (labelsIndex < 0 || labelsIndex + 1 >= args.Length)
            {
                return false;
            }

            labelsOfObjectsToDetect = args[labelsIndex + 1].Split(',');
            for (int i = 0; i < labelsOfObjectsToDetect.Length; i++)
            {
                labelsOfObjectsToDetect[i] = labelsOfObjectsToDetect[i].Trim().ToLowerInvariant();
                if (!(ObjectDetector.AvailableLabels.Any(a => a.Equals(labelsOfObjectsToDetect[i], StringComparison.InvariantCultureIgnoreCase))))
                {
                    Console.WriteLine($"Object with label {labelsOfObjectsToDetect[i]} is not detactable, these are the detectable objects:");
                    ListAvailableLabelsThenExit(2);
                }
            }

            return true;
        }

        private static void ExplainUsageThenExit()
        {
            Console.WriteLine($"RobotControl [-ListLabels] [-Labels \"l1,l2\"] [-BaudRate baudrate]");
            Console.WriteLine($"You must inform either -ListLabels or -Labels.");
            Console.WriteLine($"Default Baudrate is 115200. Acceptable values are:");
            Console.WriteLine($"  {string.Join(", ", acceptableBaudRates.Select(b => b.ToString()))}");
            Console.WriteLine($"Examples:");
            Console.WriteLine($" To list which objects are detected by RobotControl:");
            Console.WriteLine($"  RobotControl -ListLabels");
            Console.WriteLine($" To run the robot so it detects and runs towards cat and person, Arduino at 9600:");
            Console.WriteLine($"  RobotControl -Labels \"cat,person\" -BaudRate 9600");

            Environment.Exit(1);
        }

        private static bool GetOptionFlag(string[] args, string flagName) => ArgIndex(args, flagName) >= 0;

        private static string AddDashIfNeeded(string flagName) => flagName.StartsWith("-") ? flagName : $"-{flagName}";

        private static int ArgIndex(string[] args, string flagName)
        {
            flagName = AddDashIfNeeded(flagName);
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(flagName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
