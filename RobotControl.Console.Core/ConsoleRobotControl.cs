using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            private int windowWidth = Console.WindowWidth;
            private int windowHeight = Console.WindowHeight;
            private string halfWidth = new string(' ', Console.WindowWidth / 2);
            public void Display()
            {
                // Clean one line above, one line below.
                Clean(-1);
                Clean(+1);

                // Now show value.
                Console.SetCursorPosition(Point.X, Point.Y);
                Console.Write($"{Name}: ");
                Console.ForegroundColor = Color;
                Console.Write($"{Value}{halfWidth}");
                Console.ResetColor();
            }

            private void Clean(int yDelta)
            {
                Console.SetCursorPosition(Point.X, Math.Max(Point.Y + yDelta, 0));
                Console.Write(halfWidth.Substring(0, Math.Min(halfWidth.Length, windowWidth - Point.X)));
            }
        }

        List<EventName> handledEvents = new List<EventName>();
        public EventName[] HandledEvents => handledEvents.ToArray();

        public bool ShouldWaitWhileStillRunning => false;

        private static global::RobotControl.ClassLibrary.RobotControl RobotControl;
        List<List<string>> dataFields = new List<List<string>>()
        {
            new List<string>{ "AcX", "AcY", "AcZ" },
            new List<string>{ "Dcm", "Cmp", "XDl" },
            new List<string>{ "MoL", "MoR", "Vlt" },
            new List<string>{ "Say" },
            new List<string>{ "Msg" }
        };

        List<DataDisplay> dataDisplays = new List<DataDisplay>();

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            switch (eventDescriptor.Name)
            {
                case EventName.VoiceCommandDetected:
                {
                    break;
                }

                case EventName.RainDetected:
                {
                    break;
                }

                case EventName.ObjectDetected:
                {
                    try
                    {
                        var details = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventDescriptor.Detail);
                        var xd = (float)((double)details["XDeltaFromBitmapCenter"]);
                        SetDisplay("XDl", xd.ToString("0.0"), GetColorByThresholds(xd, 10f, 5f));
                    }
                    catch (Exception) { }
                    break;
                }

                case EventName.SensorValueDetected:
                {
                    break;
                }

                case EventName.NeedToMoveDetected:
                {
                    try
                    {
                        var details = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventDescriptor.Detail);
                        SetDisplay("MoL", details["l"].ToString());
                        SetDisplay("MoR", details["r"].ToString());
                    }
                    catch (Exception) { }
                    break;
                }

                case EventName.NewImageDetected:
                {
                    break;
                }

                case EventName.RawRobotDataDetected:
                {
                    break;
                }

                case EventName.RobotData:
                {
                    var s = eventDescriptor.State;
                    SetDisplay("AcX", s.XAcceleration.ToString("0.0"), GetColorByThresholds(s.XAcceleration, 2f, 3.5f));
                    SetDisplay("AcY", s.YAcceleration.ToString("0.0"), GetColorByThresholds(s.YAcceleration, 2f, 3.5f));
                    SetDisplay("AcZ", s.ZAcceleration.ToString("0.0"), GetColorByThresholds((s.ZAcceleration - 10) % 10, 2f, 3.5f));
                    SetDisplay("Dcm", s.ObstacleDistance.ToString("0.0"), GetColorByThresholds(s.ObstacleDistance, 20f, 10f));
                    SetDisplay("Vlt", s.BatteryVoltage.ToString("0.0"), GetColorByThresholds(s.BatteryVoltage, 12f, 10f));
                    SetDisplay("Cmp", s.CompassHeading.ToString("0.0"));
                    break;
                }

                case EventName.PleaseSay:
                {
                    SetDisplay("Say", eventDescriptor.Detail);
                    break;
                }

                case EventName.Exception:
                {
                    SetDisplay("Msg", eventDescriptor.Detail);
                    break;
                }
            }
            RefreshDisplay();
        }

        private int refreshCount = 0;
        private void RefreshDisplay()
        {
            Console.CursorVisible = false;
            refreshCount++;
            if (refreshCount > 64)
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

        public void Stop() { }

        public void WaitWhileStillRunning() { }
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
