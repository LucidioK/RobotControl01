namespace RobotControl.Net
{
    internal interface ISpeechCommandListener : ISubscriptionTarget
    {
        string GetLatestText();
    }
}