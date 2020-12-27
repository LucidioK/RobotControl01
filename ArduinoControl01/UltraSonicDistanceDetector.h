
class UltraSonicDistanceDetector {
public:
	UltraSonicDistanceDetector(int trigPin, int echoPin) {
	  _trigPin = trigPin;
	  _echoPin = echoPin;
	  pinMode(trigPin, OUTPUT);
	  pinMode(echoPin, INPUT);	
	}
	
	float Get() {
	  digitalWrite(_trigPin, LOW);
	  delayMicroseconds(5);
	  // Trigger the sensor by setting the trigPin high for 10 microseconds:
	  digitalWrite(_trigPin, HIGH);
	  delayMicroseconds(10);
	  digitalWrite(_trigPin, LOW);
	  // Read the echoPin, pulseIn() returns the duration (length of the pulse) in microseconds:
	  long duration = pulseIn(_echoPin, HIGH);

	  float distance = duration / 57.0;
	  return distance;
	}

private:
	int _trigPin, _echoPin;
};
