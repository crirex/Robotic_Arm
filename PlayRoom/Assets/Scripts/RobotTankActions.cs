using System;
using UnityEngine;

public class RobotTankActions: Singleton<RobotTankActions>
{
    #region Private members
    private int forwardSpeedValue = 0;
    private int rotationSpeedValue = 0;
    private int armSpeedValue = 0;
    private int clawSpeedValue = 0;

    private float gyroRotation = 0;
    private float utrasonicDistance = Constants.maximumUtrasonicDistance;

    private float lastUpperBodyRotationZ;
    private float lastClawRotation;
    #endregion

    #region Constructors
    private RobotTankActions()
    {
        lastUpperBodyRotationZ = 0;
        lastClawRotation = 0;
    }
    #endregion

    #region Methods
    public void UpdateRobot()
    {
        OculusManager.Instance.UpdateValues();
        UpdateValues();
        MoveRobotParts();
        ManageData();
    }

    void ManageData()
    {
        if (ManagerIO.Instance.IsOpen)
        {
            ManagerIO.Instance.Write(GetMovementSendString());
            ManagerIO.Instance.Write(GetUpperBodySendString());
            ManagerIO.Instance.Write(GetClawSendString());

            if (ManagerIO.Instance.ReadLenght >= Constants.bytesToReadFromRobot &&
                ManagerIO.Instance.Read() == Constants.syncValue)
            {
                gyroRotation = ManagerIO.Instance.ReadFloat();
                utrasonicDistance = ManagerIO.Instance.ReadFloat();
            }
        }
        else
        {
            ManagerIO.Instance.Initialize();
        }
    }

    void MoveRobotParts()
    {
        MoveLeftRight();
        MoveForwardBackwards();
        MoveUpperBody();
        CloseClawAfterButtonForce();
        MoveGrabableObject();
    }

    void UpdateValues()
    {
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
    }

