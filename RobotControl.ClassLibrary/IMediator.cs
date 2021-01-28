using System.Collections.Generic;

namespace RobotControl.ClassLibrary
{
    public interface IMediator : IPublishTarget, ISubscriptionTarget
    {
        void AddTarget(IPubSubBase target);
    }
}