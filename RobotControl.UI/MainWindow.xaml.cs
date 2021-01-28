using Newtonsoft.Json;

using RobotControl.ClassLibrary;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RobotControl.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IPublishTarget
    {
        RobotControl.ClassLibrary.RobotControl RobotControl;
        object robotControlLock = new object();
        List<EventName> handledEvents = new List<EventName>();
        string[] labelsOfObjectsToDetect;
        int baudRate;

        public EventName[] HandledEvents => this.Dispatcher.Invoke<EventName[]>(() => handledEvents.ToArray());

        public MainWindow()
        {
            InitializeComponent();
            foreach (var eventName in typeof(EventName).GetEnumValues())
            {
                handledEvents.Add((EventName)eventName);
            }
        }

        private async void startStop_ClickAsync(object sender, RoutedEventArgs e)
        {
            var content = (string)this.startStop.Content;
            if (content == "Start")
            {
                this.baudRate = int.Parse(this.baudRateComboBox.Text);
                this.startStop.Content = "Stop";

                var useAudio = this.enableAudioCheckBox.IsChecked.GetValueOrDefault();

                this.RobotControl = new ClassLibrary.RobotControl();
                await this.RobotControl.InitializeAsync(labelsOfObjectsToDetect: this.labelsOfObjectsToDetect,
                    baudRate: this.baudRate,
                    UseFakeSpeaker: !useAudio,
                    UseFakeSpeechCommandListener: !useAudio);
                this.RobotControl.Subscribe(this);
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void EnableDisableStartStopButton() =>
            this.startStop.IsEnabled = true;


        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            switch (eventDescriptor.Name)
            {
                case EventName.VoiceCommandDetected:
                    this.Dispatcher.Invoke(()            =>
                    {
                        this.lblLatestCommand.Content    = eventDescriptor.Detail;
                    });
                    break;
                case EventName.RainDetected:
                    break;
                case EventName.ObjectDetected:
                    this.Dispatcher.Invoke(()            =>
                    {
                        this.lblObjectData.Content       = eventDescriptor.Detail;
                        this.objectDetectionImage.Source = BitmapToBitmapImage(eventDescriptor.Bitmap);
                    });
                    break;
                case EventName.SensorValueDetected:
                    break;
                case EventName.NeedToMoveDetected:
                    {
                        var details                      = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventDescriptor.Detail);
                        this.Dispatcher.Invoke(()        =>
                        {
                            this.lblMotorL.Content       = details.ContainsKey("l") ? details["l"].ToString() : "_";
                            this.lblMotorR.Content       = details.ContainsKey("r") ? details["r"].ToString() : "_"; ;
                        });
                        break;
                    }
                case EventName.NewImageDetected:
                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    this.webCamImage.Source = BitmapToBitmapImage(eventDescriptor.Bitmap);
                    //});
                    break;
                case EventName.RawRobotDataDetected:

                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    this.lblAccelX.Content = eventDescriptor.State.XAcceleration.ToString("0.0");
                    //    this.lblAccelY.Content = eventDescriptor.State.YAcceleration.ToString("0.0");
                    //    this.lblAccelZ.Content = eventDescriptor.State.ZAcceleration.ToString("0.0");
                    //    this.lblDistance.Content = eventDescriptor.State.ObstacleDistance.ToString("0.0");
                    //    this.lblVoltage.Content = eventDescriptor.State.BatteryVoltage.ToString("0.0");
                    //    this.lblCompass.Content = eventDescriptor.State.CompassHeading.ToString("0.0");
                    //});
                    break;
                case EventName.RobotData:
                    this.Dispatcher.Invoke(()     =>
                    {
                        this.lblAccelX.Content    = eventDescriptor.State.XAcceleration.ToString("0.0");

                        DisplayAcceleration(this.accelXImage, eventDescriptor.State.XAcceleration, 1f, 2f, 3.5f);
                        DisplayAcceleration(this.accelYImage, eventDescriptor.State.YAcceleration, 1f, 2f, 3.5f);
                        DisplayAcceleration(this.accelZImage, (eventDescriptor.State.ZAcceleration -10) % 10, 1f, 2f, 3.5f);
                        DisplayCompass(this.CompassImage, eventDescriptor.State.CompassHeading);
                        this.lblAccelY.Content    = eventDescriptor.State.YAcceleration.ToString("0.0");
                        this.lblAccelZ.Content    = eventDescriptor.State.ZAcceleration.ToString("0.0");
                        this.lblDistance.Content  = eventDescriptor.State.ObstacleDistance.ToString("0.0");
                        this.lblVoltage.Content   = eventDescriptor.State.BatteryVoltage.ToString("0.0");
                        this.lblCompass.Content   = eventDescriptor.State.CompassHeading.ToString("0.0");
                    });
                    break;
                case EventName.PleaseSay:
                    this.Dispatcher.Invoke(()     =>
                    {
                        this.lblPleaseSay.Content = eventDescriptor.Detail;
                    });
                    break;
                case EventName.Exception:

                    break;

            }
        }

        private void DisplayCompass(System.Windows.Controls.Image compassImage, float compassHeading)
        {
            var bitmap = new Bitmap((int)compassImage.Width, (int)compassImage.Height);
            var backgroundColor = new System.Drawing.SolidBrush(System.Drawing.Color.LightYellow);

            // We receive 0 as the top quadrant, while Math considers 0 as the right quadrant.
            compassHeading = (float)((((double)compassHeading + 90) % 360) * Math.PI / 180.0);
            System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Black, 1);

            using (var gr = Graphics.FromImage(bitmap))
            {
                gr.FillRectangle(backgroundColor, 0, 0, bitmap.Width, bitmap.Height);
                float hw = (float)(bitmap.Width / 2.0);
                float hh = (float)(bitmap.Height / 2.0);
                float x1 = hw;
                float y1 = hh;
                float si = (float)Math.Sin(compassHeading) * -1;
                float co = (float)Math.Cos(compassHeading) * -1;
                float x2 = x1 + (hw * co);
                float y2 = y1 + (hh * si);
                gr.DrawLine(blackPen, x1, y1, x2, y2);
            }

            compassImage.Source = BitmapToBitmapImage(bitmap);
        }

        private void DisplayAcceleration(System.Windows.Controls.Image accelDisplay, float acceleration, float greenThreshold, float yellowThreshold, float redThreshold)
        {
            var bitmap = new Bitmap((int)accelDisplay.Width, (int)accelDisplay.Height);
            System.Drawing.Brush backgroundColor = null;
            if (Math.Abs(acceleration) >= redThreshold) backgroundColor = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
            else if (Math.Abs(acceleration) >= yellowThreshold) backgroundColor = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow);
            else backgroundColor = new System.Drawing.SolidBrush(System.Drawing.Color.Green);
            using (var gr = Graphics.FromImage(bitmap))
            {
                gr.FillRectangle(backgroundColor, 0, 0, bitmap.Width, bitmap.Height);
                float x1 = (float)(bitmap.Width / 2.0);
                float y1 = bitmap.Height - 1;
                float x2 = x1 + bitmap.Width * (acceleration / 10);
                float y2 = (acceleration / 10) * bitmap.Height;
                gr.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Black, 1), x1, y1, x2, y2);
            }

            accelDisplay.Source = BitmapToBitmapImage(bitmap);
        }

        private BitmapImage BitmapToBitmapImage(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private void objectsToDetectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var labels = new List<string>();
            foreach (var item in objectsToDetectComboBox.Items)
            {
                var cb = item as CheckBox;
                if (cb != null && cb.IsChecked.GetValueOrDefault())
                {
                    labels.Add(cb.Content.ToString());
                }

            }

            labelsOfObjectsToDetect = labels.ToArray();
            startStop.IsEnabled = labels.Count > 0 && startStop.Content.ToString() == "Start";
        }

        private void objectsToDetectSelectionChanged(object sender, RoutedEventArgs e)
        {
            objectsToDetectComboBox_SelectionChanged(null, null);
        }
    }
}
