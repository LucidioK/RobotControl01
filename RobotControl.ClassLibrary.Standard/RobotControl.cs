using System;
using System.Threading.Tasks;

namespace RobotControl.ClassLibrary
{
    public class RobotControl : RobotControlBase
    {
        private Action<Exception> onExceptionCallback;
        IState state;
        IObjectDetector objectDetector;
        ICameraCapturer cameraCapturer;

        IRobotCommunicationHandler robotCommunicationHandler;
        IRobotLogic robotLogic;

        public static IMediator Mediator = new Mediator();

        private  string[] LabelsOfObjectsToDetect;

        public RobotControl()
            : base(Mediator)
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
            bool UseFakeRobotLogic = false,
            float LMultiplier = 1.0f,
            float RMultiplier = 1.0f
            )
        {
            await Task.Run(() =>
            {
                LabelsOfObjectsToDetect = new string[labelsOfObjectsToDetect.Length];
                Array.Copy(labelsOfObjectsToDetect, LabelsOfObjectsToDetect, labelsOfObjectsToDetect.Length);
                ISerialPort serialPort = UseFakeRobotCommunicationHandler ? (ISerialPort)new SerialPortFake() : new SerialPortImpl();

                state = new State(Mediator);
                objectDetector = new ObjectDetector(Mediator, UseFakeObjectDetector, "TinyYolo2_model.onnx", LabelsOfObjectsToDetect);
                cameraCapturer = new CameraCapturer(Mediator, UseFakeCameraCapturer);
                robotCommunicationHandler = new RobotCommunicationHandler(Mediator, serialPort, baudRate);
                robotLogic = new RobotLogic(Mediator, UseFakeRobotLogic, state);
                robotLogic.SetMotorCalibrationValues(LMultiplier, RMultiplier);

                Mediator.AddTarget((IPubSubBase)robotCommunicationHandler);
                Mediator.AddTarget((IPubSubBase)robotLogic);
                Mediator.AddTarget((IPubSubBase)state);
                Mediator.AddTarget((IPubSubBase)objectDetector);
                Mediator.AddTarget((IPubSubBase)cameraCapturer);
            });
        }

        public async Task InitializeBasicAsync(int baudRate)
        {
            await Task.Run(() =>
            {
                ISerialPort serialPort = new SerialPortImpl();

                state = new State(Mediator);
                robotCommunicationHandler = new RobotCommunicationHandler(Mediator, serialPort, baudRate);
                robotLogic = new RobotLogic(Mediator, false, state);

                Mediator.AddTarget((IPubSubBase)robotCommunicationHandler);
                Mediator.AddTarget((IPubSubBase)robotLogic);
                Mediator.AddTarget((IPubSubBase)state);
            });
        }

        public override void Stop()
        {
            Mediator.Stop();
            Mediator.WaitWhileStillRunning();
            base.Stop();
        }

        public void BlockEvent(EventName eventName) => Mediator.BlockEvent(eventName);
        public void UnblockEvent(EventName eventName) => Mediator.UnblockEvent(eventName);
    }
}
