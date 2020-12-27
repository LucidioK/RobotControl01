namespace RobotControl.Net
{
    // Enum order reflects state priority
    public enum RobotState
    {
        AttendingVoiceCommand,
        EvadingToShelter,
        ChargingBattery,
        AvoidingObstacle,
        ChasingObject,
        Scanning,
        Idle,
    }

    public enum EventName
    {
        VoiceCommandDetected,
        RainDetected,
        ObstacleDetected,
        LowBatteryDetected,
        ObjectDetected,
        SensorValueDetected,
        NeedToMoveDetected,
        NewImageDetected,
        RobotDataDetected,
        PleaseSay,
    }
}
