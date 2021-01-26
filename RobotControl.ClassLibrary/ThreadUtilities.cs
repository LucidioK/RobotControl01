using System.Threading;

namespace RobotControl.ClassLibrary
{
    public static class ThreadUtilities
    {
        public static bool ThreadWithTimeout(ThreadStart action, int timeoutInMilliseconds)
        {
            var thread = new Thread(action);
            thread.Start();
            return thread.Join(timeoutInMilliseconds);
        }
    }
}
