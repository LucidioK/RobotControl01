using System.Collections.Concurrent;
using System.Linq;

namespace RobotControl.Net
{
    class PubSub
    {
        protected ConcurrentQueue<IPublishTarget> publishTargets = new ConcurrentQueue<IPublishTarget>();
        public void Subscribe(IPublishTarget publisher)          => publishTargets.Enqueue(publisher);
        public void Publish(IEventDescriptor eventDescriptor)    => publishTargets.ToList().ForEach(t => t.OnEvent(eventDescriptor));
    }
}
