using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class RobotTank : MonoBehaviour
{
    public SerialPort serial = new SerialPort("COM4", 115200);

    [SerializeField]
    private GameObject leftClaw;
    [SerializeField]
    private GameObject rightClaw;
    [SerializeField]
    private GameObject upperBodyBelow;
    [SerializeField]
    private GameObject upperBodyAbove;
    [SerializeField]
    private GameObject clawSupport;
    [SerializeField]
    private GameObject movementBody;
    [SerializeField]
    private GameObject rotationBody;
    [SerializeField]
    private GameObject motorObject;

    public static bool hasObject = false;

    float rightHandTrigger;
    float leftHandTrigger;
    float rightIndexTrigger;
    float leftIndexTrigger;
    float leftThumbstickUpDown;
    float leftThumbstickLeftRight;
    float rightThumbstickUpDown;
    float rightThumbstickLeftRight;
    Vector3 leftControllerPosition;
    Vector3 rightControllerPosition;
    Vector3 leftControllerRotation;
    Vector3 rightControllerRotation;

    private const float maximumOutwardsClawRotation = 0f; //Rotations tested for right claw, left claw has oposite rotations
    //private const float maximumInwardsClawRotation = -28.2f;
    private const float maximumInwardsClawRotation = -32.8f;
    private const float OutwardsXPosition = 0.0654f;
    private const float InwardsRightClawXPosition = 0.0716f;
    private const float InwardsLeftClawXPosition = OutwardsXPosition - (InwardsRightClawXPosition - OutwardsXPosition);
    private const float OutwardsZPosition = 0.2178f;
    private const float InwardsRightClawZPosition = 0.2062f;
    private const float InwardsLeftClawZPosition = OutwardsZPosition - (OutwardsZPosition - InwardsRightClawZPosition);
    private const float maximumMotorObjectZPositionOffset = -0.01f;
    private const float maximumBackwardsUpperBodyRotation = -95.3f;
    private const float maximumForwardsUpperBodyRotation = 16f;

    private const int maximumSpeed = 150; // This can be put all the way to 255 only but the tests are going to be for 150 as it's speed is adequate and it would be okay for simulations that the speed is the same.
    private const int minimumSpeed = 150;

    private const int firstSafeAsciiCharacter = 33;
    private const int lastSafeAsciiCharacter = 126;

    private const float rotationRightSpeedCorrector = 0.0095f; // It seems rotation to the left and right are different, this time i don't think it's gravity tho... idk what it is.
    private const float rotationLeftSpeedCorrector = 0.01f; // It seems rotation to the left and right are different, this time i don't think it's gravity tho... idk what it is.
    private const float forwardSpeedCorrector = 0.00002724f; // This value is sync
    private const float armGoingDownSpeedCorrector = 0.0031f; // Because of gravity it moves up slower. //This value is sync
    private const float armGoingUpSpeedCorrector = 0.0029f; // This value is sync
    private const float clawSpeedCorrector = 0.0014f;

    private const float maximumRadius = 1f;

    int forwardSpeedValue = 0;
    int rotationSpeedValue = 0;
    int armSpeedValue = 0;
    int clawSpeedValue = 0;

    private float firstMotorObjectZPosition;
    private float lastUpperBodyRotationZ;
    private float lastClawRotation;

    // Start is called before the first frame update
    void Start()
    {
        serial.Open();
        serial.ReadTimeout = 1;

        firstMotorObjectZPosition = motorObject.transform.localPosition.z;
        lastUpperBodyRotationZ = 0;
        lastClawRotation = 0;
        upperBodyBelow.transform.localRotation = Quaternion.Euler(upperBodyBelow.transform.localRotation.eulerAngles.x, upperBodyBelow.transform.localRotation.eulerAngles.y, -50);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rightHandTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
        leftHandTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);

        rightIndexTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        leftIndexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);

        leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

        leftControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles;
        rightControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch).eulerAngles;

        leftThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        rightThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;

        leftThumbstickLeftRight = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        rightThumbstickLeftRight = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        Vector2 rotationAndForwardValue = CalculateRotaionAndForwardSpeed();
        rotationSpeedValue = NormalizeValue((int)rotationAndForwardValue.x);
        forwardSpeedValue = NormalizeValue((int)rotationAndForwardValue.y);

        if (Math.Abs(rightThumbstickUpDown) > 0.2)
        {
            armSpeedValue = NormalizeValue((int)(-rightThumbstickUpDown * maximumSpeed));
        }
        else
        {
            armSpeedValue = 0;
        }

        if ((Math.Abs(rightHandTrigger) > 0.1 || Math.Abs(leftHandTrigger) > 0.1))
        {
            clawSpeedValue = NormalizeValue((int)((rightHandTrigger - leftHandTrigger) * maximumSpeed));
        }
        else
        {
            clawSpeedValue = 0;
        }

        //Debug.Log(""+forwardSpeedValue + " " + rotationSpeedValue + " " + armSpeedValue + " " + clawSpeedValue);

        MoveLeftRight();
        MoveForwardBackwards();
        MoveUpperBody();
        CloseClawAfterButtonForce();

        //if (serial.IsOpen)
        //{
            // if (serial.BytesToRead > 7 && (char)serial.ReadByte() == 'D')
            //{
            //Debug.Log("X:" + readDouble());
            //Debug.Log("Y:" + readDouble());
            //Debug.Log("Z:" + readDouble());
            //Debug.Log("gyroX:" + readDouble());
            //Debug.Log("gyroY:" + readDouble());
            //Debug.Log("Utrasonic:" + readDouble() + "cm");
            //Debug.Log(serial.ReadByte());
            //Debug.Log(serial.BytesToRead);
            //}

            serial.Write(GetMovementSendString());
            serial.Write(GetUpperBodySendString());
            serial.Write(GetClawSendString());
        //}
    }

    double readDouble()
    {
        byte[] byteArray = new byte[8];
        byteArray[0] = (byte)serial.ReadByte();
        byteArray[1] = (byte)serial.ReadByte();
        byteArray[2] = (byte)serial.ReadByte();
        byteArray[3] = (byte)serial.ReadByte();
        byteArray[4] = (byte)serial.ReadByte();
        byteArray[5] = (byte)serial.ReadByte();
        byteArray[6] = (byte)serial.ReadByte();
        byteArray[7] = (byte)serial.ReadByte();
        return BitConverter.ToDouble(byteArray, 0);
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
        if (IsNotTurned())
        {
            var wholeBodyAngles = rotationBody.transform.localRotation.eulerAngles;
            float rotateToValue = 0;
            if (rotationSpeedValue > 0)
            {
                rotateToValue = wholeBodyAngles.z - rotationRightSpeedCorrector * rotationSpeedValue;
            }
            else
            {
                rotateToValue = wholeBodyAngles.z - rotationLeftSpeedCorrector * rotationSpeedValue;
            }
            //Debug.Log(rotationRightSpeedCorrector * rotationSpeedValue);
            rotationBody.transform.localRotation = Quaternion.Euler(wholeBodyAngles.x, wholeBodyAngles.y, rotateToValue);
        }
    }

    void MoveForwardBackwards()
    {
        if (IsNotTurned())
        {
            var wholeBodyPosition = movementBody.transform.localPosition;
            float moveToValue = wholeBodyPosition.y + forwardSpeedCorrector * forwardSpeedValue;
            //Debug.Log(forwardSpeedCorrector * forwardSpeedValue);
            movementBody.transform.localPosition = new Vector3(wholeBodyPosition.x, moveToValue, wholeBodyPosition.z);
            this.transform.position = movementBody.transform.position;
            movementBody.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    void CloseClawAfterButtonForce()
    {
        if (!hasObject)
        {
            //float resultingRotationValue = (maximumOutwardsClawRotation - maximumInwardsClawRotation) * (1 - rightHandTrigger) + maximumInwardsClawRotation;
            float resultingRotationValue = GetValueInBordersForClaw();
            var rightAngles = rightClaw.transform.localRotation.eulerAngles;
            var leftAngles = leftClaw.transform.localRotation.eulerAngles;
            rightClaw.transform.localRotation = Quaternion.Euler(rightAngles.x, resultingRotationValue, rightAngles.z);
            leftClaw.transform.localRotation = Quaternion.Euler(leftAngles.x, -resultingRotationValue, leftAngles.z);

            float closedPercent = 1 - resultingRotationValue / (maximumOutwardsClawRotation - maximumInwardsClawRotation);

            rightClaw.transform.localPosition = new Vector3(
                (InwardsRightClawXPosition - OutwardsXPosition) * closedPercent + OutwardsXPosition,
                rightClaw.transform.localPosition.y,
                (InwardsRightClawZPosition - OutwardsZPosition) * closedPercent + OutwardsZPosition);

            leftClaw.transform.localPosition = new Vector3(
                (InwardsLeftClawXPosition - OutwardsXPosition) * closedPercent + OutwardsXPosition,
                leftClaw.transform.localPosition.y,
                (InwardsLeftClawZPosition - OutwardsZPosition) * closedPercent + OutwardsZPosition);

            motorObject.transform.localPosition = new Vector3(motorObject.transform.localPosition.x,
                motorObject.transform.localPosition.y,
                firstMotorObjectZPosition + maximumMotorObjectZPositionOffset * closedPercent);
        }
    }

    void MoveUpperBody()
    {
        var upperBodyAngles = upperBodyBelow.transform.localRotation.eulerAngles;
        //Debug.Log(GetValueInBordersForUpperBody());
        upperBodyBelow.transform.localRotation = Quaternion.Euler(upperBodyAngles.x, upperBodyAngles.y, GetValueInBordersForUpperBody());
        upperBodyAbove.transform.localRotation = upperBodyBelow.transform.localRotation;
        clawSupport.transform.eulerAngles = new Vector3(270, clawSupport.transform.eulerAngles.y, clawSupport.transform.eulerAngles.z);
    }

    float GetValueInBordersForUpperBody()
    {
        float rotateToValue = lastUpperBodyRotationZ;

        if (armSpeedValue > 0)
        {
            rotateToValue += armGoingDownSpeedCorrector * armSpeedValue;
        }
        else if (armSpeedValue < 0)
        {
            rotateToValue += armGoingUpSpeedCorrector * armSpeedValue;
        }
        if (maximumBackwardsUpperBodyRotation < rotateToValue && rotateToValue < maximumForwardsUpperBodyRotation)
        {
            lastUpperBodyRotationZ = rotateToValue;
        }
        else if (rotateToValue > maximumForwardsUpperBodyRotation)
        {
            lastUpperBodyRotationZ = maximumForwardsUpperBodyRotation;
        }
        else if (rotateToValue < maximumBackwardsUpperBodyRotation)
        {
            lastUpperBodyRotationZ = maximumBackwardsUpperBodyRotation;
        }
        return lastUpperBodyRotationZ;
    }

    float GetValueInBordersForClaw()
    {
        float rotateToValue = lastClawRotation + clawSpeedCorrector * -clawSpeedValue;
        if (maximumInwardsClawRotation < rotateToValue && rotateToValue < maximumOutwardsClawRotation)
        {
            lastClawRotation = rotateToValue;
        }
        else if(rotateToValue > maximumOutwardsClawRotation)
        {
            lastClawRotation = maximumOutwardsClawRotation;
        }
        else if (rotateToValue < maximumInwardsClawRotation)
        {
            lastClawRotation = maximumInwardsClawRotation;
        }
        return lastClawRotation;
    }

    Vector2 CalculateRotaionAndForwardSpeed()
    {
        float xNomalized = 0;
        float yNomalized = 0;
        //float arithmeticMedian = Math.Abs(leftThumbstickUpDown) + Math.Abs(leftThumbstickLeftRight);
        float speedMultiplyer = 1;// (float)Math.Sqrt(Math.Pow(Math.Sin(Math.PI * leftThumbstickUpDown), 2) + Math.Pow(Math.Cos(Math.PI * leftThumbstickLeftRight), 2)) / maximumRadius;

        //if (arithmeticMedian != 0)
        {
            //xNomalized = leftThumbstickLeftRight / arithmeticMedian;
            xNomalized = leftThumbstickLeftRight;
            //yNomalized = leftThumbstickUpDown / arithmeticMedian;
            yNomalized = rightIndexTrigger-leftIndexTrigger;
        }

        float speedValue = NormalizeValue((int)(maximumSpeed * speedMultiplyer));

        float rotationSpeed = xNomalized * speedValue;
        float forwardSpeed = yNomalized * speedValue;
        return new Vector2(rotationSpeed, forwardSpeed);
    }

    string GetMovementSendString()
    {
        int motor1Speed = NormalizeValue(forwardSpeedValue - rotationSpeedValue) / 3; // movement R speed
        int motor2Speed = NormalizeValue(-(forwardSpeedValue + rotationSpeedValue)) / 3; // movement L speed

        char motor1SpeedSign = 'P'; // R sign
        char motor2SpeedSign = 'P'; // L sign

        if (motor1Speed < 0)
        {
            motor1Speed *= -1;
            motor1SpeedSign = 'N';
        }

        if (motor2Speed < 0)
        {
            motor2Speed *= -1;
            motor2SpeedSign = 'N';
        }

        char motor1SpeedSend = (char)(motor1Speed + firstSafeAsciiCharacter);
        char motor2SpeedSend = (char)(motor2Speed + firstSafeAsciiCharacter);

        return "M" + motor1SpeedSign + motor1SpeedSend +
            motor2SpeedSign + motor2SpeedSend;
    }

    string GetUpperBodySendString()
    {
        int armSpeed = armSpeedValue / 3; // arm speed
        char armSpeedSign = 'P'; // arm sign

        if (armSpeed < 0)
        {
            armSpeed *= -1;
            armSpeedSign = 'N';
        }

        char armSpeedSend = (char)(armSpeed + firstSafeAsciiCharacter);

        return "A" + armSpeedSign + armSpeedSend;
    }

    string GetClawSendString()
    {
        int clawSpeed = clawSpeedValue / 3; // claw speed
        char clawSpeedSign = 'P'; // arm sign

        if (clawSpeed < 0)
        {
            clawSpeed *= -1;
            clawSpeedSign = 'N';
        }

        char clawSpeedSend = (char)(clawSpeed + firstSafeAsciiCharacter);

        return "C" + clawSpeedSign + clawSpeedSend;
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
        return value;
    }
}