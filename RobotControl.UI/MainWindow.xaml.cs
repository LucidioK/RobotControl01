using Newtonsoft.Json;

using RobotControl.ClassLibrary;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RobotControl.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IPublishTarget
    {
        RobotControl.ClassLibrary.RobotControl RobotControl;
        TimeChart accelXTimeChart;
        TimeChart accelYTimeChart;
        TimeChart accelZTimeChart;
        object exceptionLock = new object();
        object startStopLock = new object();
        float latestCompassHeading = 0.0f;
        List<EventName> handledEvents = new List<EventName>();
        string[] labelsOfObjectsToDetect;
        string configurationPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RobotControlConfiguration.json");
        Configuration configuration = new Configuration();

        int baudRate;
        ConcurrentBag<string> exceptionsToBeIgnored = new ConcurrentBag<string>();
        private bool alreadyConfigured = false;

        public EventName[] HandledEvents => this.Dispatcher.Invoke<EventName[]>(() => handledEvents.ToArray());

        public bool ShouldWaitWhileStillRunning => throw new NotImplementedException();

        public MainWindow()
        {
            InitializeComponent();
            accelXTimeChart = new TimeChart(accelXChart, -10, 10, TimeSpan.FromMilliseconds(100));
            accelYTimeChart = new TimeChart(accelYChart, -10, 10, TimeSpan.FromMilliseconds(100));
            accelZTimeChart = new TimeChart(accelZChart, -10, 10, TimeSpan.FromMilliseconds(100));
            foreach (var eventName in typeof(EventName).GetEnumValues())
            {
                handledEvents.Add((EventName)eventName);
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!this.alreadyConfigured)
            {
                this.alreadyConfigured = true;
                PopulateConfigurationData();
                HandleConfigurationData();
            }
        }

        private async void startStop_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (Monitor.TryEnter(startStopLock))
            {
                var previousCursor = this.Cursor;
                try
                {
                    this.Cursor = Cursors.Wait;
                    var content = (string)this.startStop.Content;
                    SaveConfigurationData();
                    if (content == "Start")
                    {
                        this.baudRate = int.Parse(this.baudRateComboBox.Text);
                        this.startStop.Content = "Stop";

                        var useAudio = this.enableAudioCheckBox.IsChecked.GetValueOrDefault();

                        this.RobotControl = new ClassLibrary.RobotControl();
                        await this.RobotControl.InitializeAsync(labelsOfObjectsToDetect: this.labelsOfObjectsToDetect,
                            baudRate: this.baudRate,
                            UseFakeSpeaker: !useAudio,
                            UseFakeSpeechCommandListener: !useAudio,
                            LMultiplier: this.configuration.LeftMotorMultiplier,
                            RMultiplier: this.configuration.RightMotorMultiplier);
                        this.RobotControl.Subscribe(this);
                        this.textMotors.IsEnabled = true;
                        this.runMotors.IsEnabled = true;
                        this.scanForObjects_Checked(null, null);
                    }
                    else
                    {
                        var eventDescriptor = new EventDescriptor { Name = EventName.NeedToMoveDetected, Detail = "{'operation':'motor','l':0,'r':0}" };
                        this.RobotControl.Publish(eventDescriptor);
                        eventDescriptor.WaitEvent.WaitOne();
                        Environment.Exit(0);
                    }

                }
                finally
                {
                    this.Cursor = previousCursor;
                    Monitor.Exit(startStopLock);
                }
            }
        }

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
                        this.objectDetectionImage.Source = Utilities.BitmapToBitmapImage(eventDescriptor.Bitmap);
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
                case EventName.MotorCalibrationResponse:

                    break;
                case EventName.RobotData:
                    this.Dispatcher.Invoke(()     =>
                    {
                        this.lblAccelX.Content    = eventDescriptor.State.XAcceleration.ToString("0.0");

                        DisplayAcceleration(this.accelXImage, this.accelXTimeChart, eventDescriptor.State.XAcceleration, 1f, 2f, 3.5f);
                        DisplayAcceleration(this.accelYImage, this.accelYTimeChart, eventDescriptor.State.YAcceleration, 1f, 2f, 3.5f);
                        DisplayAcceleration(this.accelZImage, this.accelZTimeChart, (eventDescriptor.State.ZAcceleration -10) % 10, 1f, 2f, 3.5f);
                        DisplayCompass(this.CompassImage, eventDescriptor.State.CompassHeading);
                        this.lblAccelY.Content    = eventDescriptor.State.YAcceleration.ToString("0.0");
                        this.lblAccelZ.Content    = eventDescriptor.State.ZAcceleration.ToString("0.0");
                        this.lblDistance.Content  = eventDescriptor.State.ObstacleDistance.ToString("0.0");
                        this.lblVoltage.Content   = eventDescriptor.State.BatteryVoltage.ToString("0.0");
                        this.lblCompass.Content   = eventDescriptor.State.CompassHeading.ToString("0.0");
                        Interlocked.Exchange(ref this.latestCompassHeading, eventDescriptor.State.CompassHeading);
                    });
                    break;
                case EventName.PleaseSay:
                    this.Dispatcher.Invoke(()     =>
                    {
                        this.lblPleaseSay.Content = eventDescriptor.Detail;
                    });
                    break;
                case EventName.Exception:
                    lock (exceptionLock)
                    {
                        if (exceptionsToBeIgnored.Contains(RemoveNumbers(eventDescriptor.Detail)))
                        {
                            break;
                        }

                        var choice = MessageBox.Show($"Exception:\n{eventDescriptor.Detail}\nYes to ignore this exception, No to continue, Cancel to exit.", "Exception", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                        if (choice == MessageBoxResult.Cancel)
                        {
                            Environment.Exit(1);
                        }

                        if (choice == MessageBoxResult.Yes)
                        {
                            exceptionsToBeIgnored.Add(RemoveNumbers(eventDescriptor.Detail));
                        }

                        break;
                    }

            }
        }

        private string RemoveNumbers(string s) => s.Replace("0", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "").Replace("5", "").Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "");

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

            compassImage.Source = Utilities.BitmapToBitmapImage(bitmap);
        }

        private void DisplayAcceleration(
            System.Windows.Controls.Image accelDisplay,
            TimeChart timeChart,
            float acceleration,
            float greenThreshold,
            float yellowThreshold,
            float redThreshold)
        {
            var bitmap = new Bitmap((int)accelDisplay.Width, (int)accelDisplay.Height);
            System.Drawing.Color backgroundColor = System.Drawing.Color.Green;
            if (Math.Abs(acceleration) >= redThreshold) backgroundColor = System.Drawing.Color.Red;
            else if (Math.Abs(acceleration) >= yellowThreshold) backgroundColor = System.Drawing.Color.Yellow;
            var backgroundBrush = new System.Drawing.SolidBrush(backgroundColor);
            using (var gr = Graphics.FromImage(bitmap))
            {
                gr.FillRectangle(backgroundBrush, 0, 0, bitmap.Width, bitmap.Height);
                float x1 = (float)(bitmap.Width / 2.0);
                float y1 = bitmap.Height - 1;
                float x2 = x1 + bitmap.Width * (acceleration / 10);
                float y2 = (acceleration / 10) * bitmap.Height;
                gr.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Black, 1), x1, y1, x2, y2);
            }

            accelDisplay.Source = Utilities.BitmapToBitmapImage(bitmap);

            timeChart?.Post(acceleration, backgroundColor);
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

        private async void calibrateCompass_Click(object sender, RoutedEventArgs e)
        {
            this.baudRate = int.Parse(this.baudRateComboBox.Text);

            this.RobotControl = new ClassLibrary.RobotControl();
            await this.RobotControl.InitializeBasicAsync(baudRate: this.baudRate);
            this.RobotControl.Subscribe(this);
            float north, south, east;
            if ((north = askForCompassHeading("Point the front of the robot towards North")) == float.NegativeInfinity)
            {
                return;
            }

            if ((east = askForCompassHeading("Now, point the front of the robot towards East")) == float.NegativeInfinity)
            {
                return;
            }

            if ((south = askForCompassHeading("OK, now point the front of the robot towards South")) == float.NegativeInfinity)
            {
                return;
            }



        }

        private float askForCompassHeading(string message)
        {
            var choice = MessageBox.Show(message, "Calibrating Compass", MessageBoxButton.OKCancel, MessageBoxImage.None);

            if (choice == MessageBoxResult.Cancel)
            {
                return float.NegativeInfinity;
            }

            return getAverageCompassHeading();
        }

        private float getAverageCompassHeading()
        {
            float heading = 0;
            for (var i = 0; i < 4; i++)
            {
                heading += this.latestCompassHeading;
                Thread.Sleep(100);
            }

            return heading / 4;
        }

        private void calibrateMotors_Click(object sender, RoutedEventArgs e)
        {
            var choice = MessageBox.Show("Place the robot in a flat place with\nat least 2m of space ahead and 1m each side.\nThen click OK to continue.", "Calibrate Motors", MessageBoxButton.OKCancel);
            if (choice == MessageBoxResult.Cancel)
            {
                return;
            }

            RobotControl.Publish(new EventDescriptor { Name = EventName.MotorCalibrationRequest });
        }

        public void Stop()
        {
        }

        public void WaitWhileStillRunning()
        {
        }

        private void textMotors_Click(object sender, RoutedEventArgs e)
        {
            StopDetectingObjects();
            RobotControl.Publish(new EventDescriptor { Name = EventName.NeedToMoveDetected, Detail = $"{{'operation':'motor','l':0,'r':0}}" });

            int r = (int)(250 * float.Parse(this.RMult.Text));
            int l = (int)(-250 * float.Parse(this.LMult.Text));
            RobotControl.Publish(new EventDescriptor { Name = EventName.NeedToMoveDetected, Detail = $"{{'operation':'motor','l':{l},'r':{r}}}" });
            Thread.Sleep(500);
            RobotControl.Publish(new EventDescriptor { Name = EventName.NeedToMoveDetected, Detail = $"{{'operation':'motor','l':0,'r':0}}" });
            scanForObjects_Checked(null, null);
        }

        private void scanForObjects_Checked(object sender, RoutedEventArgs e)
        {
            if (RobotControl != null)
            {
                if (IsChecked(this.scanForObjects))
                {
                    StartDetectingObects();
                }
                else
                {
                    StopDetectingObjects();
                }
            }
        }

        private void StartDetectingObects()
        {
            RobotControl.UnblockEvent(EventName.NewImageDetected);
            RobotControl.UnblockEvent(EventName.ObjectDetected);
        }

        private void StopDetectingObjects()
        {
            RobotControl.BlockEvent(EventName.NewImageDetected);
            RobotControl.BlockEvent(EventName.ObjectDetected);
        }

        private void PopulateConfigurationData()
        {
            if (File.Exists(this.configurationPath))
            {
                string config = File.ReadAllText(this.configurationPath);
                this.configuration = JsonConvert.DeserializeObject<Configuration>(config);
            }
        }

        private void SaveConfigurationData()
        {
            this.configuration.EnableAudio          = IsChecked(this.enableAudioCheckBox);
            this.configuration.ScanForObjects       = IsChecked(this.scanForObjects);
            this.configuration.LeftMotorMultiplier  = float.Parse(this.LMult.Text);
            this.configuration.RightMotorMultiplier = float.Parse(this.RMult.Text);
            this.configuration.SerialPortBaudrate   = this.baudRate;

            this.configuration.ObjectsToDetect.Clear();
            for (int i = 0; i < this.objectsToDetectComboBox.Items.Count; i++)
            {
                CheckBox checkBox                   = (CheckBox)this.objectsToDetectComboBox.Items[i];
                if (IsChecked(checkBox))
                {
                    this.configuration.ObjectsToDetect.Add((string)checkBox.Content);
                }
            }

            File.WriteAllText(this.configurationPath, JsonConvert.SerializeObject(this.configuration));
        }


        private void HandleConfigurationData()
        {
            this.enableAudioCheckBox.IsChecked      = this.configuration.EnableAudio;
            this.scanForObjects.IsChecked           = this.configuration.ScanForObjects;
            this.LMult.Text                         = this.configuration.LeftMotorMultiplier.ToString("0.00");
            this.RMult.Text                         = this.configuration.RightMotorMultiplier.ToString("0.00");
            this.baudRate                           = this.configuration.SerialPortBaudrate;
            this.baudRateComboBox.SelectedValue     = this.baudRate.ToString();
            this.baudRateComboBox.Text              = this.baudRate.ToString();

            for (int i                              = 0; i < this.objectsToDetectComboBox.Items.Count; i++)
            {
                CheckBox checkBox                   = (CheckBox)this.objectsToDetectComboBox.Items[i];
                checkBox.IsChecked                  = this.configuration.ObjectsToDetect.Contains(checkBox.Content);
            }
        }

        private bool IsChecked(CheckBox checkBox) => checkBox.IsChecked.HasValue ? checkBox.IsChecked.Value : false;

        private void saveConfiguration_Click(object sender, RoutedEventArgs e) => SaveConfigurationData();

        private void runMotors_Click(object sender, RoutedEventArgs e) =>
            this.RobotControl.Publish(
                new EventDescriptor
                {
                    Name = EventName.NeedToMoveDetected,
                    Detail = $"{{'operation':'timedmotor','l':{(int)(int.Parse(this.LPower.Text) * this.configuration.LeftMotorMultiplier)},'r':{(int)(int.Parse(this.RPower.Text) * this.configuration.RightMotorMultiplier)},'t':{this.TimeToRun.Text}}}"
                });
    }
}
