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

// F - forward; B - backwards; L - left; R - right; U - up; D - down; C - close claw; O - open claw
void loop() {
if(Serial.available())
{
    delay(1000);
    //You need to put in an Enum with speed values. It shouldn't be the exact value you get as input
    data = Serial.read();
    while(Serial.available() == 0) { }
    inputSpeed = Serial.read();
    if(data == 'F'){
      float curentSpeed = inputSpeed + slowestSpeed;
      Forward(curentSpeed);
      Serial.write((int)curentSpeed);
      Serial.flush();
    }
    else if(data == 'B'){
      Backward(inputSpeed + slowestSpeed);
    }
    else if(data == 'L'){
      TurnLeft(inputSpeed + slowestSpeed);
    }
    else if(data == 'R'){
      TurnRight(inputSpeed + slowestSpeed);
    }
  }
else{
  StayStill();
}
}
