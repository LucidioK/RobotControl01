namespace RobotControl.Net
{
    using OpenCvSharp;
    using OpenCvSharp.Extensions;

    using System;
    using System.Drawing;
    using System.IO;
    using System.Threading;

    internal class CameraCapturer : ICameraCapturer
    {
        private const string FakeCameraCapturerVideoPath = "FakeCameraCapturer.mp4";
        private VideoCapture videoCapture;
        private readonly Thread cameraThread;
        private object latestBitmapLock = new object();
        private Bitmap latestBitmap;
        private bool fresh = false;
        private bool fake;

        public CameraCapturer(bool fake)
        {
            this.fake = fake;
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

        private void CameraCaptureLoopThread()
        {
            StartCapture();
            var frame = new Mat();
            while ((frame = ReadFromCapturer()) != null)
            {
                if (Monitor.TryEnter(latestBitmapLock))
                {
                    latestBitmap = BitmapConverter.ToBitmap(frame.Flip(FlipMode.Y));
                    fresh = true;
                    pubSub.Publish(new EventDescriptor
                    {
                        Name = EventName.NewImageDetected,
                        Bitmap = latestBitmap
                    });
                }
            }
        }

        private Mat ReadFromCapturer()
        {
            Mat frame = new Mat();
            videoCapture.Read(frame);
            if (frame.Empty() && fake)
            {
                videoCapture.Release();
                StartFakeCapture();
                videoCapture.Read(frame);
            }

            if (frame.Empty() && !fake)
            {
                frame = null;
            }

            return frame;
        }

        private void StartCapture()
        {
            if (fake)
            {
                StartFakeCapture();
            }
            else
            {
                videoCapture = new VideoCapture(0);
                videoCapture.Open(0);
            }
        }

        private void StartFakeCapture()
        {
            if (File.Exists(FakeCameraCapturerVideoPath))
            {
                videoCapture = new VideoCapture(FakeCameraCapturerVideoPath);
            }
            else
            {
                throw new Exception($"Could not find video {FakeCameraCapturerVideoPath}");
            }
        }

        private PubSub pubSub = new PubSub();
        public void Subscribe(IPublishTarget publisherTarget) => pubSub.Subscribe(publisherTarget);
    }
}