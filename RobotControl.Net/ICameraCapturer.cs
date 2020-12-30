using System.Drawing;

namespace RobotControl.Net
{
    internal interface ICameraCapturer: ISubscriptionTarget
    {
        Bitmap GetLatestBitmap();
    }
}