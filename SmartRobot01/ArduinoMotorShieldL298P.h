/*
class MotorPins
{
public:
	int DirectionPin;
	int SpeedPin;
	int BrakePin;
	int CurrentPin;
  
  MotorPins(int directionPin, int speedPin, int brakePin, int currentPin)
  {
	  DirectionPin = directionPin;
	  SpeedPin     = speedPin    ;
	  BrakePin     = brakePin    ;
	  CurrentPin   = currentPin  ;
  }
}
*/
class ArduinoMotorShieldL298P
{
private:
  const int DirectionA = 12,
            DirectionB = 13,
            SpeedA     =  3,
            SpeedB     = 11,
            BrakeA     =  9,
            BrakeB     =  8,
            CurrentA   =  0,
            CurrentB   =  1;

  void setSpeedInternal(int speed, char motorId)
  {
    int brk = (motorId == 'A') ? BreakA : BreakB;

	if (speed == 0)
	{
	  digitalWrite(brk, HIGH);
	}
	else
	{
	  int dir = (motorId == 'A') ? DirectionA : DirectionB;
	  int spd = (motorId == 'A') ? SpeedA     : SpeedB    ;	
      digitalWrite(dir, (speed > 0) ? HIGH : LOW); // HIGH is forward
      digitalWrite(brk, LOW);   //Disengage the Brake
      analogWrite(spd, abs(speed));   //Spins the motor
	}
  }
  
  
public:
  ArduinoMotorShieldL298P()
  {
    //Setup Channel A
    pinMode(DirectionA, OUTPUT); //Initiates Motor Channel A pin
    pinMode(BrakeA, OUTPUT); //Initiates Brake Channel A pin
  
    //Setup Channel B
    pinMode(DirectionB, OUTPUT); //Initiates Motor Channel B pin
    pinMode(BrakeB, OUTPUT);  //Initiates Brake Channel B pin
  }

  void SetSpeed(int speedA, int speedB)
  {
	setSpeedInternal(speedA, 'A');
	setSpeedInternal(speedB, 'B');
  }
};

