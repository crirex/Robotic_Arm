using System;
using UnityEngine;

public class RobotTankActions: Singleton<RobotTankActions>
{
    #region Private Proprieties
    public int ForwardSpeedValue { get; set; } = 0;
    public int RotationSpeedValue { get; set; } = 0;
    public int ArmSpeedValue { get; set; } = 0;
    public int ClawSpeedValue { get; set; } = 0;
    #endregion

    #region Private Members
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

    #region Public Methods
    public void UpdateRobot()
    {
        OculusManager.Instance.UpdateValues();
        PhysicalTankDataManager.Instance.UpdateData();
        UpdateValues();

        MoveRobotParts();
        PhysicalTankDataManager.Instance.SendAllData(ForwardSpeedValue, RotationSpeedValue, ArmSpeedValue, ClawSpeedValue);
    }

    public void UpdateValues()
    {
        RotationSpeedValue = 0;
        ForwardSpeedValue = 0;
        ClawSpeedValue = 0;
        ArmSpeedValue = 0;

        if (OculusManager.Instance.AreTouchControllersDetected)
        {
            Vector2Int rotationAndForwardValue = CalculateRotaionAndForwardSpeed();
            RotationSpeedValue = PhysicalTankDataManager.Instance.NormalizeValue(rotationAndForwardValue.x);
            ForwardSpeedValue = PhysicalTankDataManager.Instance.NormalizeValue(rotationAndForwardValue.y);

            if (Math.Abs(OculusManager.Instance.RightThumbstickUpDown) > Constants.deadZone)
            {
                ArmSpeedValue = PhysicalTankDataManager.Instance.NormalizeValue(
                    (int)(-OculusManager.Instance.RightThumbstickUpDown * Constants.maximumSpeed));
            }

            if ((Math.Abs(OculusManager.Instance.RightHandTrigger) > Constants.deadZone ||
                Math.Abs(OculusManager.Instance.LeftHandTrigger) > Constants.deadZone))
            {
                ClawSpeedValue = PhysicalTankDataManager.Instance.NormalizeValue((int)((OculusManager.Instance.RightHandTrigger -
                    OculusManager.Instance.LeftHandTrigger) * Constants.maximumSpeed));
            }
        }
    }

    public void MoveRobotParts()
    {
        MoveLeftRight();
        MoveForwardBackwards();
        MoveUpperBody();
        CloseOpenClaw();
        MoveGrabableObject();
    }
    #endregion

    #region Private Methods
    private void MoveLeftRight()
    {
        if (IsNotTurned())
        {
            var wholeBodyAngles = RobotTankModel.rotationBody.transform.localRotation.eulerAngles;
            float rotateToValue;
            if (!ManagerIO.Instance.IsOpen)
            {
                if (RotationSpeedValue > 0)
                {
                    rotateToValue = wholeBodyAngles.z - Constants.rotationRightSpeedCorrector * RotationSpeedValue;
                }
                else
                {
                    rotateToValue = wholeBodyAngles.z - Constants.rotationLeftSpeedCorrector * RotationSpeedValue;
                }
            }
            else
            {
                rotateToValue = PhysicalTankDataManager.Instance.GyroscopeRotation;
            }
            RobotTankModel.rotationBody.transform.localRotation = 
                Quaternion.Euler(wholeBodyAngles.x, wholeBodyAngles.y, rotateToValue);
        }
    }

