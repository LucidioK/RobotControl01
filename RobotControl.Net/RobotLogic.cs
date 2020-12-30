
namespace RobotControl.Net
{
    using System;
    internal class RobotLogic : IRobotLogic
    {
        private PubSub pubSub = new PubSub();
        private IState state;

        public RobotLogic(IState state)
        {
            this.state = state;
        }

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            switch (eventDescriptor.Name)
            {
                case EventName.ObstacleDetected:
                    startObstacleAvoidance(eventDescriptor);
                    break;
                case EventName.ObjectDetected:
                    turnOrMove(eventDescriptor);
                    break;
                case EventName.LowBatteryDetected:
                    startChargingProcedure();
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
            throw new NotImplementedException();
        }

        private void turnOrMove(IEventDescriptor eventDescriptor)
        {
            if (eventDescriptor.Value < -5) // object is to the left
            {
                motor(-44, 44);
            }
            else if (eventDescriptor.Value > -5) // object is to the right
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
            motor(0, 0);
        }

        private void motor(float l, float r) => pubSub.Publish(new EventDescriptor { Name = EventName.NeedToMoveDetected, Detail = $"{{'operation':'motor','l':{l},'r':{r}}}" });
    }
}
