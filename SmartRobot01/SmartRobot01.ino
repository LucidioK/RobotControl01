
#include <SPI.h>
#include <ArduinoJson.h>
#include "UltraSonicDistanceDetector.h"
#include <Adafruit_SI1145.h>
#include <Adafruit_LSM303_Accel.h>
#include <Adafruit_LSM303DLH_Mag.h>
#include <Adafruit_Sensor.h>
#include "ArduinoMotorShieldL298P.h"
#include "VoltageReader.h"

VoltageReader              voltageReader    (A0, 47000, 33000);

Adafruit_SI1145                uv      = Adafruit_SI1145();
Adafruit_LSM303_Accel_Unified  accel   = Adafruit_LSM303_Accel_Unified(54321);
Adafruit_LSM303DLH_Mag_Unified mag     = Adafruit_LSM303DLH_Mag_Unified(12345);
bool                           accelOK = false;
bool                           magOK   = false;
bool                           uvOK    = false;

ArduinoMotorShieldL298P        motors  = ArduinoMotorShieldL298P();

String outStatus;

void controlMotors(int l, int r)
{
  motors.setSpeed(l, r);
}

void readAndDispatchCommands()
{
  if (Serial.available() > 0)
  {
    String s= Serial.readStringUntil('\n');
    outStatus += s;
    outStatus += ";";
    StaticJsonDocument<200> doc;
    DeserializationError error = deserializeJson(doc, s);
    if (error)
    {
      outStatus += error.c_str();
      outStatus += ";";      
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
    outStatus += "magNOK;";
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
    outStatus += "accelNOK;";
  }

  if (magOK)
    doc["compass"] = getCompassHeading();
  else
    outStatus += "magNOK;";  

  //doc["distance"] = distanceDetector.Get();
  doc["voltage"]  = voltageReader.Get();

  if (!uvOK)  outStatus += "uvNOK;";

  uint16_t r1   = uv.readUV();
  delay(10);
  uint16_t r2   = uv.readUV();
  delay(10);
  uint16_t r3   = uv.readUV();
    
  doc["uv"]  = (r1+r2+r3) / 3;
  if (outStatus != "" && outStatus != ";")
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
