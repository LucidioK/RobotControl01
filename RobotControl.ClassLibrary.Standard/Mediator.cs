
namespace RobotControl.ClassLibrary
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    class Mediator : Stoppable, IMediator
    {
        readonly ConcurrentQueue<IPublishTarget> publishTargets = new ConcurrentQueue<IPublishTarget>();
        readonly ConcurrentBag<EventName> blockedEvents = new ConcurrentBag<EventName>();
        public EventName[] HandledEvents => new EventName[] { };

        public override bool ShouldWaitWhileStillRunning => true;

        public Mediator()
        {
        }

        public void AddTarget(IPubSubBase target)
        {
            EnqueueTargetIfNeeded(target);
            SubscribeIfNeeded(target);
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
                publishTargets.Enqueue((IPublishTarget) target);
            }
        }

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            if (blockedEvents.Contains(eventDescriptor.Name))
            {
                return;
            }

            foreach (var publishTarget in publishTargets)
            {
                if (publishTarget.HandledEvents.Contains(eventDescriptor.Name))
                {
                    publishTarget.OnEvent(eventDescriptor);
                    eventDescriptor.EventWasProcessed();
                }
            }
        }

        private bool Implements(object o, string interfaceName) =>
            o.GetType().GetInterface(interfaceName) != null;

        public void Subscribe(IPublishTarget target) =>
            publishTargets.Enqueue(target);

        public override void Stop()
        {
            foreach (var target in publishTargets)
            {
                target.Stop();
                if (target.ShouldWaitWhileStillRunning)
                {
                    target.WaitWhileStillRunning();
                }
            }

            while (publishTargets.TryDequeue(out IPublishTarget target)) ;
            FinishedCleaning();
        }

        public override void WaitWhileStillRunning() =>
            WaitWhileStillRunningInternal(10000);

        public void BlockEvent(EventName eventName) => blockedEvents.Add(eventName);

        public void UnblockEvent(EventName eventName) => blockedEvents.TryTake(out EventName ev);
    }
}
