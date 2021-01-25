

namespace RobotControl.Net
{
    using Microsoft.WindowsAzure.Security.Authentication.GetSigningCertificates;

    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    class Program
    {
        private static bool UseFakeObjectDetector = false;
        private static bool UseFakeCameraCapturer = false;
        private static bool UseFakeSpeechCommandListener = false;
        private static bool UseFakeRobotCommunicationHandler = false;
        private static bool UseFakeRobotLogic = false;
        private static string[] LabelsOfObjectsToDetect;
        private static RobotControl RobotControl;
        static void Main(string[] args)
        {
            HandleArgs(args);
            RobotControl = new RobotControl(LabelsOfObjectsToDetect,
                115200,
                UseFakeObjectDetector,
                UseFakeCameraCapturer,
                UseFakeSpeechCommandListener,
                UseFakeRobotCommunicationHandler,
                UseFakeRobotLogic);

            while (true) { Thread.Sleep(10); }
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

            if (!GetLabelsOfObjectsToDetect(args))
            {
                ExplainUsageThenExit();
            }

            UseFakeObjectDetector            = GetOptionFlag(args, nameof(UseFakeObjectDetector));
            UseFakeCameraCapturer            = GetOptionFlag(args, nameof(UseFakeCameraCapturer));
            UseFakeSpeechCommandListener     = GetOptionFlag(args, nameof(UseFakeSpeechCommandListener));
            UseFakeRobotCommunicationHandler = GetOptionFlag(args, nameof(UseFakeRobotCommunicationHandler));
            UseFakeRobotLogic                = GetOptionFlag(args, nameof(UseFakeRobotLogic));
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

            LabelsOfObjectsToDetect = args[labelsIndex + 1].Split(',');
            for (int i = 0; i < LabelsOfObjectsToDetect.Length; i++)
            {
                LabelsOfObjectsToDetect[i] = LabelsOfObjectsToDetect[i].Trim().ToLowerInvariant();
                if (!(ObjectDetector.AvailableLabels.Any(a => a.Equals(LabelsOfObjectsToDetect[i], StringComparison.InvariantCultureIgnoreCase))))
                {
                    Console.WriteLine($"Object with label {LabelsOfObjectsToDetect[i]} is not detactable, these are the detectable objects:");
                    ListAvailableLabelsThenExit(2);
                }
            }

            return true;
        }

        private static void ExplainUsageThenExit()
        {
            Console.WriteLine($"RobotControl [-ListLabels] [-Labels \"l1,l2\"] [-{nameof(UseFakeObjectDetector)}] [-{nameof(UseFakeCameraCapturer)}] [-{nameof(UseFakeSpeechCommandListener)}] [-{nameof(UseFakeRobotCommunicationHandler)}] [-{nameof(UseFakeRobotLogic)}]");
            Console.WriteLine($"You must inform either -ListLabels or -Labels.");
            Console.WriteLine($" If you use -{nameof(UseFakeCameraCapturer)},");
            Console.WriteLine($"   RobotControl will simulate camera input by reading the file FakeCameraCapturer.mp4. ");
            Console.WriteLine($"   You can provide your content by overwriting this file, in the same folder as this executable.");
            Console.WriteLine($" If you use -{nameof(UseFakeRobotCommunicationHandler)},");
            Console.WriteLine($"   RobotControl will simulate serial port input by reading the file FakeSerialPort.json");
            Console.WriteLine($"Examples:");
            Console.WriteLine($" To list which objects are detected by RobotControl:");
            Console.WriteLine($"  RobotControl -ListLabels");
            Console.WriteLine($" To run the robot so it detects and runs towards cat and person:");
            Console.WriteLine($"  RobotControl -Labels \"cat,person\"]");
            Console.WriteLine($" To run RobotControl so it only detects cat and person,");
            Console.WriteLine($" but doesn't send / receive anything to the robot (such as if you just want to test the object detection logic):");
            Console.WriteLine($"  RobotControl -Labels \"cat,person\" -{nameof(UseFakeRobotCommunicationHandler)} -{nameof(UseFakeRobotLogic)}");
            Console.WriteLine($" To run the robot using simulated object detection (such as if you just want to test the robot logic):");
            Console.WriteLine($"  RobotControl -Labels \"cat,person\" -{nameof(UseFakeCameraCapturer)}  -{nameof(UseFakeCameraCapturer)}");

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
