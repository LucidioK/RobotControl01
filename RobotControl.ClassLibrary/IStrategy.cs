namespace RobotControl.Net
{
    public interface IStrategy
    {
        IEventDescriptor Run(IEventDescriptor eventDescriptor);
    }
}
