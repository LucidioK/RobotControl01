﻿
namespace RobotControl.Net
{
    using Newtonsoft.Json;

    using System;

    class RobotData
    {
        public float accelX { get; set; }
        public float accelY { get; set; }
        public float accelZ { get; set; }
        public float compass { get; set; }
        public float distance { get; set; }
        public float voltage { get; set; }
        public float uv { get; set; }
        public string status { get; set; }
    }

    class State : IState
    {
        private RobotState robotState;
        private float obstacleDistance;
        private float uVLevel;
        private float batteryVoltage;
        private float xAcceleration;
        private float yAcceleration;
        private float zAcceleration;
        private float compassHeading;
        private object lockObject = new object();

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

        private PubSub pubSub = new PubSub();
        public void Subscribe(IPublishTarget publisherTarget) => pubSub.Subscribe(publisherTarget);

        static public State FromRobotData(RobotData data) =>
            new State
            {
                BatteryVoltage = data.voltage,
                CompassHeading = data.compass,
                ObstacleDistance = data.distance,
                UVLevel = data.uv,
                XAcceleration = data.accelX,
                YAcceleration = data.accelY,
                ZAcceleration = data.accelZ,
            };

        public float ObstacleDistance { get { lock(lockObject) { return obstacleDistance; }} set { lock(lockObject) { obstacleDistance = value; }}}
        public float UVLevel          { get { lock(lockObject) { return uVLevel         ; }} set { lock(lockObject) { uVLevel          = value; }}}
        public float BatteryVoltage   { get { lock(lockObject) { return batteryVoltage  ; }} set { lock(lockObject) { batteryVoltage   = value; }}}
        public float XAcceleration    { get { lock(lockObject) { return xAcceleration   ; }} set { lock(lockObject) { xAcceleration    = value; }}}
        public float YAcceleration    { get { lock(lockObject) { return yAcceleration   ; }} set { lock(lockObject) { yAcceleration    = value; }}}
        public float ZAcceleration    { get { lock(lockObject) { return zAcceleration   ; }} set { lock(lockObject) { zAcceleration    = value; }}}
        public float CompassHeading   { get { lock(lockObject) { return compassHeading  ; }} set { lock(lockObject) { compassHeading   = value; }}}
        public EventName[] HandledEvents => new EventName[] { EventName.RawRobotDataDetected };

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            if (eventDescriptor.Name == EventName.RawRobotDataDetected)
            {
                RobotData robotData;
                try
                {
                    System.Diagnostics.Debug.WriteLine($"-->DATA FROM ROBOT: {eventDescriptor.Detail}");
                    robotData = JsonConvert.DeserializeObject<RobotData>(eventDescriptor.Detail);
                    if (!string.IsNullOrEmpty(robotData.status))
                    {
                        Console.WriteLine($"Robot returned status [{robotData.status}]");
                    }
                    var state = (IState)new State()
                    {
                        BatteryVoltage = robotData.voltage,
                        CompassHeading = robotData.compass,
                        ObstacleDistance = robotData.distance,
                        UVLevel = robotData.uv,
                        XAcceleration = robotData.accelX,
                        YAcceleration = robotData.accelY,
                        ZAcceleration = robotData.accelZ,
                    };
                    var ev = new EventDescriptor { Name = EventName.RobotData, };
                    ev.State = state;
                    pubSub.Publish(ev);

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"--> {nameof(State)}:{nameof(OnEvent)} exception: {ex.Message}/{ex.StackTrace}");
                }
            }
        }
    }
}
