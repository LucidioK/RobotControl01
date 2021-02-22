namespace RobotControl.ClassLibrary
{
    public interface IRobotLogic : IPublishTarget, ISubscriptionTarget
    {
        void SetMotorCalibrationValues(float LMultiplier, float RMultiplier);
    }
}