namespace RobotControl.ClassLibrary
{
    public interface IStoppable
    {
        void Stop();

        void WaitWhileStillRunning();

        bool ShouldWaitWhileStillRunning { get; }
    }

    public interface IPubSubBase : IStoppable
    {

    }

    public interface ISubscriptionTarget : IPubSubBase
    {
        void Subscribe(IPublishTarget publisher);
    }

    public interface IPublishTarget : IPubSubBase
    {
        void OnEvent(IEventDescriptor eventDescriptor);
        EventName[] HandledEvents { get; }
    }
}
