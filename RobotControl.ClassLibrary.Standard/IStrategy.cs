namespace RobotControl.ClassLibrary
{
    public interface IStrategy
    {
        IEventDescriptor Run(IEventDescriptor eventDescriptor);
    }
}
