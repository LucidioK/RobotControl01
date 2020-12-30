using System.Drawing;

namespace RobotControl.Net
{
    public interface IObjectDetector : ISubscriptionTarget, IPublishTarget
    {
        int ImageHeight { get; }
        int ImageWidth { get; }

        ObjectDetector.BoundingBox[] DetectObjects(Bitmap bitmap);
    }
}