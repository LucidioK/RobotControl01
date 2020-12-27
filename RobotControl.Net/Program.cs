

namespace RobotControl.Net
{
    using System.Collections.Concurrent;
    using System.Threading;

    class Program
    {
        static IState                            state                    = new State();
        static ObjectDetector                    objectDetector           = new ObjectDetector("TinyYolo2_model.onnx");
        static CameraCapturer                    cameraCapturer           = new CameraCapturer(state);
        static SpeechCommandListener             speechCommandListener    = new SpeechCommandListener(state);
        static RobotCommunicationHandler         robotReader              = new RobotCommunicationHandler(state);
        static RobotLogic                        robotLogic               = new RobotLogic(state);
        //static ConcurrentQueue<IEventDescriptor> eventDescriptors         = new ConcurrentQueue<IEventDescriptor>();
        static ConcurrentQueue<IPubSubBase>      publishersAndSubscribers = new ConcurrentQueue<IPubSubBase>();
        //static ChainOfResponsibility             chainOfResponsibility;
        static Mediator                          mediator;

        static void Main(string[] args)
        {
            publishersAndSubscribers.Enqueue((IPubSubBase)state);
            publishersAndSubscribers.Enqueue((IPubSubBase)objectDetector);
            publishersAndSubscribers.Enqueue((IPubSubBase)cameraCapturer);
            publishersAndSubscribers.Enqueue((IPubSubBase)speechCommandListener);
            publishersAndSubscribers.Enqueue((IPubSubBase)robotReader);
            publishersAndSubscribers.Enqueue((IPubSubBase)robotLogic);
            mediator = new Mediator(publishersAndSubscribers);

            //eventDescriptors.Enqueue((IEventDescriptor)objectDetector);
            //eventDescriptors.Enqueue((IEventDescriptor)cameraCapturer);
            //eventDescriptors.Enqueue((IEventDescriptor)robotReader);
            //chainOfResponsibility = new ChainOfResponsibility(eventDescriptors);
            while (true) { Thread.Sleep(10); }
        }




    }

}
