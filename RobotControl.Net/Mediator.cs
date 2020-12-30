
namespace RobotControl.Net
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    class Mediator : IMediator
    {
        readonly ConcurrentQueue<IPublishTarget> publishTargets;

        public Mediator(IEnumerable<IPubSubBase> targets)
        {
            foreach (var target in targets)
            {
                EnqueueTargetIfNeeded(target);
                SubscribeIfNeeded(target);
            }
        }

        private void SubscribeIfNeeded(IPubSubBase target) =>
            (target as ISubscriptionTarget)?.Subscribe(this);

        private void EnqueueTargetIfNeeded(IPubSubBase target)
        {
            if (Implements(target, nameof(IPublishTarget)))
            {
                publishTargets.Enqueue((IPublishTarget)target);
            }
        }

        public void OnEvent(IEventDescriptor eventDescriptor) => 
            publishTargets.ToList().ForEach(t => t.OnEvent(eventDescriptor));

        private bool Implements(object o, string interfaceName) => 
            o.GetType().GetInterface(interfaceName) != null;
    }
}
