using System.Drawing;

namespace RobotControl.ClassLibrary
{
    public interface ICameraCapturer: ISubscriptionTarget
    {
        Bitmap GetLatestBitmap();
    }
}