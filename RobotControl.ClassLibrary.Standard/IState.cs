namespace RobotControl.ClassLibrary
{
    public interface IState : IPublishTarget,ISubscriptionTarget
    {
        RobotState RobotState { get; set; }
        float ObstacleDistance { get; set; }
        float UVLevel { get; set; }
        float BatteryVoltage { get; set; }
        float XAcceleration { get; set; }
        float YAcceleration { get; set; }
        float ZAcceleration { get; set; }
        float CompassHeading { get; set; }
    }
}
