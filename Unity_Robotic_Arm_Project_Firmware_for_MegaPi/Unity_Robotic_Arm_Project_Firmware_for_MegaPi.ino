#include <MeMegaPi.h>

MeEncoderOnBoard Encoder_1(SLOT1);
MeEncoderOnBoard Encoder_2(SLOT2);

int data;

void setup() {
  Serial.begin(9600);
}

void stay() {
  Encoder_1.setMotorPwm(0);
  Encoder_2.setMotorPwm(0);
}

// S - stay; M - move; U - up; D - down; C - close claw; O - open claw; P - positive; N - Negative
void loop() {
if(Serial.available())
{
  while(Serial.available() == 0) { }
  data = Serial.read(); 
  if(data == 'M'){
    while(Serial.available() == 0) { }
    int motor1SpeedSign = Serial.read(); // R sign
    Serial.write(motor1SpeedSign);

    while(Serial.available() == 0) { }
    int motor1Speed = Serial.read(); // R part 1
    Serial.write(motor1Speed);

    while(Serial.available() == 0) { }
    int motor1SpeedPart2 = Serial.read(); // R part 2
    Serial.write(motor1SpeedPart2);

    while(Serial.available() == 0) { }
    int motor2SpeedSign = Serial.read(); // R sign
    Serial.write(motor2SpeedSign);

    while(Serial.available() == 0) { }
    int motor2Speed = Serial.read(); // L part 1
    Serial.write(motor2Speed);

    while(Serial.available() == 0) { }
    int motor2SpeedPart2 = Serial.read(); // L part 2
    Serial.write(motor2SpeedPart2);
    Serial.flush();
    
    motor1Speed = motor1Speed + motor1SpeedPart2;
    motor2Speed = motor2Speed + motor2SpeedPart2;
    
    if(motor1SpeedSign == 'N')
    {
      motor1Speed = motor1Speed * -1;
    }
    
    if(motor2SpeedSign == 'N')
    {
      motor2Speed = motor2Speed * -1;
    }
     
    Encoder_1.setMotorPwm(motor1Speed);
    Encoder_2.setMotorPwm(motor2Speed);
    } else if (data == 'S')
    {
      stay();
    }
  }
}
