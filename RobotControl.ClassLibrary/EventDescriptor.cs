using System.Drawing;

namespace RobotControl.ClassLibrary
{
    public interface IEventDescriptor
    {
        EventName Name { get; }
        Bitmap Bitmap { get; set; }
        float Value { get; }
        string Detail { get; }
        IState State { get; }
    }

    public class EventDescriptor : IEventDescriptor
    {
        public EventName Name { get; set; }

        public float Value { get; set; }

        public Bitmap Bitmap { get; set; }

        public string Detail { get; set; }

        public IState State { get; set; }
    }
}
