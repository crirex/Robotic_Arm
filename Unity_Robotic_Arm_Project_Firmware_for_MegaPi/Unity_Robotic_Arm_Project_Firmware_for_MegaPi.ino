#include <MeMegaPi.h>

MeEncoderOnBoard Encoder_1(SLOT1);
MeEncoderOnBoard Encoder_2(SLOT2);

int data;
int inputSpeed;
int slowestSpeed = 113;

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

void StayStill()
{
  Encoder_1.setMotorPwm(0);
  Encoder_2.setMotorPwm(0);
}

void setup() {
  Serial.begin(9600);
}

// S - Stay, F - forward; B - backwards; L - left; R - right; U - up; D - down; C - close claw; O - open claw
void loop() {
if(Serial.available())
{
    //You need to put in an Enum with speed values. It shouldn't be the exact value you get as input
    data = Serial.read();
    while(Serial.available() == 0) { }
    if(data == 'F'){
      inputSpeed = Serial.read();
      float curentSpeed = inputSpeed + slowestSpeed;
      Forward(curentSpeed);
    }
    else if(data == 'B'){
      inputSpeed = Serial.read();
      float curentSpeed = inputSpeed + slowestSpeed;
      Backward(curentSpeed);
    }
    else if(data == 'L'){
      inputSpeed = Serial.read();
      float curentSpeed = inputSpeed + slowestSpeed;
      TurnLeft(curentSpeed);
    }
    else if(data == 'R'){
      inputSpeed = Serial.read();
      float curentSpeed = inputSpeed + slowestSpeed;
      TurnRight(curentSpeed);
    }
    else if(data == 'S'){
      StayStill();
    }
  }
}
