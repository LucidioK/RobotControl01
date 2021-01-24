
#include <SPI.h>
#include <ArduinoJson.h>
#include "UltraSonicDistanceDetector.h"
#include <Adafruit_SI1145.h>
#include <Adafruit_LSM303_Accel.h>
#include <Adafruit_LSM303DLH_Mag.h>
#include <Adafruit_Sensor.h>
#include "L298NX2.h"
#include "VoltageReader.h"

VoltageReader              voltageReader    (A0, 47000, 33000);
UltraSonicDistanceDetector distanceDetector (/*TRIGPIN*/3, /*ECHOPIN*/2);

Adafruit_SI1145                uv      = Adafruit_SI1145();
Adafruit_LSM303_Accel_Unified  accel   = Adafruit_LSM303_Accel_Unified(54321);
Adafruit_LSM303DLH_Mag_Unified mag     = Adafruit_LSM303DLH_Mag_Unified(12345);
bool                           accelOK = false;
bool                           magOK   = false;
bool                           uvOK    = false;

const int EN_A = 4, IN1_A = 5, IN2_A = 7, IN1_B = 8, IN2_B = 12, EN_B = 13;
L298NX2 motors(EN_A, IN1_A, IN2_A, EN_B, IN1_B, IN2_B);

String outStatus;

void controlMotors(int l, int r)
{
  bool rReverse = (r < 0), lReverse = (l < 0);
  r = min(abs(r),220);
  l = min(abs(l),220);
  if (r < 40 && l < 40)
  {
    Serial.print("StoMot.");
    motors.stop();
  }
  else
  {
    motors.setSpeedB(l);
    if (lReverse) { motors.backwardB(); } else { motors.forwardB(); }
    motors.setSpeedA(r);
    if (rReverse) { motors.backwardA(); } else { motors.forwardA(); }
  }  
}

void readAndDispatchCommands()
{
  if (Serial.available() > 0)
  {
    String s= Serial.readStringUntil('\n');
    outStatus += s;
    outStatus += "|";
    StaticJsonDocument<200> doc;
    DeserializationError error = deserializeJson(doc, s);
    if (error)
    {
      outStatus += error.c_str();
      outStatus += "|";      
      return;
    }
    if (doc["operation"] == "motor")
    {
      controlMotors(doc["l"], doc["r"]);
    }
  }
}

float getCompassHeading()
{
  if (!magOK)
  {
    outStatus += "magNOK|";
    return -1.0;
  }
  sensors_event_t event;
  mag.getEvent(&event);
 
  float heading = (atan2(event.magnetic.y,event.magnetic.x) * 180) / 3.14159;
 
  if (heading < 0)
  {
    heading = 360 + heading;
  }

  return heading;
}

void sendSensorValues()
{
  StaticJsonDocument<200> doc;
  if (accelOK)
  {
    sensors_event_t event;
    accel.getEvent(&event);
    doc["accelX"]   = event.acceleration.x;
    doc["accelY"]   = event.acceleration.y;
    doc["accelZ"]   = event.acceleration.z;
  }
  else
  {
    outStatus += "accelNOK|";
  }

  if (magOK)
    doc["compass"] = getCompassHeading();
  else
    outStatus += "magNOK|";  

  doc["distance"] = distanceDetector.Get();
  doc["voltage"]  = voltageReader.Get();

  if (!uvOK)  outStatus += "uvNOK|";

  uint16_t r1   = uv.readUV();
  delay(10);
  uint16_t r2   = uv.readUV();
  delay(10);
  uint16_t r3   = uv.readUV();
    
  doc["uv"]  = (r1+r2+r3) / 3;
  if (outStatus != "")
  {
    doc["status"] = outStatus;
  }
  
  String s;
  serializeJson(doc, s);
  Serial.println(s);
}

void setup() {
  Serial.begin(115200);
  mag.enableAutoRange(true);
  magOK   = mag.begin();
  accelOK = accel.begin();
  uvOK = uv.begin();

  if (accelOK)
  {
    accel.setRange(LSM303_RANGE_4G);
  }

  Serial.println("Device is ready 20209823 1243");  
}

void loop() {
  
  outStatus = "";
  
  readAndDispatchCommands();

  sendSensorValues();

  delay(100); 
}
