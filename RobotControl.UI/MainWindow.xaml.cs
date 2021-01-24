using Newtonsoft.Json;

using RobotControl.Net;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RobotControl.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Net.IPublishTarget
    {
        RobotControl.Net.RobotControl RobotControl;
        List<EventName> handledEvents = new List<EventName>();
        BitmapImage WebCamImageSource = new BitmapImage();
        public EventName[] HandledEvents => handledEvents.ToArray();

        public MainWindow()
        {
            InitializeComponent();
            foreach (var eventName in typeof(EventName).GetEnumValues())
            {
                handledEvents.Add((EventName)eventName);
            }
        }

        private void startStop_Click(object sender, RoutedEventArgs e)
        {
            //if (this.startStop.Content == "Start")
            //{
                var labels = GetLabelsOfObjectsToDetect();
                RobotControl = new Net.RobotControl(labels);
                RobotControl.Subscribe(this);
                this.startStop.IsEnabled = false;
            //}
            //else
            //{

            //}
        }

        private string[] GetLabelsOfObjectsToDetect()
        {
            var labels = new List<string>();
            if (checkBoxAeroplane.IsChecked.GetValueOrDefault())   { labels.Add("aeroplane");   }
            if (checkBoxBicycle.IsChecked.GetValueOrDefault())     { labels.Add("bicycle");     }
            if (checkBoxBird.IsChecked.GetValueOrDefault())        { labels.Add("bird");        }
            if (checkBoxBoat.IsChecked.GetValueOrDefault())        { labels.Add("boat");        }
            if (checkBoxBottle.IsChecked.GetValueOrDefault())      { labels.Add("bottle");      }
            if (checkBoxBus.IsChecked.GetValueOrDefault())         { labels.Add("bus");         }
            if (checkBoxCar.IsChecked.GetValueOrDefault())         { labels.Add("car");         }
            if (checkBoxCat.IsChecked.GetValueOrDefault())         { labels.Add("cat");         }
            if (checkBoxChair.IsChecked.GetValueOrDefault())       { labels.Add("chair");       }
            if (checkBoxCow.IsChecked.GetValueOrDefault())         { labels.Add("cow");         }
            if (checkBoxDiningtable.IsChecked.GetValueOrDefault()) { labels.Add("diningtable"); }
            if (checkBoxDog.IsChecked.GetValueOrDefault())         { labels.Add("dog");         }
            if (checkBoxHorse.IsChecked.GetValueOrDefault())       { labels.Add("horse");       }
            if (checkBoxMotorbike.IsChecked.GetValueOrDefault())   { labels.Add("motorbike");   }
            if (checkBoxPerson.IsChecked.GetValueOrDefault())      { labels.Add("person");      }
            if (checkBoxPottedplant.IsChecked.GetValueOrDefault()) { labels.Add("pottedplant"); }
            if (checkBoxSheep.IsChecked.GetValueOrDefault())       { labels.Add("sheep");       }
            if (checkBoxSofa.IsChecked.GetValueOrDefault())        { labels.Add("sofa");        }
            if (checkBoxTrain.IsChecked.GetValueOrDefault())       { labels.Add("train");       }
            if (checkBoxTvmonitor.IsChecked.GetValueOrDefault())   { labels.Add("tvmonitor");   }
            return labels.ToArray();
        }

        private void checkBoxAeroplane_Checked(object sender, RoutedEventArgs e)    => EnableDisableStartStopButton();
        private void checkBoxBicycle_Checked(object sender, RoutedEventArgs e)      => EnableDisableStartStopButton();
        private void checkBoxBird_Checked(object sender, RoutedEventArgs e)         => EnableDisableStartStopButton();
        private void checkBoxBoat_Checked(object sender, RoutedEventArgs e)         => EnableDisableStartStopButton();
        private void checkBoxBottle_Checked(object sender, RoutedEventArgs e)       => EnableDisableStartStopButton();
        private void checkBoxBus_Checked(object sender, RoutedEventArgs e)          => EnableDisableStartStopButton();
        private void checkBoxCar_Checked(object sender, RoutedEventArgs e)          => EnableDisableStartStopButton();
        private void checkBoxCat_Checked(object sender, RoutedEventArgs e)          => EnableDisableStartStopButton();
        private void checkBoxChair_Checked(object sender, RoutedEventArgs e)        => EnableDisableStartStopButton();
        private void checkBoxCow_Checked(object sender, RoutedEventArgs e)          => EnableDisableStartStopButton();
        private void checkBoxDiningtable_Checked(object sender, RoutedEventArgs e)  => EnableDisableStartStopButton();
        private void checkBoxDog_Checked(object sender, RoutedEventArgs e)          => EnableDisableStartStopButton();
        private void checkBoxHorse_Checked(object sender, RoutedEventArgs e)        => EnableDisableStartStopButton();
        private void checkBoxMotorbike_Checked(object sender, RoutedEventArgs e)    => EnableDisableStartStopButton();
        private void checkBoxPerson_Checked(object sender, RoutedEventArgs e)       => EnableDisableStartStopButton();
        private void checkBoxPottedplant_Checked(object sender, RoutedEventArgs e)  => EnableDisableStartStopButton();
        private void checkBoxSheep_Checked(object sender, RoutedEventArgs e)        => EnableDisableStartStopButton();
        private void checkBoxSofa_Checked(object sender, RoutedEventArgs e)         => EnableDisableStartStopButton();
        private void checkBoxTrain_Checked(object sender, RoutedEventArgs e)        => EnableDisableStartStopButton();
        private void checkBoxTvmonitor_Checked(object sender, RoutedEventArgs e)    => EnableDisableStartStopButton();

        private void EnableDisableStartStopButton()                                 =>
            this.startStop.IsEnabled =
                checkBoxAeroplane.IsChecked.GetValueOrDefault() ||
                checkBoxBicycle.IsChecked.GetValueOrDefault() ||
                checkBoxBird.IsChecked.GetValueOrDefault() ||
                checkBoxBoat.IsChecked.GetValueOrDefault() ||
                checkBoxBottle.IsChecked.GetValueOrDefault() ||
                checkBoxBus.IsChecked.GetValueOrDefault() ||
                checkBoxCar.IsChecked.GetValueOrDefault() ||
                checkBoxCat.IsChecked.GetValueOrDefault() ||
                checkBoxChair.IsChecked.GetValueOrDefault() ||
                checkBoxCow.IsChecked.GetValueOrDefault() ||
                checkBoxDiningtable.IsChecked.GetValueOrDefault() ||
                checkBoxDog.IsChecked.GetValueOrDefault() ||
                checkBoxHorse.IsChecked.GetValueOrDefault() ||
                checkBoxMotorbike.IsChecked.GetValueOrDefault() ||
                checkBoxPerson.IsChecked.GetValueOrDefault() ||
                checkBoxPottedplant.IsChecked.GetValueOrDefault() ||
                checkBoxSheep.IsChecked.GetValueOrDefault() ||
                checkBoxSofa.IsChecked.GetValueOrDefault() ||
                checkBoxTrain.IsChecked.GetValueOrDefault() ||
                checkBoxTvmonitor.IsChecked.GetValueOrDefault();

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            switch (eventDescriptor.Name)
            {
                case EventName.VoiceCommandDetected:
                    lblLatestCommand.Content = eventDescriptor.Detail;
                    break;
                case EventName.RainDetected: 
                    break;
                case EventName.ObjectDetected:
                    this.lblObjectData.Content = eventDescriptor.Detail;
                    this.objectDetectionImage.Source = BitmapToBitmapImage(eventDescriptor.Bitmap);
                    break;
                case EventName.SensorValueDetected: 
                    break;
                case EventName.NeedToMoveDetected:
                    var details = JsonConvert.DeserializeObject<Dictionary<string,object>>(eventDescriptor.Detail);
                    lblMotorL.Content = details.ContainsKey("l") ? details["l"].ToString() : "_";
                    lblMotorR.Content = details.ContainsKey("r") ? details["r"].ToString() : "_"; ;
                    break;
                case EventName.NewImageDetected:
                    this.webCamImage.Source = BitmapToBitmapImage(eventDescriptor.Bitmap);
                    break;
                case EventName.RawRobotDataDetected: 
                    break;
                case EventName.RobotData:
                    lblAccelX.Content = eventDescriptor.State.XAcceleration.ToString("0.0");
                    lblAccelY.Content = eventDescriptor.State.YAcceleration.ToString("0.0");
                    lblAccelZ.Content = eventDescriptor.State.ZAcceleration.ToString("0.0");
                    lblDistance.Content = eventDescriptor.State.ObstacleDistance.ToString("0.0");
                    lblVoltage.Content = eventDescriptor.State.BatteryVoltage.ToString("0.0");
                    lblCompass.Content = eventDescriptor.State.CompassHeading.ToString("0.0");
                    break;
                case EventName.PleaseSay:
                    lblPleaseSay.Content = eventDescriptor.Detail;
                    break;

            }
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
    }
}
