#include <MeMegaPi.h>

MeMegaPiDCMotor dc;
MeEncoderOnBoard Encoder_1(SLOT1);
MeEncoderOnBoard Encoder_2(SLOT2);
MeEncoderOnBoard Encoder_3(SLOT3);

int data;
const int firstSafeAsciiCharacter = 33;
const int speedMultiplier = 3;

void setup() 
{
  Serial.begin(115200);
  dc.reset(PORT4B);
}

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

int constrainToMaxAndMinSpeed(int speed)
{
  const int maximumSpeed = 255;
  const int minimumSpeed = 0;
  
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

// m - don't use movement; M - move; A - arm(up/down movement); a - don't use arm; C - claw; c - don't use claw; N - Negative
void loop() 
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
