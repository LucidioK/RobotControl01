using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl.Net
{
    public class RobotControl
    {
        IState state;
        IObjectDetector objectDetector;
        ICameraCapturer cameraCapturer;
        ISpeechCommandListener speechCommandListener;
        IRobotCommunicationHandler robotCommunicationHandler;
        IRobotLogic robotLogic;
        ISpeaker speaker;

        ConcurrentQueue<IPubSubBase> publishersAndSubscribers;

        IMediator mediator;

        private  string[] LabelsOfObjectsToDetect;

        private  PubSub pubSub = new PubSub();

        public RobotControl(
            string[] labelsOfObjectsToDetect,
            bool UseFakeObjectDetector = false,
            bool UseFakeCameraCapturer = false,
            bool UseFakeSpeechCommandListener = false,
            bool UseFakeRobotCommunicationHandler = false,
            bool UseFakeRobotLogic = false)
        {
            LabelsOfObjectsToDetect = new string[labelsOfObjectsToDetect.Length];
            Array.Copy(labelsOfObjectsToDetect, LabelsOfObjectsToDetect, labelsOfObjectsToDetect.Length);
            ISerialPort serialPort = UseFakeRobotCommunicationHandler ? (ISerialPort)new SerialPortFake() : new SerialPortImpl();

            state = new State();
            objectDetector = new ObjectDetector(UseFakeObjectDetector, "TinyYolo2_model.onnx", LabelsOfObjectsToDetect);
            cameraCapturer = new CameraCapturer(UseFakeCameraCapturer);
            speechCommandListener = new SpeechCommandListener(UseFakeSpeechCommandListener, state);
            robotCommunicationHandler = new RobotCommunicationHandler(serialPort);
            robotLogic = new RobotLogic(UseFakeRobotLogic, state);
            speaker = new Speaker();

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
        }

        public void Subscribe(IPublishTarget target) => mediator.Subscribe(target);

    }
}
