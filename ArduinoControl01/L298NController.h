#define L298N_MOTOR_A 0
#define L298N_MOTOR_B 1

class L298NController {
public:

  L298NController(int in1pin, int enApin, int in2pin, int csApin, int in3pin, int enBpin, int in4pin, int csBpin) {
    _in1pin = in1pin;
    _enApin = enApin;
    _in2pin = in2pin;
    _csApin = csApin;
    _in3pin = in3pin;
    _enBpin = enBpin;
    _in4pin = in4pin;
    _csBpin = csBpin;
  }
  
  L298NController(int in1pin, int enApin, int in2pin, int csApin) {
    initMinusOne();
    _in1pin = in1pin;
    _enApin = enApin;
    _in2pin = in2pin;
    _csApin = csApin;
  }

  L298NController(int in1pin, int enApin) {
    initMinusOne();
    _enApin = enApin;
    _in1pin = in1pin;
  }

  void Start(int motor) { Start(motor, 255); }

  void Start(int motor, int power) { Start(motor, power, 0); }

  void Start(int motor, int power, int durationInMilliseconds) { Start(motor, power, durationInMilliseconds, true); }

  void Start(int motor, int power, int durationInMilliseconds, bool keepRunning) {
    //serialSSI("L298NController.Start-------------------------------", "motor", motor);
    //serialSSI("L298NController.Start", "power", power);
    //serialSSI("L298NController.Start", "durationInMilliseconds", durationInMilliseconds);
    //serialSSI("L298NController.Start", "keepRunning", keepRunning ? 1 : 0);
    
    digitalWriteInternal(getIN(motor, durationInMilliseconds < 0), HIGH);
    
    analogWriteInternal (getEN(motor), power);
    delay(abs(durationInMilliseconds));
    if (!keepRunning) {
      Stop(motor);
    }
  }

  void Stop(int motor) {
    //serialSSI("L298NController.Stop", "motor", motor);
    digitalWriteInternal(getIN(motor, true), LOW);
    digitalWriteInternal(getIN(motor, false), LOW);
    analogWriteInternal (getEN(motor), 0);    
  }

private:
  void digitalWriteInternal(int pin, int value) {
    //serialSSI("L298NController.digitalWriteInternal", "pin", pin);
    //serialSSI("L298NController.digitalWriteInternal", "value", value);
    if (pin >= 0) {
      digitalWrite(pin, value);
    }
  }

  void analogWriteInternal(int pin, int value) {
    if (pin >= 0) {
      analogWrite(pin, value);
    }
  }

  void initMinusOne() { _in1pin = _enApin = _in2pin = _csApin = _in3pin = _enBpin = _in4pin = _csBpin = -1; };
  
  int _in1pin, _enApin, _in2pin, _csApin, _in3pin, _enBpin, _in4pin, _csBpin;
  int getEN(int motor) { return motor == L298N_MOTOR_A || _enBpin < 0 ? _enApin : _enBpin; }
  int getIN(int motor, bool reverse) { 
    return (motor == L298N_MOTOR_A) ? _in1pin : 
      (reverse && _in2pin >= 0 ? _in2pin : _in1pin);
  }

};
