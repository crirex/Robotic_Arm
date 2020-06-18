#include <TimedAction.h>

#include <MeMegaPi.h>
#include <Wire.h>

MeMegaPiDCMotor dc;
MeEncoderOnBoard Encoder_1(SLOT1);
MeEncoderOnBoard Encoder_2(SLOT2);
MeEncoderOnBoard Encoder_3(SLOT3);
MeGyro gyro(PORT_7);
MeUltrasonicSensor us(PORT_8);

int data;
const char syncChar = (char)255;
const int firstSafeAsciiCharacter = 33;
const int speedMultiplier = 3;

typedef union {
 float floatPoint;
 byte binary[4];
} binaryFloat;

int getSignByCharacter(int character)
{
  if(character == 'N')
  {
    return -1;
  }
  return 1;
}

int awaitRead()
{
  while(Serial.available() == 0) { }
  return Serial.read(); 
}

void writeData(binaryFloat value)
{
  Serial.write(value.binary[0]);
  Serial.write(value.binary[1]);
  Serial.write(value.binary[2]);
  Serial.write(value.binary[3]);
}

void writeData(float value)
{
  binaryFloat flValue;
  flValue.floatPoint = value;
  writeData(flValue);
}

int constrainToMaxAndMinSpeed(int speed)
{
  const int maximumSpeed = 255;
  const int minimumSpeed = 0;

  if(speed == 0)
  {
    return speed;
  }
  
  if (speed > maximumSpeed)
  {
    return maximumSpeed;
  }

  if (speed < minimumSpeed)
  {
    return minimumSpeed;
  }
  
  return speed;
}

int getMotorSpeed()
{
  int motorSpeedSign = awaitRead(); // sign
  int motorSpeed = awaitRead(); //speed
  motorSpeed = constrainToMaxAndMinSpeed((motorSpeed - firstSafeAsciiCharacter) * speedMultiplier);
  return motorSpeed * getSignByCharacter(motorSpeedSign);
}

void moveAction()
{
  Encoder_1.setMotorPwm(getMotorSpeed());// R
  Encoder_2.setMotorPwm(getMotorSpeed());// L
}

void clawAction()
{
  dc.run(getMotorSpeed());
}

void armAction()
{
  Encoder_3.setMotorPwm(getMotorSpeed());// Arm
}

void sendGyroData()
{
  //writeData((float)gyro.getAngleX());
  //writeData((float)gyro.getAngleY());
  writeData((float)gyro.getAngleZ());
  //writeData((float)gyro.getGyroX());
  //writeData((float)gyro.getGyroY());
}

void sendUltrasonicSensorData() 
{
  float distance = 0;
  long timeInterval = 0;
  timeInterval = us.measure();
  if(timeInterval == 0)
  {
    writeData(500.0f);
  }
  //distance = us.distanceCm();
  distance = (float)timeInterval / 58.0f;
  if(distance > 500.0f)
  {
    distance = 500.0f;
  }
  writeData(distance);
}

void sendDataPart()
{
  Serial.write(syncChar);
  sendGyroData();
  sendUltrasonicSensorData();
}

void readMovementInfo()
{
  data = awaitRead();
  if(data == 'M')
  {
    moveAction();
  }
  else if (data == 'C')
  {
    clawAction();
  }
  else if (data == 'A')
  {
    armAction();
  }
}

void setup() 
{
  Serial.begin(115200);
  dc.reset(PORT4B);
  gyro.begin();
}

TimedAction sendThread = TimedAction(20,sendDataPart);
//TimedAction readThread = TimedAction(300,readMovementInfo);

// m - don't use movement; M - move; A - arm(up/down movement); a - don't use arm; C - claw; c - don't use claw; N - Negative
void loop() 
{
  gyro.update();
  readMovementInfo();
  sendThread.check();
}
