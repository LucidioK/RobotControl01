

namespace RobotControl.Net
{
    using Microsoft.WindowsAzure.Security.Authentication.GetSigningCertificates;

    using System;
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
        //static ConcurrentQueue<IEventDescriptor> eventDescriptors       ;
        static ConcurrentQueue<IPubSubBase> publishersAndSubscribers ;
        //static ChainOfResponsibility             chainOfResponsibility;
        static IMediator                          mediator;
        private static bool UseFakeObjectDetector = false;
        private static bool UseFakeCameraCapturer = false;
        private static bool UseFakeSpeechCommandListener = false;
        private static bool UseFakeRobotCommunicationHandler = false;
        private static bool UseFakeRobotLogic = false;

        static void Main(string[] args)
        {
            HandleArgs(args);

            state                    = new State();
            objectDetector           = UseFakeObjectDetector 
                                        ? (IObjectDetector)new FakeObjectDetector() 
                                        : new ObjectDetector("TinyYolo2_model.onnx");

            cameraCapturer           = UseFakeCameraCapturer 
                                        ? (ICameraCapturer)new FakeCameraCapturer()
                                        : new CameraCapturer(state);
            speechCommandListener    = UseFakeSpeechCommandListener 
                                        ? (ISpeechCommandListener)new FakeSpeechCommandListener() 
                                        : new SpeechCommandListener(state);
            robotCommunicationHandler= UseFakeRobotCommunicationHandler 
                                        ? (IRobotCommunicationHandler)new FakeRobotCommunicationHandler() 
                                        : new RobotCommunicationHandler(state);
            robotLogic               = UseFakeRobotLogic 
                                        ? (IRobotLogic)new FakeRobotLogic() 
                                        : new RobotLogic(state);

            publishersAndSubscribers = new ConcurrentQueue<IPubSubBase>();

            publishersAndSubscribers.Enqueue((IPubSubBase)state);
            publishersAndSubscribers.Enqueue((IPubSubBase)objectDetector);
            publishersAndSubscribers.Enqueue((IPubSubBase)cameraCapturer);
            publishersAndSubscribers.Enqueue((IPubSubBase)speechCommandListener);
            publishersAndSubscribers.Enqueue((IPubSubBase)robotCommunicationHandler);
            publishersAndSubscribers.Enqueue((IPubSubBase)robotLogic);
            mediator = new Mediator(publishersAndSubscribers);

            //eventDescriptors.Enqueue((IEventDescriptor)objectDetector);
            //eventDescriptors.Enqueue((IEventDescriptor)cameraCapturer);
            //eventDescriptors.Enqueue((IEventDescriptor)robotReader);
            //chainOfResponsibility = new ChainOfResponsibility(eventDescriptors);
            while (true) { Thread.Sleep(10); }
        }

        private static void HandleArgs(string[] args)
        {
            var lArgs = new List<string>(args);
            lArgs.ForEach(a => a.Trim(new char[] { ' ', '-', '/' }).ToLowerInvariant());
            if (lArgs.Contains("?") || lArgs.Contains("help"))
            {
                Console.WriteLine("$RobotControl [-{nameof(UseFakeObjectDetector)}] [-{nameof(UseFakeCameraCapturer)}] [-{nameof(UseSpeechCommandListener)}] [-{nameof(UseFakeRobotCommunicationHandler)}] [-{nameof(UseRobotLogic)}]");
                Environment.Exit(1);
            }
            UseFakeObjectDetector            = args.Contains(nameof(UseFakeObjectDetector).ToLowerInvariant());
            UseFakeCameraCapturer            = args.Contains(nameof(UseFakeCameraCapturer).ToLowerInvariant());
            UseFakeSpeechCommandListener     = args.Contains(nameof(UseFakeSpeechCommandListener).ToLowerInvariant());
            UseFakeRobotCommunicationHandler = args.Contains(nameof(UseFakeRobotCommunicationHandler).ToLowerInvariant());
            UseFakeRobotLogic                = args.Contains(nameof(UseFakeRobotLogic).ToLowerInvariant());
        }
    }

}
