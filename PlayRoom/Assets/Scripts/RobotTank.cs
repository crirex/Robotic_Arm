using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class RobotTank : MonoBehaviour
{
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
    [SerializeField]
    private GameObject grabObject;

    public static bool hasObject = false;

    int forwardSpeedValue = 0;
    int rotationSpeedValue = 0;
    int armSpeedValue = 0;
    int clawSpeedValue = 0;

    float gyroRotation = 0;
    float utrasonicDistance = Constants.maximumUtrasonicDistance;

    private float firstMotorObjectZPosition;
    private float lastUpperBodyRotationZ;
    private float lastClawRotation;

    // Start is called before the first frame update
    void Start()
    {
        firstMotorObjectZPosition = motorObject.transform.localPosition.z;
        lastUpperBodyRotationZ = 0;
        lastClawRotation = 0;
        upperBodyBelow.transform.localRotation = Quaternion.Euler(upperBodyBelow.transform.localRotation.eulerAngles.x, 
            upperBodyBelow.transform.localRotation.eulerAngles.y, -50);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        OculusManager.Instance.UpdateValues();

        Vector2 rotationAndForwardValue = CalculateRotaionAndForwardSpeed();
        rotationSpeedValue = NormalizeValue((int)rotationAndForwardValue.x);
        forwardSpeedValue = NormalizeValue((int)rotationAndForwardValue.y);

        if (Math.Abs(OculusManager.Instance.RightThumbstickUpDown) > Constants.deadZone)
        {
            armSpeedValue = NormalizeValue((int)(-OculusManager.Instance.RightThumbstickUpDown * Constants.maximumSpeed));
        }
        else
        {
            armSpeedValue = 0;
        }

        if ((Math.Abs(OculusManager.Instance.RightHandTrigger) > Constants.deadZone || 
            Math.Abs(OculusManager.Instance.LeftHandTrigger) > Constants.deadZone))
        {
            clawSpeedValue = NormalizeValue((int)((OculusManager.Instance.RightHandTrigger - 
                OculusManager.Instance.LeftHandTrigger) * Constants.maximumSpeed));
        }
        else
        {
            clawSpeedValue = 0;
        }

        MoveLeftRight();
        MoveForwardBackwards();
        MoveUpperBody();
        CloseClawAfterButtonForce();
        MoveGrabableObject();

        if (ManagerIO.Instance.IsOpen)
        {
            ManagerIO.Instance.Write(GetMovementSendString());
            ManagerIO.Instance.Write(GetUpperBodySendString());
            ManagerIO.Instance.Write(GetClawSendString());

            if (ManagerIO.Instance.ReadLenght >= Constants.bytesToReadFromRobot && 
                ManagerIO.Instance.Read() == Constants.syncValue)
            {
                gyroRotation = readFloat();
                utrasonicDistance = readFloat();
            }
        }
        else
        {
            ManagerIO.Instance.Initialize();
        }
    }

    float readFloat()
    {
        byte[] byteArray = new byte[Constants.bytesForFloat];
        for(int i = 0; i < Constants.bytesForFloat; ++i)
        {
            byteArray[i] = (byte)ManagerIO.Instance.Read();
        }
        return BitConverter.ToSingle(byteArray, 0);
    }

    bool IsNotTurned()
    {
        var tankRotations = transform.rotation.eulerAngles;
        if (tankRotations.x > Constants.maximumXRotationInWhichTheTankCanMove || 
            tankRotations.x < Constants.minimumXRotationInWhichTheTankCanMove)
        {
            return false;
        }
        return true;
    }

    void MoveGrabableObject()
    {
        if (utrasonicDistance < Constants.maximumUtrasonicDistance)
        {
            Transform tankTransform = gameObject.GetComponent<Transform>();
            grabObject.transform.position = new Vector3(tankTransform.transform.position.x, grabObject.transform.position.y, 
                tankTransform.transform.position.z + utrasonicDistance * Constants.oneCM + Constants.objectDistanceCorrector);
        }
    }

    void MoveLeftRight()
    {
        if (IsNotTurned())
        {
            var wholeBodyAngles = rotationBody.transform.localRotation.eulerAngles;
            float rotateToValue = 0;
            if (rotationSpeedValue > 0)
            {
                rotateToValue = wholeBodyAngles.z - Constants.rotationRightSpeedCorrector * rotationSpeedValue;
            }
            else
            {
                rotateToValue = wholeBodyAngles.z - Constants.rotationLeftSpeedCorrector * rotationSpeedValue;
            }
            if (!ManagerIO.Instance.IsOpen)
            {
                rotationBody.transform.localRotation = Quaternion.Euler(wholeBodyAngles.x, wholeBodyAngles.y, rotateToValue);
            }
            rotationBody.transform.localRotation = Quaternion.Euler(wholeBodyAngles.x, wholeBodyAngles.y, gyroRotation);
        }
    }

    void MoveForwardBackwards()
    {
        if (IsNotTurned())
        {
            var wholeBodyPosition = movementBody.transform.localPosition;
            float moveToValue = wholeBodyPosition.y + Constants.forwardSpeedCorrector * forwardSpeedValue;
            movementBody.transform.localPosition = new Vector3(wholeBodyPosition.x, moveToValue, wholeBodyPosition.z);
            this.transform.position = movementBody.transform.position;
            movementBody.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    void CloseClawAfterButtonForce()
    {
        if (!hasObject)
        {
            float resultingRotationValue = GetValueInBordersForClaw();
            var rightAngles = rightClaw.transform.localRotation.eulerAngles;
            var leftAngles = leftClaw.transform.localRotation.eulerAngles;
            rightClaw.transform.localRotation = Quaternion.Euler(rightAngles.x, resultingRotationValue, rightAngles.z);
            leftClaw.transform.localRotation = Quaternion.Euler(leftAngles.x, -resultingRotationValue, leftAngles.z);

            float closedPercent = 1 - resultingRotationValue / 
                (Constants.maximumOutwardsClawRotation - Constants.maximumInwardsClawRotation);

            rightClaw.transform.localPosition = new Vector3(
                (Constants.InwardsRightClawXPosition - Constants.OutwardsXPosition) * closedPercent + Constants.OutwardsXPosition,
                rightClaw.transform.localPosition.y,
                (Constants.InwardsRightClawZPosition - Constants.OutwardsZPosition) * closedPercent + Constants.OutwardsZPosition);

            leftClaw.transform.localPosition = new Vector3(
                (Constants.InwardsLeftClawXPosition - Constants.OutwardsXPosition) * closedPercent + Constants.OutwardsXPosition,
                leftClaw.transform.localPosition.y,
                (Constants.InwardsLeftClawZPosition - Constants.OutwardsZPosition) * closedPercent + Constants.OutwardsZPosition);

            motorObject.transform.localPosition = new Vector3(motorObject.transform.localPosition.x,
                motorObject.transform.localPosition.y,
                firstMotorObjectZPosition + Constants.maximumMotorObjectZPositionOffset * closedPercent);
        }
    }

    void MoveUpperBody()
    {
        var upperBodyAngles = upperBodyBelow.transform.localRotation.eulerAngles;
        upperBodyBelow.transform.localRotation = Quaternion.Euler(upperBodyAngles.x, upperBodyAngles.y, 
            GetValueInBordersForUpperBody());
        upperBodyAbove.transform.localRotation = upperBodyBelow.transform.localRotation;
        clawSupport.transform.eulerAngles = new Vector3(270, clawSupport.transform.eulerAngles.y, 
            clawSupport.transform.eulerAngles.z);
    }

    float GetValueInBordersForUpperBody()
    {
        float rotateToValue = lastUpperBodyRotationZ;

        if (armSpeedValue > 0)
        {
            rotateToValue += Constants.armGoingDownSpeedCorrector * armSpeedValue;
        }
        else if (armSpeedValue < 0)
        {
            rotateToValue += Constants.armGoingUpSpeedCorrector * armSpeedValue;
        }
        if (Constants.maximumBackwardsUpperBodyRotation < rotateToValue && 
            rotateToValue < Constants.maximumForwardsUpperBodyRotation)
        {
            lastUpperBodyRotationZ = rotateToValue;
        }
        else if (rotateToValue > Constants.maximumForwardsUpperBodyRotation)
        {
            lastUpperBodyRotationZ = Constants.maximumForwardsUpperBodyRotation;
        }
        else if (rotateToValue < Constants.maximumBackwardsUpperBodyRotation)
        {
            lastUpperBodyRotationZ = Constants.maximumBackwardsUpperBodyRotation;
        }
        return lastUpperBodyRotationZ;
    }

    float GetValueInBordersForClaw()
    {
        float rotateToValue = lastClawRotation + Constants.clawSpeedCorrector * -clawSpeedValue;
        if (Constants.maximumInwardsClawRotation < rotateToValue && rotateToValue < Constants.maximumOutwardsClawRotation)
        {
            lastClawRotation = rotateToValue;
        }
        else if(rotateToValue > Constants.maximumOutwardsClawRotation)
        {
            lastClawRotation = Constants.maximumOutwardsClawRotation;
        }
        else if (rotateToValue < Constants.maximumInwardsClawRotation)
        {
            lastClawRotation = Constants.maximumInwardsClawRotation;
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
            if (Math.Abs(OculusManager.Instance.LeftThumbstickLeftRight) > Constants.deadZone)
            {
                xNomalized = OculusManager.Instance.LeftThumbstickLeftRight;
            }
            //yNomalized = leftThumbstickUpDown / arithmeticMedian;
            if (Math.Abs(OculusManager.Instance.RightIndexTrigger) > Constants.deadZone || 
                Math.Abs(OculusManager.Instance.LeftIndexTrigger) > Constants.deadZone)
            {
                yNomalized = OculusManager.Instance.RightIndexTrigger - OculusManager.Instance.LeftIndexTrigger;
            }
        }

        float speedValue = NormalizeValue((int)(Constants.maximumSpeed * speedMultiplyer));

        float rotationSpeed = xNomalized * speedValue;
        float forwardSpeed = yNomalized * speedValue;
        return new Vector2(rotationSpeed, forwardSpeed);
    }

    string GetMovementSendString()
    {
        int motor1Speed = NormalizeValue(forwardSpeedValue - rotationSpeedValue) / 3; // movement R speed
        int motor2Speed = NormalizeValue(-(forwardSpeedValue + rotationSpeedValue)) / 3; // movement L speed

        char motor1SpeedSign = Constants.positiveSign; // R sign
        char motor2SpeedSign = Constants.positiveSign; // L sign

        if (motor1Speed < 0)
        {
            motor1Speed *= -1;
            motor1SpeedSign = Constants.negativeSign;
        }

        if (motor2Speed < 0)
        {
            motor2Speed *= -1;
            motor2SpeedSign = Constants.negativeSign;
        }

        char motor1SpeedSend = (char)(motor1Speed + Constants.firstSafeAsciiCharacter);
        char motor2SpeedSend = (char)(motor2Speed + Constants.firstSafeAsciiCharacter);

        return Constants.movementOption.ToString() + motor1SpeedSign + motor1SpeedSend +
            motor2SpeedSign + motor2SpeedSend;
    }

    string GetUpperBodySendString()
    {
        int armSpeed = armSpeedValue / 3; // arm speed
        char armSpeedSign = Constants.positiveSign; // arm sign

        if (armSpeed < 0)
        {
            armSpeed *= -1;
            armSpeedSign = Constants.negativeSign;
        }

        char armSpeedSend = (char)(armSpeed + Constants.firstSafeAsciiCharacter);

        return Constants.armOption.ToString() + armSpeedSign + armSpeedSend;
    }

    string GetClawSendString()
    {
        int clawSpeed = clawSpeedValue / 3; // claw speed
        char clawSpeedSign = Constants.positiveSign; // claw sign

        if (clawSpeed < 0)
        {
            clawSpeed *= -1;
            clawSpeedSign = Constants.negativeSign;
        }

        char clawSpeedSend = (char)(clawSpeed + Constants.firstSafeAsciiCharacter);

        return Constants.clawOption.ToString() + clawSpeedSign + clawSpeedSend;
    }

    int NormalizeValue(int value)
    {
        if (value < Constants.minimumSpeed && value > 0)
        {
            value = Constants.minimumSpeed;
        }

        if (value > -Constants.minimumSpeed && value < 0)
        {
            value = -Constants.minimumSpeed;
        }

        if (value > Constants.maximumSpeed)
        {
            value = Constants.maximumSpeed;
        }

        if (value < -Constants.maximumSpeed)
        {
            value = -Constants.maximumSpeed;
        }
        return value;
    }
}