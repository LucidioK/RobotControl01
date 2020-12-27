
namespace RobotControl.Net
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    class Mediator : IPublishTarget
    {
        ConcurrentQueue<IPublishTarget> publishTargets;

        public Mediator(IEnumerable<IPubSubBase> targets)
        {
            foreach (var target in targets)
            {
                if (implements(target, nameof(IPublishTarget)))
                {
                    publishTargets.Enqueue((IPublishTarget)target);
                }
                if (implements(target, nameof(ISubscriptionTarget)))
                {
                    ((ISubscriptionTarget)target).Subscribe(this);
                }
            }
        }

        public void OnEvent(IEventDescriptor eventDescriptor) => publishTargets.ToList().ForEach(t => t.OnEvent(eventDescriptor));

        private bool implements(object o, string interfaceName) => o.GetType().GetInterface(interfaceName) != null;
    }
}
