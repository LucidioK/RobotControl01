using OpenCvSharp;

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace RobotControl.ClassLibrary
{
    public abstract class RobotControlBase
    {
        protected IMediator mediator;

        public RobotControlBase(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public void Subscribe(IPublishTarget publisher) => mediator.Subscribe(publisher);
        public void Publish(IEventDescriptor eventDescriptor) => mediator.OnEvent(eventDescriptor);

        public void TryCatch(Action a, Action finallyAction = null)
        {
            try
            {
                a.Invoke();
            }
            catch (Exception ex)
            {
                PublishException(ex);
            }
            finally
            {
                finallyAction?.Invoke();
            }
        }

        public void PublishException(Exception ex) =>
            Publish(new EventDescriptor
            {
                Name = EventName.Exception,
                Detail = $"{ex.Message}\n{ex.StackTrace}"
            });
    }
}
