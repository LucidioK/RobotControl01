namespace RobotControl.Net
{
    internal interface ISpeechCommandListener : ISubscriptionTarget
    {
        string[] Commands { get; }
        string GetLatestText();
    }
}