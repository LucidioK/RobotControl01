namespace RobotControl.Net
{
    interface IStrategy
    {
        IEventDescriptor Run(IEventDescriptor eventDescriptor);
    }
}
