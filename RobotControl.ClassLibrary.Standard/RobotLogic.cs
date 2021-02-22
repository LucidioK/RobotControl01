
namespace RobotControl.ClassLibrary
{
    using System;
    internal class RobotLogic : RobotControlBase, IRobotLogic
    {
        private readonly bool fake;
        private IState state;
        private float LMultiplier = 1.0f, RMultiplier = 1.0f;
        public EventName[] HandledEvents => new EventName[] { EventName.RobotData, EventName.ObjectDetected, EventName.RainDetected, EventName.MotorCalibrationRequest, EventName.VoiceCommandDetected };
        public RobotLogic(IMediator mediator, bool fake, IState state)
            : base(mediator)
        {
            this.fake = fake;
            this.state = state;
        }

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            if (ShouldContinue())
            {
                TryCatch(() =>
                {
                    switch (eventDescriptor.Name)
                    {
                        case EventName.RobotData:
                            checkRobotData(eventDescriptor);
                            break;
                        case EventName.ObjectDetected:
                            turnOrMove(eventDescriptor);
                            break;
                        case EventName.RainDetected:
                            startEvadingToShelter();
                            break;
                        case EventName.VoiceCommandDetected:
                            handleVoiceCommand(eventDescriptor);
                            break;
                        case EventName.MotorCalibrationRequest:
                            motorCalibrationRequest();
                            break;
                        default:
                            return;
                    }
                });
            }
        }

        public void SetMotorCalibrationValues(float LMultiplier, float RMultiplier)
        {
            this.LMultiplier = LMultiplier;
            this.RMultiplier = RMultiplier;
        }

        private void motorCalibrationRequest()
        {
            ;
        }

        private void checkRobotData(IEventDescriptor eventDescriptor)
        {
            if (eventDescriptor.State.ObstacleDistance < 30 && eventDescriptor.State.ObstacleDistance > 0)
            {
                startObstacleAvoidance(eventDescriptor);
            }
            else if (eventDescriptor.State.BatteryVoltage < 11)
            {
                startChargingProcedure();
            }
        }

        private void handleVoiceCommand(IEventDescriptor eventDescriptor)
        {
            switch (eventDescriptor.Detail.Replace("robot ", "").Replace("robot", ""))
            {
                case "start":
                    startScanningObjects();
                    break;
                case "continue":
                    startScanningObjects();
                    break;
                case "stop":
                    stopRobot();
                    break;
            }
        }

        private void stopRobot() => motor(0, 0);

        private void startScanningObjects()
        {
            state.RobotState = RobotState.Scanning;
            rotateInPlace();
        }

        private void rotateInPlace() => motor(64, -64);


        private void startEvadingToShelter()
        {
            state.RobotState = RobotState.EvadingToShelter;
        }

        private void startChargingProcedure()
        {
            // TODO: "-->IMPORTANT: {nameof(RobotLogic)}.{nameof(startChargingProcedure)} not yet implemented!");
        }

        private void turnOrMove(IEventDescriptor eventDescriptor)
        {
            var objectPosition = eventDescriptor.Value * 100;
            if (objectPosition < -5) // object is to the left
            {
                motor(-64, 64);
            }
            else if (objectPosition > 5) // object is to the right
            {
                motor(64, -64);
            }
            else // object is straight ahead, CHARGE!
            {
                motor(150, 150);
            }
        }

        private void startObstacleAvoidance(IEventDescriptor eventDescriptor)
        {
            state.RobotState = RobotState.AvoidingObstacle;
            Publish(new EventDescriptor {
                Name = EventName.PleaseSay,
                Detail = $"Detected obstacle at {eventDescriptor.State.ObstacleDistance} centimeters, starting obstacle avoidance."
            });
            motor(0, 0);
        }

        private void motor(float l, float r) =>
            Publish(new EventDescriptor
            {
                Name = EventName.NeedToMoveDetected,
                Detail = $"{{'operation':'motor','l':{l * LMultiplier},'r':{r * RMultiplier}}}"
            });
    }
}
