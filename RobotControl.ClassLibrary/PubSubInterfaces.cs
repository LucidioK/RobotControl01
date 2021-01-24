namespace RobotControl.Net
{
    public interface IPubSubBase
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
