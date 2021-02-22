using System.Collections.Generic;

namespace RobotControl.UI
{
    public class Configuration
    {
        public HashSet<string> ObjectsToDetect      { get; set; } = new HashSet<string>(){ "person" };
        public float    LeftMotorMultiplier  { get; set; } = 1.0f;
        public float    RightMotorMultiplier { get; set; } = 1.0f;
        public int      SerialPortBaudrate   { get; set; } = 115200;
        public bool     EnableAudio          { get; set; } = true;
        public bool     ScanForObjects        { get; set; } = true;
    }
}
