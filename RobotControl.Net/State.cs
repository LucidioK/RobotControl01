
namespace RobotControl.Net
{
    using Newtonsoft.Json;

    using System;

    class State : IState, IPublishTarget
    {
        private RobotState robotState;
        private float obstacleDistance;
        private float uVLevel;
        private float batteryVoltage;
        private float xAcceleration;
        private float yAcceleration;
        private float zAcceleration;
        private float compassHeading;
        private PubSub pubSub     = new PubSub();
        private object lockObject = new object();


        class RobotData
        {
            public float accelX { get; set; }
            public float accelY { get; set; }
            public float accelZ { get; set; }
            public float compass { get; set; }
            public float distance { get; set; }
            public float voltage { get; set; }
            public float uv { get; set; }
            public float status { get; set; }
        }

        public RobotState RobotState
        {
            get { lock(lockObject) { return robotState      ; }}
            set
            {
                var name = Enum.GetName(typeof(RobotState), value);
                var len = name.Length;
                for (var i = len-1; i > 0; i--)
                {
                    if (char.IsUpper(name[i]))
                    {
                        name = name.Substring(0, i) + " " + name.Substring(i);
                    }
                }
                pubSub.Publish(new EventDescriptor { Name = EventName.PleaseSay, Detail = $"State now is: {name}" });
                lock (lockObject)
                {
                    robotState = value;
                }
            }
        }

        public float ObstacleDistance { get { lock(lockObject) { return obstacleDistance; }} set { lock(lockObject) { obstacleDistance = value; }}}
        public float UVLevel          { get { lock(lockObject) { return uVLevel         ; }} set { lock(lockObject) { uVLevel          = value; }}}
        public float BatteryVoltage   { get { lock(lockObject) { return batteryVoltage  ; }} set { lock(lockObject) { batteryVoltage   = value; }}}
        public float XAcceleration    { get { lock(lockObject) { return xAcceleration   ; }} set { lock(lockObject) { xAcceleration    = value; }}}
        public float YAcceleration    { get { lock(lockObject) { return yAcceleration   ; }} set { lock(lockObject) { yAcceleration    = value; }}}
        public float ZAcceleration    { get { lock(lockObject) { return zAcceleration   ; }} set { lock(lockObject) { zAcceleration    = value; }}}
        public float CompassHeading   { get { lock(lockObject) { return compassHeading  ; }} set { lock(lockObject) { compassHeading   = value; }}}

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            if (eventDescriptor.Name == EventName.RobotDataDetected)
            {
                RobotData robotData;
                try
                {
                    robotData = JsonConvert.DeserializeObject<RobotData>(eventDescriptor.Detail);
                    this.BatteryVoltage = robotData.voltage;
                    this.CompassHeading = robotData.compass;
                    this.ObstacleDistance = robotData.distance;
                    this.UVLevel = robotData.uv;
                    this.XAcceleration = robotData.accelX;
                    this.YAcceleration = robotData.accelY;
                    this.ZAcceleration = robotData.accelZ;
                }
                catch (Exception) { }
            }
        }
    }
}
