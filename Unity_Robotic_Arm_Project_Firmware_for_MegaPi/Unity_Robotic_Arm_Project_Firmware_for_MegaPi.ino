#include <MeMegaPi.h>

MeEncoderOnBoard Encoder_1(SLOT1);
MeEncoderOnBoard Encoder_2(SLOT2);

float slowestSpeed = 110;
float fastestSpeed = 255;

void Forward(float speed)
{
  Encoder_1.setMotorPwm(speed);
  Encoder_2.setMotorPwm(-speed);
}

void Backward(float speed)
{
  Encoder_1.setMotorPwm(-speed);
  Encoder_2.setMotorPwm(speed);
}

void TurnLeft(float speed)
{
  Encoder_1.setMotorPwm(speed);
  Encoder_2.setMotorPwm(speed);
}

void TurnRight(float speed)
{
  Encoder_1.setMotorPwm(-speed);
  Encoder_2.setMotorPwm(-speed);
}

void setup() {
  // put your setup code here, to run once:

}

void loop() {
  
}
