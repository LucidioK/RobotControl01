using System.Drawing;

namespace RobotControl.Net
{
    public interface ICameraCapturer: ISubscriptionTarget
    {
        Bitmap GetLatestBitmap();
    }
}