    private void MoveForwardBackwards()
    {
        if (IsNotTurned())
        {
            var wholeBodyPosition = RobotTankModel.movementBody.transform.localPosition;
            float moveToValue = wholeBodyPosition.y + Constants.forwardSpeedCorrector * ForwardSpeedValue;
            RobotTankModel.movementBody.transform.localPosition = 
                new Vector3(wholeBodyPosition.x, moveToValue, wholeBodyPosition.z);
            RobotTankModel.robotArmTank.transform.position = RobotTankModel.movementBody.transform.position;
            RobotTankModel.movementBody.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    private void MoveUpperBody()
    {
        var upperBodyAngles = RobotTankModel.upperBodyBelow.transform.localRotation.eulerAngles;
        RobotTankModel.upperBodyBelow.transform.localEulerAngles = 
            new Vector3(upperBodyAngles.x, upperBodyAngles.y, GetValueInBordersForUpperBody());
        RobotTankModel.upperBodyAbove.transform.localRotation = RobotTankModel.upperBodyBelow.transform.localRotation;
        RobotTankModel.clawSupport.transform.eulerAngles = new Vector3(270, RobotTankModel.clawSupport.transform.eulerAngles.y,
            RobotTankModel.clawSupport.transform.eulerAngles.z);
    }

    private void CloseOpenClaw()
    {
        if (!GetGrabbed.grabbed)
        {
            float resultingRotationValue = GetValueInBordersForClaw();
            var rightAngles = RobotTankModel.rightClaw.transform.localRotation.eulerAngles;
            var leftAngles = RobotTankModel.leftClaw.transform.localRotation.eulerAngles;

            RobotTankModel.rightClaw.transform.localRotation = 
                Quaternion.Euler(rightAngles.x, resultingRotationValue, rightAngles.z);
            RobotTankModel.leftClaw.transform.localRotation = 
                Quaternion.Euler(leftAngles.x, -resultingRotationValue, leftAngles.z);

            float closedPercent = (resultingRotationValue - Constants.minimumClawRotation) /
                (Constants.maximumClawRotation - Constants.minimumClawRotation);


            float clawXPosition = (Constants.maximumClawXPosition - Constants.minimumClawXPosition) * 
                closedPercent + Constants.minimumClawXPosition;

            float clawZPosition = (Constants.maximumClawZPosition - Constants.minimumClawZPosition) * 
                closedPercent + Constants.minimumClawZPosition;

            RobotTankModel.rightClaw.transform.localPosition = new Vector3(
                clawXPosition,
                RobotTankModel.rightClaw.transform.localPosition.y,
                clawZPosition);

            RobotTankModel.leftClaw.transform.localPosition = new Vector3(
                -clawXPosition,
                RobotTankModel.leftClaw.transform.localPosition.y,
                clawZPosition);


            float motorObjectZPosition = (Constants.maximumMotorObjectZPosition - Constants.minimumMotorObjectZPosition) *
                closedPercent + Constants.minimumMotorObjectZPosition;

            RobotTankModel.motorObject.transform.localPosition = new Vector3(
                RobotTankModel.motorObject.transform.localPosition.x,
                RobotTankModel.motorObject.transform.localPosition.y,
                motorObjectZPosition);


            float clawGrabberXPosition = (Constants.maximumClawGrabberXPosition - Constants.minimumClawGrabberXPosition) * 
                closedPercent + Constants.minimumClawGrabberXPosition;

            RobotTankModel.rightClawGrabber.transform.localPosition = new Vector3(
                clawGrabberXPosition,
                RobotTankModel.rightClawGrabber.transform.localPosition.y,
                RobotTankModel.rightClawGrabber.transform.localPosition.z);

            RobotTankModel.leftClawGrabber.transform.localPosition = new Vector3(
                -clawGrabberXPosition,
                RobotTankModel.leftClawGrabber.transform.localPosition.y,
                RobotTankModel.leftClawGrabber.transform.localPosition.z);
        }
    }

    private void MoveGrabableObject()
    {
        if (PhysicalTankDataManager.Instance.UtrasonicDistance < Constants.maximumUtrasonicDistance)
        {
            Transform tankTransform = RobotTankModel.robotArmTank.transform;
            RobotTankModel.grabObject.transform.position = new Vector3(
                tankTransform.transform.position.x, 
                RobotTankModel.grabObject.transform.position.y,
                tankTransform.transform.position.z + 
                PhysicalTankDataManager.Instance.UtrasonicDistance * Constants.oneCM + Constants.objectDistanceCorrector);
        }
    }

    private float GetValueInBordersForUpperBody()
    {
        float rotateToValue = lastUpperBodyRotationZ;

        if (ArmSpeedValue > 0)
        {
            rotateToValue += Constants.armGoingDownSpeedCorrector * ArmSpeedValue;
        }
        else if (ArmSpeedValue < 0)
        {
            rotateToValue += Constants.armGoingUpSpeedCorrector * ArmSpeedValue;
        }
        if (Constants.minimumUpperBodyRotation < rotateToValue &&
            rotateToValue < Constants.maximumUpperBodyRotation)
        {
            lastUpperBodyRotationZ = rotateToValue;
        }
        else if (rotateToValue > Constants.maximumUpperBodyRotation)
        {
            lastUpperBodyRotationZ = Constants.maximumUpperBodyRotation;
        }
        else if (rotateToValue < Constants.minimumUpperBodyRotation)
        {
            lastUpperBodyRotationZ = Constants.minimumUpperBodyRotation;
        }
        return lastUpperBodyRotationZ;
    }

    private float GetValueInBordersForClaw()
    {
        float rotateToValue = lastClawRotation + Constants.clawSpeedCorrector * -ClawSpeedValue;
        if (Constants.minimumClawRotation < rotateToValue && rotateToValue < Constants.maximumClawRotation)
        {
            lastClawRotation = rotateToValue;
        }
        else if (rotateToValue > Constants.maximumClawRotation)
        {
            lastClawRotation = Constants.maximumClawRotation;
        }
        else if (rotateToValue < Constants.minimumClawRotation)
        {
            lastClawRotation = Constants.minimumClawRotation;
        }
        return lastClawRotation;
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

    private Vector2Int CalculateRotaionAndForwardSpeed()
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

        float speedValue = PhysicalTankDataManager.Instance.NormalizeValue((int)(Constants.maximumSpeed));

        int rotationSpeed = (int)(xNomalized * speedValue);
        int forwardSpeed = (int)(yNomalized * speedValue);
        return new Vector2Int(rotationSpeed, forwardSpeed);
    }
    #endregion
}
