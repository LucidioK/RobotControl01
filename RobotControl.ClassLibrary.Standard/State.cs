
namespace RobotControl.ClassLibrary
{
    using Newtonsoft.Json;

    using System;

    class RobotData
    {
        public float accelX   { get; set; }
        public float accelY   { get; set; }
        public float accelZ   { get; set; }
        public float compass  { get; set; }
        public float distance { get; set; }
        public float voltage  { get; set; }
        public float uv       { get; set; }
        public string status  { get; set; }
    }

    class State : RobotControlBase, IState
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

        public State(IMediator mediator) : base(mediator) { }

        public RobotState RobotState
        {
            get { lock(lockObject) { return robotState      ; }}
            set
            {
                string name = EnumUtilities.EnumValueToSpaceSeparatedString(value);

                Publish(new EventDescriptor
                {
                    Name = EventName.PleaseSay,
                    Detail = $"State now is: {name}"
                });

                lock (lockObject)
                {
                    robotState = value;
                }
            }
        }

        private static string StateNameToSpaceSeparatedString(RobotState value)
        {
            var name = Enum.GetName(typeof(RobotState), value);
            var len = name.Length;
            for (var i = len - 1; i > 0; i--)
            {
                if (char.IsUpper(name[i]))
                {
                    name = name.Substring(0, i) + " " + name.Substring(i);
                }
            }

            return name;
        }

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
            TryCatch(() =>
            {
                if (eventDescriptor.Name == EventName.RawRobotDataDetected)
                {
                    RobotData robotData;

                    System.Diagnostics.Debug.WriteLine($"-->DATA FROM ROBOT: {eventDescriptor.Detail}");
                    try
                    {
                        robotData = JsonConvert.DeserializeObject<RobotData>(eventDescriptor.Detail);
                    }
                    catch (Newtonsoft.Json.JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"-->EXCEPTION DESERIALIZING: {ex.Message}");
                        return;
                    }

                    var state = (IState)new State(mediator)
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
                    Publish(ev);
                }
            });
        }
    }
}
