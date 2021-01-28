namespace RobotControl.ClassLibrary
{
    internal interface ISpeechCommandListener : ISubscriptionTarget
    {
        string[] Commands { get; }
        string GetLatestText();
    }
}