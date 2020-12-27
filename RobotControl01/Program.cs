

using Microsoft.ML;

using OnnxObjectDetection;



namespace RobotControl01
{
    using OpenCvSharp;
    using OpenCvSharp.Extensions;

    using System;
    using System.Drawing;
    using System.Threading;

    class Program
    {
        static VideoCapture videoCapture;
        static Thread cameraThread;
        static TinyYoloModel tinyYoloModel;
        static OnnxModelConfigurator onnxModelConfigurator;
        static OnnxOutputParser onnxOutputParser;
        static PredictionEngine<ImageInputData, TinyYoloPrediction> tinyYoloPredictionEngine;
        //static SpeechRecognitionEngine speechRecognitionEngine
        static void Main(string[] args)
        {


            tinyYoloModel = new TinyYoloModel(@"TinyYolo2_model.onnx");
            onnxModelConfigurator = new OnnxModelConfigurator(tinyYoloModel);
            onnxOutputParser = new OnnxOutputParser(tinyYoloModel);
            tinyYoloPredictionEngine = onnxModelConfigurator.GetMlNetPredictionEngine<TinyYoloPrediction>();

            cameraThread = new Thread(new ThreadStart(CameraCaptureLoopThread));
            cameraThread.Start();
            
            while (true) { Thread.Sleep(10);  }
        }

        private static void CameraCaptureLoopThread()
        {
            Console.Clear();

            videoCapture = new VideoCapture(0);
            videoCapture.Open(0);
            var frame = new Mat();
            while (videoCapture.IsOpened())
            {
                if (videoCapture.Read(frame))
                {
                    Console.CursorTop = 0;
                    Console.CursorLeft = 0;
                    Bitmap bitmap = BitmapConverter.ToBitmap(frame.Flip(FlipMode.Y));
                    var boxes = DetectObjects(bitmap);
                    var midXImg = bitmap.Width / 2;
                    Console.WriteLine(DateTime.Now);
                    var corrX = (float)bitmap.Width / ImageSettings.imageWidth;
                    var corrY = (float)bitmap.Height / ImageSettings.imageHeight;
                    float x=0, y=0, w=0, h=0;
                    foreach (var box in boxes)
                    {
                        x = box.Dimensions.X * corrX;
                        y = box.Dimensions.Y * corrY;
                        w = box.Dimensions.Width * corrX;
                        h = box.Dimensions.Height * corrY;
                        var midXBox = (box.Dimensions.X * corrX) + (box.Dimensions.Width * corrX / 2);
                        var position = midXBox - midXImg;
                        Console.WriteLine($"{box.Label} {(int)(box.Confidence*100)}% bitmap width: {bitmap.Width} Box X:{(int)box.Dimensions.X} Box Width: {(int)box.Dimensions.Width} position: {position}");
                    }
                    if (w != 0)
                    {
                        using (var gr = Graphics.FromImage(bitmap))
                        {
                            gr.DrawRectangle(new Pen(Color.Red, 3), x, y, w, h);
                        }
                    }
                    bitmap.Save("captured.bmp");

                    Console.WriteLine();

                }
            }
        }

        private static BoundingBox[] DetectObjects(Bitmap bitmap)
        {
            var prediction = tinyYoloPredictionEngine.Predict(new ImageInputData { Image = bitmap });
            var labels = prediction.PredictedLabels;
            var boundingBoxes = onnxOutputParser.ParseOutputs(labels);
            var filteredBoxes = onnxOutputParser.FilterBoundingBoxes(boundingBoxes, 5, 0.5f);
            return filteredBoxes.ToArray();
        }
    }
}
