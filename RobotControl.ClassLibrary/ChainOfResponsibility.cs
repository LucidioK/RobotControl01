namespace RobotControl.ClassLibrary
{
    internal class ChainOfResponsibility
    {
        System.Collections.Concurrent.ConcurrentQueue<IEventDescriptor> eventDescriptors;
        public ChainOfResponsibility(System.Collections.Concurrent.ConcurrentQueue<IEventDescriptor> eventDescriptors) => this.eventDescriptors = eventDescriptors;

        public void Run()
        {

        }
    }
}