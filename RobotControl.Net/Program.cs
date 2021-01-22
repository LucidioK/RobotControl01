﻿

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
        static IState                       state                    ;
        static IObjectDetector              objectDetector           ;
        static ICameraCapturer              cameraCapturer           ;
        static ISpeechCommandListener       speechCommandListener    ;
        static IRobotCommunicationHandler   robotCommunicationHandler;
        static IRobotLogic                  robotLogic               ;
        static ISpeaker                     speaker                  ;

        //static ConcurrentQueue<IEventDescriptor> eventDescriptors       ;
        static ConcurrentQueue<IPubSubBase> publishersAndSubscribers ;

        //static ChainOfResponsibility             chainOfResponsibility;
        static IMediator                          mediator;
        private static bool UseFakeObjectDetector = false;
        private static bool UseFakeCameraCapturer = false;
        private static bool UseFakeSpeechCommandListener = false;
        private static bool UseFakeRobotCommunicationHandler = false;
        private static bool UseFakeRobotLogic = false;
        private static string[] LabelsOfObjectsToDetect;
        private static PubSub pubSub = new PubSub();

        static void Main(string[] args)
        {
            HandleArgs(args);

            ISerialPort serialPort   = UseFakeRobotCommunicationHandler ? (ISerialPort)new SerialPortFake() : new SerialPortImpl();

            state                    = new State();
            objectDetector           = new ObjectDetector(UseFakeObjectDetector, "TinyYolo2_model.onnx", LabelsOfObjectsToDetect);
            cameraCapturer           = new CameraCapturer(UseFakeCameraCapturer);
            speechCommandListener    = new SpeechCommandListener(UseFakeSpeechCommandListener, state);
            robotCommunicationHandler= new RobotCommunicationHandler(serialPort);
            robotLogic               = new RobotLogic(UseFakeRobotLogic, state);
            speaker                  = new Speaker();

            publishersAndSubscribers = new ConcurrentQueue<IPubSubBase>();

            publishersAndSubscribers.Enqueue((IPubSubBase)robotCommunicationHandler);
            publishersAndSubscribers.Enqueue((IPubSubBase)robotLogic);
            publishersAndSubscribers.Enqueue((IPubSubBase)state);
            publishersAndSubscribers.Enqueue((IPubSubBase)objectDetector);
            publishersAndSubscribers.Enqueue((IPubSubBase)cameraCapturer);
            publishersAndSubscribers.Enqueue((IPubSubBase)speechCommandListener);
            publishersAndSubscribers.Enqueue((IPubSubBase)speaker);
            mediator = new Mediator(publishersAndSubscribers);

            pubSub.Publish(new EventDescriptor { Name = EventName.PleaseSay, Detail = "Robot is Ready." });
            //eventDescriptors.Enqueue((IEventDescriptor)objectDetector);
            //eventDescriptors.Enqueue((IEventDescriptor)cameraCapturer);
            //eventDescriptors.Enqueue((IEventDescriptor)robotReader);
            //chainOfResponsibility = new ChainOfResponsibility(eventDescriptors);
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
