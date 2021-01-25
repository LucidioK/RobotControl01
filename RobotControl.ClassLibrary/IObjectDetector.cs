using System.Drawing;

namespace RobotControl.Net
{
    public interface IObjectDetector : ISubscriptionTarget, IPublishTarget
    {
        int ImageHeight { get; }
        int ImageWidth { get; }

        void DetectObjects(Bitmap bitmap);
    }
}