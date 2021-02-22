using System;

namespace RobotControl.ClassLibrary
{
    // Enum order reflects state priority
    public enum RobotState
    {
        AttendingVoiceCommand,
        EvadingToShelter,
        ChargingBattery,
        AvoidingObstacle,
        CompassCalibration,
        MotorsCalibration,
        ChasingObject,
        Scanning,
        Idle,
    }

    public enum EventName
    {
        VoiceCommandDetected,
        RainDetected,
        ObjectDetected,
        SensorValueDetected,
        NeedToMoveDetected,
        NewImageDetected,
        RawRobotDataDetected,
        RobotData,
        PleaseSay,
        Exception,
        MotorCalibrationRequest,
        MotorCalibrationResponse,
    }

    public static class EnumUtilities
    {
        public static string EnumValueToSpaceSeparatedString<T>(T value) where T : Enum
        {
            var spaceSeparatedString = Enum.GetName(typeof(T), value);
            var originalLength = spaceSeparatedString.Length;
            for (var i = originalLength - 1; i > 0; i--)
            {
                if (char.IsUpper(spaceSeparatedString[i]))
                {
                    spaceSeparatedString = spaceSeparatedString.Substring(0, i) + " " + spaceSeparatedString.Substring(i);
                }
            }

            return spaceSeparatedString;
        }
    }
}
