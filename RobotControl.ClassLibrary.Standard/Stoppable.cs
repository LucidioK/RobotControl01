using System;
using System.Threading;

namespace RobotControl.ClassLibrary
{
    public abstract class Stoppable : IStoppable
    {
        protected AutoResetEvent stillRunning    = new AutoResetEvent(false);

        protected long stopPlease                = 0;

        public virtual void Stop()               => SetStopPlease();

        public virtual bool ShouldWaitWhileStillRunning  => false;

        protected void SetStopPlease()           => Interlocked.Exchange(ref stopPlease, 1);

        protected bool ShouldContinue()          => Interlocked.Read(ref stopPlease) == 0;

        public virtual void WaitWhileStillRunning() => WaitWhileStillRunningInternal(2000);

        protected void FinishedCleaning()        => stillRunning.Set();

        protected void WaitWhileStillRunningInternal(int millisecondsTimeout)
        {
            if (!stillRunning.WaitOne(millisecondsTimeout))
            {
                throw new TimeoutException();
            }
        }

    }
}