    bool IsNotTurned()
    {
        var tankRotations = RobotTankModel.robotArmTank.transform.rotation.eulerAngles;
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
            Transform tankTransform = RobotTankModel.robotArmTank.transform;
            RobotTankModel.grabObject.transform.position = new Vector3(tankTransform.transform.position.x, RobotTankModel.grabObject.transform.position.y,
                tankTransform.transform.position.z + utrasonicDistance * Constants.oneCM + Constants.objectDistanceCorrector);
        }
    }

    void MoveLeftRight()
    {
        if (IsNotTurned())
        {
            var wholeBodyAngles = RobotTankModel.rotationBody.transform.localRotation.eulerAngles;
            float rotateToValue;
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
                RobotTankModel.rotationBody.transform.localRotation = Quaternion.Euler(wholeBodyAngles.x, wholeBodyAngles.y, rotateToValue);
            }
            else
            {
                RobotTankModel.rotationBody.transform.localRotation = Quaternion.Euler(wholeBodyAngles.x, wholeBodyAngles.y, gyroRotation);
            }
        }
    }

    void MoveForwardBackwards()
    {
        if (IsNotTurned())
        {
            var wholeBodyPosition = RobotTankModel.movementBody.transform.localPosition;
            float moveToValue = wholeBodyPosition.y + Constants.forwardSpeedCorrector * forwardSpeedValue;
            RobotTankModel.movementBody.transform.localPosition = new Vector3(wholeBodyPosition.x, moveToValue, wholeBodyPosition.z);
            RobotTankModel.robotArmTank.transform.position = RobotTankModel.movementBody.transform.position;
            RobotTankModel.movementBody.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    void CloseClawAfterButtonForce()
    {
        if (!GetGrabbed.grabbed)
        {
            float resultingRotationValue = GetValueInBordersForClaw();
            var rightAngles = RobotTankModel.rightClaw.transform.localRotation.eulerAngles;
            var leftAngles = RobotTankModel.leftClaw.transform.localRotation.eulerAngles;
            RobotTankModel.rightClaw.transform.localRotation = Quaternion.Euler(rightAngles.x, resultingRotationValue, rightAngles.z);
            RobotTankModel.leftClaw.transform.localRotation = Quaternion.Euler(leftAngles.x, -resultingRotationValue, leftAngles.z);

            float closedPercent = 1 - resultingRotationValue /
                (Constants.maximumOutwardsClawRotation - Constants.maximumInwardsClawRotation);

            RobotTankModel.rightClaw.transform.localPosition = new Vector3(
                (Constants.InwardsRightClawXPosition - Constants.OutwardsXPosition) * closedPercent + Constants.OutwardsXPosition,
                RobotTankModel.rightClaw.transform.localPosition.y,
                (Constants.InwardsRightClawZPosition - Constants.OutwardsZPosition) * closedPercent + Constants.OutwardsZPosition);

            RobotTankModel.leftClaw.transform.localPosition = new Vector3(
                (Constants.InwardsLeftClawXPosition - Constants.OutwardsXPosition) * closedPercent + Constants.OutwardsXPosition,
                RobotTankModel.leftClaw.transform.localPosition.y,
                (Constants.InwardsLeftClawZPosition - Constants.OutwardsZPosition) * closedPercent + Constants.OutwardsZPosition);

            RobotTankModel.motorObject.transform.localPosition = new Vector3(RobotTankModel.motorObject.transform.localPosition.x,
                RobotTankModel.motorObject.transform.localPosition.y,
                RobotTankModel.firstMotorObjectZPosition + Constants.maximumMotorObjectZPositionOffset * closedPercent);
        }
    }

    void MoveUpperBody()
    {
        var upperBodyAngles = RobotTankModel.upperBodyBelow.transform.localRotation.eulerAngles;
        RobotTankModel.upperBodyBelow.transform.localRotation = Quaternion.Euler(upperBodyAngles.x, upperBodyAngles.y,
            GetValueInBordersForUpperBody());
        RobotTankModel.upperBodyAbove.transform.localRotation = RobotTankModel.upperBodyBelow.transform.localRotation;
        RobotTankModel.clawSupport.transform.eulerAngles = new Vector3(270, RobotTankModel.clawSupport.transform.eulerAngles.y,
            RobotTankModel.clawSupport.transform.eulerAngles.z);
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
        else if (rotateToValue > Constants.maximumOutwardsClawRotation)
        {
            lastClawRotation = Constants.maximumOutwardsClawRotation;
        }
        else if (rotateToValue < Constants.maximumInwardsClawRotation)
        {
            lastClawRotation = Constants.maximumInwardsClawRotation;
        }
        return lastClawRotation;
    }

    public Vector2 CalculateRotaionAndForwardSpeed()
    {
        float xNomalized = 0f;
        float yNomalized = 0f;
        if (Math.Abs(OculusManager.Instance.LeftThumbstickLeftRight) > Constants.deadZone)
        {
            xNomalized = OculusManager.Instance.LeftThumbstickLeftRight;
        }
        if (Math.Abs(OculusManager.Instance.RightIndexTrigger) > Constants.deadZone ||
            Math.Abs(OculusManager.Instance.LeftIndexTrigger) > Constants.deadZone)
        {
            yNomalized = OculusManager.Instance.RightIndexTrigger - OculusManager.Instance.LeftIndexTrigger;
        }

        float speedValue = NormalizeValue((int)(Constants.maximumSpeed));

        float rotationSpeed = xNomalized * speedValue;
        float forwardSpeed = yNomalized * speedValue;
        return new Vector2(rotationSpeed, forwardSpeed);
    }

    private Vector2 ALT_CalculateRotaionAndForwardSpeed()
    {
        float xNomalized = 0f;
        float yNomalized = 0f;
        float arithmeticMedian = Math.Abs(OculusManager.Instance.LeftThumbstickUpDown) +
            Math.Abs(OculusManager.Instance.LeftThumbstickLeftRight);
        float speedMultiplyer = (float)Math.Sqrt(Math.Pow(Math.Sin(Math.PI * OculusManager.Instance.LeftThumbstickUpDown), 2) +
            Math.Pow(Math.Cos(Math.PI * OculusManager.Instance.LeftThumbstickLeftRight), 2)) / Constants.maximumRadius;

        if (arithmeticMedian != 0)
        {
            if (Math.Abs(OculusManager.Instance.LeftThumbstickLeftRight) > Constants.deadZone)
            {
                xNomalized = OculusManager.Instance.LeftThumbstickLeftRight / arithmeticMedian;
            }
            if (Math.Abs(OculusManager.Instance.RightIndexTrigger) > Constants.deadZone ||
                Math.Abs(OculusManager.Instance.LeftIndexTrigger) > Constants.deadZone)
            {
                yNomalized = OculusManager.Instance.LeftThumbstickUpDown / arithmeticMedian;
            }
        }

        float speedValue = NormalizeValue((int)(Constants.maximumSpeed * speedMultiplyer));

        float rotationSpeed = xNomalized * speedValue;
        float forwardSpeed = yNomalized * speedValue;
        return new Vector2(rotationSpeed, forwardSpeed);
    }

    string GetMovementSendString()
    {
        int motor1Speed = NormalizeValue(forwardSpeedValue - rotationSpeedValue) / 
            Constants.normalizationValueForSendData; // movement R speed
        int motor2Speed = NormalizeValue(-(forwardSpeedValue + rotationSpeedValue)) / 
            Constants.normalizationValueForSendData; // movement L speed

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
        int armSpeed = armSpeedValue / Constants.normalizationValueForSendData; // arm speed
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
        int clawSpeed = clawSpeedValue / Constants.normalizationValueForSendData; // claw speed
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
    #endregion
}
