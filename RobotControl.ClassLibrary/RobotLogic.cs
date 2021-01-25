
namespace RobotControl.Net
{
    using System;
    internal class RobotLogic : IRobotLogic
    {
        private readonly bool fake;
        private IState state;
        public EventName[] HandledEvents => new EventName[] { EventName.RobotData, EventName.ObjectDetected, EventName.RainDetected, EventName.VoiceCommandDetected };
        public RobotLogic(bool fake, IState state)
        {
            this.fake = fake;
            this.state = state;
        }

        public void OnEvent(IEventDescriptor eventDescriptor)
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
                default:
                    return;
            }
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

        private PubSub pubSub = new PubSub();
        public void Subscribe(IPublishTarget publisherTarget) => pubSub.Subscribe(publisherTarget);

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

        private void rotateInPlace() => motor(44, -44);


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
                motor(-44, 44);
            }
            else if (objectPosition > 5) // object is to the right
            {
                motor(44, -44);
            }
            else // object is straight ahead, CHARGE!
            {
                motor(150, 150);
            }
        }

        private void startObstacleAvoidance(IEventDescriptor eventDescriptor)
        {
            state.RobotState = RobotState.AvoidingObstacle;
            pubSub.Publish(new EventDescriptor {
                Name = EventName.PleaseSay,
                Detail = $"Detected obstacle at {eventDescriptor.State.ObstacleDistance} centimeters, starting obstacle avoidance."
            });
            motor(0, 0);
        }

        private void motor(float l, float r) =>
            pubSub.Publish(new EventDescriptor
            {
                Name = EventName.NeedToMoveDetected,
                Detail = $"{{'operation':'motor','l':{l},'r':{r}}}"
            });
    }
}
