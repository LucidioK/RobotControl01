
namespace RobotControl.Net
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    class Mediator : IMediator
    {
        readonly ConcurrentQueue<IPublishTarget> publishTargets = new ConcurrentQueue<IPublishTarget>();
        public EventName[] HandledEvents => new EventName[] { };
        public Mediator(IEnumerable<IPubSubBase> targets)
        {
            foreach (var target in targets)
            {
                EnqueueTargetIfNeeded(target);
                SubscribeIfNeeded(target);
            }
        }

        private void SubscribeIfNeeded(IPubSubBase target)
        {
            if (Implements(target, nameof(ISubscriptionTarget)))
            {
                ((ISubscriptionTarget)target).Subscribe(this);
            }
        }

        private void EnqueueTargetIfNeeded(IPubSubBase target)
        {
            if (Implements(target, nameof(IPublishTarget)))
            {
                publishTargets.Enqueue((IPublishTarget)target);
            }
        }

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            foreach (var publishTarget in publishTargets)
            {
                if (publishTarget.HandledEvents.Contains(eventDescriptor.Name))
                {
                    publishTarget.OnEvent(eventDescriptor);
                }
            }
        }

        private bool Implements(object o, string interfaceName) =>
            o.GetType().GetInterface(interfaceName) != null;

        public void Subscribe(IPublishTarget target) =>
            publishTargets.Enqueue(target);

    }
}
