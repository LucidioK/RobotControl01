namespace RobotControl.ClassLibrary
{
    using OpenCvSharp;
    using OpenCvSharp.Extensions;

    using System;
    using System.Drawing;
    using System.IO;
    using System.Threading;

    internal class CameraCapturer : RobotControlBase, ICameraCapturer
    {
        private const string FakeCameraCapturerVideoPath = "FakeCameraCapturer.mp4";
        private VideoCapture videoCapture;
        private object latestBitmapLock = new object();
        private Bitmap latestBitmap;
        private bool fresh = false;
        private bool fake;
        private Thread thread;
        public CameraCapturer(IMediator mediator, bool fake) : base(mediator)
        {
            this.fake = fake;
            this.thread = new Thread(CameraCaptureLoopThread);
            this.thread.Priority = ThreadPriority.AboveNormal;
            this.thread.Start();
            //ThreadPool.QueueUserWorkItem(CameraCaptureLoopThread, this);
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

        private void CameraCaptureLoopThread(/*object state*/)
        {
            TryCatch(() =>
            {
                var cameraCapturer = this; // (CameraCapturer)state;

                cameraCapturer.StartCapture();
                var frame = new Mat();
                while ((frame = cameraCapturer.ReadFromCapturer()) != null)
                {
                    if (Monitor.TryEnter(cameraCapturer.latestBitmapLock))
                    {
                        latestBitmap = BitmapConverter.ToBitmap(frame.Flip(FlipMode.Y));
                        fresh = true;
                        Publish(new EventDescriptor
                        {
                            Name = EventName.NewImageDetected,
                            Bitmap = latestBitmap
                        });
                    }
                }
            });
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
                PublishException(new Exception($"Could not find video {FakeCameraCapturerVideoPath}"));
            }
        }
    }
}