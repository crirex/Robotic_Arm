using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class RobotTank : MonoBehaviour
{
    public SerialPort serial = new SerialPort("COM6", 115200);

    [SerializeField]
    private GameObject leftClaw;
    [SerializeField]
    private GameObject rightClaw;
    [SerializeField]
    private GameObject upperBody;
    [SerializeField]
    private GameObject movementBody;
    [SerializeField]
    private GameObject rotationBody;
    [SerializeField]
    private GameObject motorObject;

    public static bool hasObject = false;

    float buttonPressedForce;
    float buttonPressedForce2;
    float leftThumbstickUpDown;
    float leftThumbstickLeftRight;
    float rightThumbstickUpDown;
    float rightThumbstickLeftRight;
    Vector3 leftControllerPosition;
    Vector3 rightControllerPosition;
    Vector3 leftControllerRotation;
    Vector3 rightControllerRotation;

    private const float maximumOutwardsClawRotation = 0f; //Rotations tested for right claw, left claw has oposite rotations
    private const float maximumInwardsClawRotation = -28.2f;
    private const float OutwardsXPosition = 0.0654f;
    private const float InwardsRightClawXPosition = 0.0716f;
    private const float InwardsLeftClawXPosition = OutwardsXPosition - (InwardsRightClawXPosition - OutwardsXPosition);
    private const float OutwardsZPosition = 0.2178f;
    private const float InwardsRightClawZPosition = 0.2062f;
    private const float InwardsLeftClawZPosition = OutwardsZPosition - (OutwardsZPosition - InwardsRightClawZPosition);
    private const float maximumMotorObjectZPositionOffset = -0.01f;
    private float firstMotorObjectZPosition;
    private const float maximumBackwardsUpperBodyRotation = -110f;
    private const float maximumForwardsUpperBodyRotation = 0f;

    private const int maximumSpeed = 186;
    private const int minimumSpeed = 100;

    private const float rotationSpeedCorrector = 0.32f * (1f / ((float)maximumSpeed - (float)minimumSpeed));
    private const float forwardSpeedCorrector = 0.001f * (1f / ((float)maximumSpeed - (float)minimumSpeed));
    private const float armSpeedCorrector = 0.13f * (1f / ((float)maximumSpeed - (float)minimumSpeed));
    
    private const float maximumRadius = 1f;

    int forwardSpeedValue = 0;
    int rotationSpeedValue = 0;
    int armSpeedValue = 0;
    int clawSpeedValue = 0;

    private float lastUpperBodyRotationZ;
    // Start is called before the first frame update
    void Start()
    {
        serial.Open();
        serial.ReadTimeout = 1;

        firstMotorObjectZPosition = motorObject.transform.localPosition.z;
        lastUpperBodyRotationZ = 0;
        upperBody.transform.localRotation = Quaternion.Euler(upperBody.transform.localRotation.eulerAngles.x, upperBody.transform.localRotation.eulerAngles.y, -50);
    }

    // Update is called once per frame
    void Update()
    {

        buttonPressedForce = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
        buttonPressedForce2 = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
        leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        leftControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles;
        rightControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch).eulerAngles;
        leftThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        leftThumbstickLeftRight = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        rightThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
        rightThumbstickLeftRight = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        Vector2 rotationAndForwardValue = CalculateRotaionAndForwardSpeed();

        rotationSpeedValue = (int)rotationAndForwardValue.x;
        forwardSpeedValue = (int)rotationAndForwardValue.y;
        armSpeedValue = NormalizeValue((int)(-rightThumbstickUpDown * maximumSpeed));
        clawSpeedValue = NormalizeValue((int)((buttonPressedForce - buttonPressedForce2) * maximumSpeed));

        if (forwardSpeedValue != 0 && rotationSpeedValue != 0)
        {
            if (IsNotTurned())
            {
                MoveLeftRight();
                MoveForwardBackwards();
            }
            serial.Write(GetMovementSendString());
        }
        else
        {
            serial.Write("m");
        }

        if (Math.Abs(rightThumbstickUpDown) > 0.2)
        {
            MoveUpperBody();
            serial.Write(GetUpperBodySendString());
        }
        else
        {
            serial.Write("a");
        }

        if (clawSpeedValue != 0 && (Math.Abs(buttonPressedForce) > 0.1 || Math.Abs(buttonPressedForce2) > 0.1))
        {
            CloseClawAfterButtonForce();
            serial.Write(GetClawSendString());
        }
        else
        {
            serial.Write("c");
        }
    }

    bool IsNotTurned()
    {
        var tankRotations = transform.rotation.eulerAngles;
        if (tankRotations.x > 100 || tankRotations.x < 80)
        {
            return false;
        }
        return true;
    }

    void MoveLeftRight()
    {
        var wholeBodyAngles = rotationBody.transform.localRotation.eulerAngles;
        float rotateToValue = wholeBodyAngles.z - rotationSpeedCorrector * rotationSpeedValue;
        Debug.Log(rotationSpeedCorrector * rotationSpeedValue);
        rotationBody.transform.localRotation = Quaternion.Euler(wholeBodyAngles.x, wholeBodyAngles.y, rotateToValue);
    }

    void MoveForwardBackwards()
    {
        var wholeBodyPosition = movementBody.transform.localPosition;
        float moveToValue = wholeBodyPosition.y + forwardSpeedCorrector * forwardSpeedValue;
        Debug.Log(forwardSpeedCorrector * forwardSpeedValue);
        movementBody.transform.localPosition = new Vector3(wholeBodyPosition.x, moveToValue, wholeBodyPosition.z);
        this.transform.position = movementBody.transform.position;
        movementBody.transform.localPosition = new Vector3(0, 0, 0);
    }

    void CloseClawAfterButtonForce()
    {
        if (!hasObject)
        {
            float resultingRotationValue = (maximumOutwardsClawRotation - maximumInwardsClawRotation) * (1 - buttonPressedForce) + maximumInwardsClawRotation;
            var rightAngles = rightClaw.transform.localRotation.eulerAngles;
            var leftAngles = rightClaw.transform.localRotation.eulerAngles;
            rightClaw.transform.localRotation = Quaternion.Euler(rightAngles.x, resultingRotationValue, rightAngles.z);
            leftClaw.transform.localRotation = Quaternion.Euler(leftAngles.x, -resultingRotationValue, leftAngles.z);

            rightClaw.transform.localPosition = new Vector3(
                (InwardsRightClawXPosition - OutwardsXPosition) * buttonPressedForce + OutwardsXPosition,
                rightClaw.transform.localPosition.y,
                (InwardsRightClawZPosition - OutwardsZPosition) * buttonPressedForce + OutwardsZPosition);

            leftClaw.transform.localPosition = new Vector3(
                (InwardsLeftClawXPosition - OutwardsXPosition) * buttonPressedForce + OutwardsXPosition,
                leftClaw.transform.localPosition.y,
                (InwardsLeftClawZPosition - OutwardsZPosition) * buttonPressedForce + OutwardsZPosition);

            motorObject.transform.localPosition = new Vector3(motorObject.transform.localPosition.x,
                motorObject.transform.localPosition.y,
                firstMotorObjectZPosition + maximumMotorObjectZPositionOffset * buttonPressedForce);
        }
    }

    void MoveUpperBody()
    {
        var upperBodyAngles = upperBody.transform.localRotation.eulerAngles;
        Debug.Log(GetValueInBordersForUpperBody());
        upperBody.transform.localRotation = Quaternion.Euler(upperBodyAngles.x, upperBodyAngles.y, GetValueInBordersForUpperBody());
    }

    float GetValueInBordersForUpperBody()
    {
        float rotateToValue = lastUpperBodyRotationZ + armSpeedCorrector * armSpeedValue;
        if (maximumBackwardsUpperBodyRotation < rotateToValue && rotateToValue < maximumForwardsUpperBodyRotation)
        {
            lastUpperBodyRotationZ = rotateToValue;
        }
        return lastUpperBodyRotationZ;
    }

    Vector2 CalculateRotaionAndForwardSpeed()
    {
        float xNomalized = 0;
        float yNomalized = 0;
        float arithmeticMedian = Math.Abs(leftThumbstickUpDown) + Math.Abs(leftThumbstickLeftRight);
        float speedMultiplyer = (float)Math.Sqrt(Math.Pow(Math.Sin(Math.PI * leftThumbstickUpDown), 2) + Math.Pow(Math.Cos(Math.PI * leftThumbstickLeftRight), 2)) / maximumRadius;

        if (arithmeticMedian != 0)
        {
            xNomalized = leftThumbstickLeftRight / arithmeticMedian;
            yNomalized = leftThumbstickUpDown / arithmeticMedian;
        }

        float speedValue = NormalizeValue((int)(maximumSpeed * speedMultiplyer));

        float rotationSpeed = xNomalized * speedValue;
        float forwardSpeed = yNomalized * speedValue;
        Debug.Log("" + xNomalized.ToString() + " " + yNomalized.ToString() + " " + speedValue.ToString() + " " + rotationSpeed.ToString() + " " + forwardSpeed.ToString());
        return new Vector2(rotationSpeed, forwardSpeed);
    }

    string GetMovementSendString()
    {
        int motor1Speed = NormalizeValue(forwardSpeedValue - rotationSpeedValue) / 2; // R part 1
        int motor1SpeedPart2 = motor1Speed; // R part 2

        int motor2Speed = NormalizeValue(-(forwardSpeedValue + rotationSpeedValue)) / 2; // L part 1
        int motor2SpeedPart2 = motor2Speed; // L part 2

        char motor1SpeedSign = 'P'; // R sign
        char motor2SpeedSign = 'P'; // L sign

        if (motor1Speed < 0)
        {
            motor1Speed *= -1;
            motor1SpeedPart2 *= -1;
            motor1SpeedSign = 'N';
        }

        if (motor2Speed < 0)
        {
            motor2Speed *= -1;
            motor2SpeedPart2 *= -1;
            motor2SpeedSign = 'N';
        }

        char motor1SpeedSend = (char)motor1Speed; // R part 1
        char motor1SpeedPart2Send = (char)motor1SpeedPart2; // R part 2

        char motor2SpeedSend = (char)motor2Speed; // L part 1
        char motor2SpeedPart2Send = (char)motor2SpeedPart2; // L part 2

        Debug.Log("M" + motor1SpeedSign + motor1SpeedSend + motor1SpeedPart2Send +
            motor2SpeedSign + motor2SpeedSend + motor2SpeedPart2Send);
        Debug.Log("M" + motor1SpeedSign + motor1Speed.ToString() + motor1SpeedPart2.ToString() +
            motor2SpeedSign + motor2Speed.ToString() + motor2SpeedPart2.ToString());
        return "M" + motor1SpeedSign + motor1SpeedSend + motor1SpeedPart2Send +
            motor2SpeedSign + motor2SpeedSend + motor2SpeedPart2Send;
    }

    string GetUpperBodySendString()
    {
        int armSpeed = armSpeedValue / 2; // arm part 1
        int armSpeedPart2 = armSpeed; // arm part 2
        char armSpeedSign = 'P'; // arm sign

        if (armSpeed < 0)
        {
            armSpeed *= -1;
            armSpeedPart2 *= -1;
            armSpeedSign = 'N';
        }

        char armSpeedSend = (char)armSpeed; // arm part 1
        char armSpeedPart2Send = (char)armSpeedPart2; // arm part 2

        return "A" + armSpeedSign + armSpeedSend + armSpeedPart2Send;
    }

    string GetClawSendString()
    {
        int clawSpeed = clawSpeedValue / 2; // arm part 1
        int clawSpeedPart2 = clawSpeed; // arm part 2
        char clawSpeedSign = 'P'; // arm sign

        if (clawSpeed < 0)
        {
            clawSpeed *= -1;
            clawSpeedPart2 *= -1;
            clawSpeedSign = 'N';
        }

        char clawSpeedSend = (char)clawSpeed; // arm part 1
        char clawSpeedPart2Send = (char)clawSpeedPart2; // arm part 2

        return "C" + clawSpeedSign + clawSpeedSend + clawSpeedPart2Send;
    }

    int NormalizeValue(int value)
    {
        if (value < minimumSpeed && value > 0)
        {
            value = minimumSpeed;
        }

        if (value > -minimumSpeed && value < 0)
        {
            value = -minimumSpeed;
        }

        if (value > maximumSpeed)
        {
            value = maximumSpeed;
        }

        if (value < -maximumSpeed)
        {
            value = -maximumSpeed;
        }

        if (value % 2 != 0)
        {
            value -= 1;
        }

        return value;
    }
}