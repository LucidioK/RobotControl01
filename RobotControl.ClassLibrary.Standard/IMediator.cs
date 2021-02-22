using System.Collections.Generic;

namespace RobotControl.ClassLibrary
{
    public interface IMediator : IPublishTarget, ISubscriptionTarget
    {
        void AddTarget(IPubSubBase target);
        void BlockEvent(EventName eventName);
        void UnblockEvent(EventName eventName);
    }
}