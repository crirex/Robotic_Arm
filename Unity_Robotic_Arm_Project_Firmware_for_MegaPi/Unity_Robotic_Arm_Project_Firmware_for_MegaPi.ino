#include <MeMegaPi.h>

MeMegaPiDCMotor dc;
MeEncoderOnBoard Encoder_1(SLOT1);
MeEncoderOnBoard Encoder_2(SLOT2);
MeEncoderOnBoard Encoder_3(SLOT3);

int data;

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
  const int maximumSpeed = 186;
  const int minimumSpeed = 100;
  
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
  int motorSpeed = awaitRead(); // part 1
  int motorSpeedPart2 = awaitRead(); // part 2
  motorSpeed = constrainToMaxAndMinSpeed(motorSpeed + motorSpeedPart2 + 50);
  return motorSpeed * getSignByCharacter(motorSpeedSign);
}

void notMoveAction() 
{
  Encoder_1.setMotorPwm(0);
  Encoder_2.setMotorPwm(0);
}

void notArmAction() 
{
  Encoder_3.setMotorPwm(0);
}

void notClawAction() 
{
  dc.run(0);
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
  else if (data == 'm')
  {
    notMoveAction();
  }
  else if (data == 'C')
  {
    clawAction();
  }
  else if (data == 'c')
  {
    notClawAction();
  }
  else if (data == 'A')
  {
    armAction();
  }
  else if (data == 'a')
  {
    notArmAction();
  }
}
