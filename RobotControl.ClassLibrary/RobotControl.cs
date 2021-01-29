using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl.ClassLibrary
{
    public class RobotControl : RobotControlBase
    {
        private Action<Exception> onExceptionCallback;
        IState state;
        IObjectDetector objectDetector;
        ICameraCapturer cameraCapturer;
        ISpeechCommandListener speechCommandListener;
        IRobotCommunicationHandler robotCommunicationHandler;
        IRobotLogic robotLogic;
        ISpeaker speaker;

        public static IMediator mediator = new Mediator();

        private  string[] LabelsOfObjectsToDetect;

        public RobotControl()
            : base(mediator)
        {
        }

        public async Task InitializeAsync(
            string[] labelsOfObjectsToDetect,
            int baudRate,
            bool UseFakeObjectDetector = false,
            bool UseFakeCameraCapturer = false,
            bool UseFakeSpeechCommandListener = false,
            bool UseFakeSpeaker = false,
            bool UseFakeRobotCommunicationHandler = false,
            bool UseFakeRobotLogic = false
            )
        {
            await Task.Run(() =>
            {
                LabelsOfObjectsToDetect = new string[labelsOfObjectsToDetect.Length];
                Array.Copy(labelsOfObjectsToDetect, LabelsOfObjectsToDetect, labelsOfObjectsToDetect.Length);
                ISerialPort serialPort = UseFakeRobotCommunicationHandler ? (ISerialPort)new SerialPortFake() : new SerialPortImpl();

                state = new State(mediator);
                objectDetector = new ObjectDetector(mediator, UseFakeObjectDetector, "TinyYolo2_model.onnx", LabelsOfObjectsToDetect);
                cameraCapturer = new CameraCapturer(mediator, UseFakeCameraCapturer);
                speechCommandListener = new SpeechCommandListener(mediator, UseFakeSpeechCommandListener, state);
                robotCommunicationHandler = new RobotCommunicationHandler(mediator, serialPort, baudRate);
                robotLogic = new RobotLogic(mediator, UseFakeRobotLogic, state);
                if (!UseFakeSpeaker)
                {
                    speaker = new Speaker(mediator);
                }

                mediator.AddTarget((IPubSubBase)robotCommunicationHandler);
                mediator.AddTarget((IPubSubBase)robotLogic);
                mediator.AddTarget((IPubSubBase)state);
                mediator.AddTarget((IPubSubBase)objectDetector);
                mediator.AddTarget((IPubSubBase)cameraCapturer);
                mediator.AddTarget((IPubSubBase)speechCommandListener);
                if (!UseFakeSpeaker)
                {
                    mediator.AddTarget((IPubSubBase)speaker);
                }

                var s = "Robot is Ready. These are the commands I can understand: ";
                s += string.Join(", ", speechCommandListener.Commands);
                mediator.OnEvent(new EventDescriptor { Name = EventName.PleaseSay, Detail = s });
            });
        }
    }
}
