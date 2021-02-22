/*
 * 
 * {'operation':'stop'}
 * {'operation':'motorcalibration'}
 * {'operation':'motor','l':-200,'r':200}
 * {'operation':'timedmotor','l':-200,'r':200,'t':500}
 */
#include <SPI.h>
#include <ArduinoJson.h>
// #include <Adafruit_SI1145.h>
#include <Adafruit_LSM303_Accel.h>
#include <Adafruit_LSM303DLH_Mag.h>
#include <Adafruit_Sensor.h>
#include "ArduinoMotorShieldL298P.h"
#include "VoltageReader.h"
#include <VL53L0X.h>

enum RobotStateEnum
{
  NONE,
  MOVING,
  CALIBRATING,
  STOPPED
};


RobotStateEnum             robotState = NONE;
VoltageReader              voltageReader    (A0, 47000, 33000);

VL53L0X                        distance;
// Adafruit_SI1145                uv      = Adafruit_SI1145();
Adafruit_LSM303_Accel_Unified  accel   = Adafruit_LSM303_Accel_Unified(54321);
Adafruit_LSM303DLH_Mag_Unified mag     = Adafruit_LSM303DLH_Mag_Unified(12345);
int calibrationDataCounter             = 0;
float accummulatedAccelX               = 0.0;
float accummulatedAccelY               = 0.0;
float accummulatedAccelZ               = 0.0;
bool                           accelOK = false;
bool                           magOK   = false;
// bool                           uvOK    = false;

ArduinoMotorShieldL298P        motors  = ArduinoMotorShieldL298P();

String outStatus;

void controlMotors(int l, int r)
{
  motors.setSpeed(l, r);
}

void stop()
{
  controlMotors(0, 0);
  robotState = STOPPED;
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
      robotState = MOVING;
      controlMotors(doc["l"], doc["r"]);
    }

    if (doc["operation"] == "timedmotor")
    {
      robotState = MOVING;
      controlMotors(doc["l"], doc["r"]);
      delay(doc["t"]);
      stop();
    }

    if (doc["operation"] == "motorcalibration")
    {
      calibrationDataCounter = 0;
      accummulatedAccelX     = 0.0;
      accummulatedAccelY     = 0.0;
      accummulatedAccelZ     = 0.0;      
      robotState             = CALIBRATING;

      Serial.println("Starting calibration -200, 200.");
      controlMotors(-200, 200);
    }

    if (doc["operation"] == "stop")
    {
      stop();
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
  doc["dataType"]        = "sensorvalues";
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

  doc["distance"] = distance.readRangeContinuousMillimeters() / 10;
  doc["voltage"]  = voltageReader.Get();

//  if (!uvOK)  outStatus += "uvNOK;";
//
//  uint16_t r1   = uv.readUV();
//  delay(10);
//  uint16_t r2   = uv.readUV();
//  delay(10);
//  uint16_t r3   = uv.readUV();
//    
//  doc["uv"]  = (r1+r2+r3) / 3;
  if (outStatus != "" && outStatus != ";")
  {
    doc["status"] = outStatus;
  }
  
  String s;
  serializeJson(doc, s);
  Serial.println(s);
}

void initializeDistance()
{
  Wire.begin();

  distance.init();
  distance.setTimeout(500);

  // Start continuous back-to-back mode (take readings as
  // fast as possible).  To use continuous timed mode
  // instead, provide a desired inter-measurement period in
  // ms (e.g. distance.startContinuous(100)).
  distance.startContinuous();
}

//void initializeUV()
//{
//  uvOK = uv.begin();
//}

void initializeMag()
{
  mag.enableAutoRange(true);
  magOK   = mag.begin();
}

void initializeAccel()
{
  accelOK = accel.begin();

  if (accelOK)
  {
    accel.setRange(LSM303_RANGE_4G);
  }
}


void runCalibrationDataCollection()
{
  calibrationDataCounter++;

  if (distance.readRangeContinuousMillimeters() < 200 || calibrationDataCounter > 100)
  {
    stop();
    StaticJsonDocument<200> doc;
    doc["dataType"]        = "motorcalibrationresponse";    
    doc["averageAccelX"]   = accummulatedAccelX / calibrationDataCounter;    
    doc["averageAccelY"]   = accummulatedAccelY / calibrationDataCounter;    
    doc["averageAccelZ"]   = accummulatedAccelZ / calibrationDataCounter;    
    doc["count"]           = calibrationDataCounter;    
    String s;
    serializeJson(doc, s);
    Serial.println(s);    
    delay(5000);
  }

  sensors_event_t event;
  accel.getEvent(&event);
  accummulatedAccelX += event.acceleration.x;
  accummulatedAccelY += event.acceleration.y;
  accummulatedAccelZ += event.acceleration.z;
}

void setup() 
{
  Serial.begin(115200);
  initializeDistance();
//  initializeUV();
  initializeMag();
  initializeAccel();

  stop();
  Serial.println("Device is ready 20210221 1900");  
}

void loop() 
{
  outStatus = "";
  switch (robotState)
  {
    case NONE:
    case MOVING:
    {
      readAndDispatchCommands();
    
      sendSensorValues();
    
      delay(100); 

      break;
    }  

    case STOPPED:
      Serial.print(".");
      readAndDispatchCommands();
      delay(1000);
      break;

    case CALIBRATING:
      runCalibrationDataCollection();
      delay(10);
      break;
  }

}
