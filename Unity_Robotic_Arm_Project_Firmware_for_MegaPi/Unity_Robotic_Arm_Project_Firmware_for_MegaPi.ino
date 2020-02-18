#include <MeMegaPi.h>
#include <Wire.h>

MeMegaPiDCMotor dc;
MeEncoderOnBoard Encoder_1(SLOT1);
MeEncoderOnBoard Encoder_2(SLOT2);
MeEncoderOnBoard Encoder_3(SLOT3);
//MeGyro gyro(PORT_7);
//MeUltrasonicSensor us(PORT_8);

int data;
const int firstSafeAsciiCharacter = 33;
const int speedMultiplier = 3;

//typedef union {
// float floatPoint;
// byte binary[4];
//} binaryFloat;

//typedef union {
//  double doublePoint;
//  byte binary[8];
//} binaryDouble;

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

//void writeData(binaryDouble value)
//{
//  Serial.write('D');
//  Serial.write(value.binary[7]);
//  Serial.write(value.binary[6]);
//  Serial.write(value.binary[5]);
//  Serial.write(value.binary[4]);
//  Serial.write(value.binary[3]);
//  Serial.write(value.binary[2]);
//  Serial.write(value.binary[1]);
//  Serial.write(value.binary[0]);
//}

//void writeData(double value)
//{
//  binaryDouble biValue;
//  biValue.doublePoint = value;
//  writeData(biValue);
//}

//void writeData(binaryFloat value)
//{
//  Serial.write(value.binary[3]);
//  Serial.write(value.binary[2]);
//  Serial.write(value.binary[1]);
//  Serial.write(value.binary[0]);
//}

//void writeData(float value)
//{
//  binaryFloat flValue;
//  flValue.floatPoint = value;
//  writeData(flValue);
//}

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

//void sendGyroData()
//{
//  gyro.update();
//  writeData(gyro.getAngleX());
//  writeData(gyro.getAngleY());
//  writeData(gyro.getAngleZ());
//  writeData(gyro.getGyroX());
//  writeData(gyro.getGyroY());
//}

//void sendUltrasonicSensorData() 
//{
//  //writeData(us.distanceCm());
//  //writeData((double)us.measure() / 58.0);
//  writeData((double)54.28);
//}

void setup() 
{
  Serial.begin(115200);
  dc.reset(PORT4B);
  //gyro.begin();
}

// m - don't use movement; M - move; A - arm(up/down movement); a - don't use arm; C - claw; c - don't use claw; N - Negative
void loop() 
{
  //sendGyroData();
  //Serial.write('A');
  //if(Serial.available() == 0)
  //{
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
    //sendUltrasonicSensorData();
  //}
}
