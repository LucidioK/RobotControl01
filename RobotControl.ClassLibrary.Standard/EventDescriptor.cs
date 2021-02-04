using System.Drawing;
using System.Threading;

namespace RobotControl.ClassLibrary
{
    public interface IEventDescriptor
    {
        EventName Name { get; }
        Bitmap Bitmap { get; set; }
        float Value { get; }
        string Detail { get; }
        IState State { get; }
        AutoResetEvent WaitEvent { get; }
        void EventWasProcessed();
    }

    public class EventDescriptor : IEventDescriptor
    {
        public EventName Name { get; set; }

        public float Value { get; set; }

        public Bitmap Bitmap { get; set; }

        public string Detail { get; set; }

        public IState State { get; set; }

        AutoResetEvent waitEvent = new AutoResetEvent(true);
        public AutoResetEvent WaitEvent => waitEvent;
        public void EventWasProcessed() { waitEvent.Set();  }
    }
}
