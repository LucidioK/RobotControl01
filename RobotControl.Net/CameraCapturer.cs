namespace RobotControl.Net
{
    using OpenCvSharp;
    using OpenCvSharp.Extensions;

    using System;
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Threading;

    internal class CameraCapturer : ISubscriptionTarget
    {
        private VideoCapture videoCapture;
        private readonly Thread cameraThread;
        private object latestBitmapLock = new object();
        private Bitmap latestBitmap;
        private bool fresh = false;

        public CameraCapturer(IState state)
        {
            this.state = state;
            cameraThread = new Thread(new ThreadStart(CameraCaptureLoopThread));
            cameraThread.Start();
        }

        public Bitmap GetLatestBitmap()
        {
            lock (latestBitmapLock)
            {
                if (fresh)
                {
                    fresh = false;
                    return latestBitmap;
                }
                else
                {
                    return null;
                }
            }
        }

        IState state;
        private void CameraCaptureLoopThread()
        {
            Console.Clear();

            videoCapture = new VideoCapture(0);
            videoCapture.Open(0);
            var frame = new Mat();
            while (videoCapture.IsOpened())
            {
                if (videoCapture.Read(frame) && Monitor.TryEnter(latestBitmapLock))
                {
                    latestBitmap = BitmapConverter.ToBitmap(frame.Flip(FlipMode.Y));
                    fresh = true;
                    pubSub.Publish(new EventDescriptor { Name = EventName.NewImageDetected, Bitmap = latestBitmap });
                }
            }
        }

        private PubSub pubSub = new PubSub();
        public void Subscribe(IPublishTarget publisherTarget) => pubSub.Subscribe(publisherTarget);
    }
}