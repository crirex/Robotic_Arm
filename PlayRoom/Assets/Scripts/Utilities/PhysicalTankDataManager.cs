using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalTankDataManager : Singleton<PhysicalTankDataManager>
{
    #region Proprieties
    public float GyroscopeRotation { get; private set; } = 0.0f;
    public float UtrasonicDistance { get; private set; } = Constants.maximumUtrasonicDistance;
    #endregion

    #region Constructors
    private PhysicalTankDataManager() { }
    #endregion

    #region Public Methods
    public void UpdateData()
    {
        if (ManagerIO.Instance.IsOpen)
        {
            if (ManagerIO.Instance.ReadLenght >= Constants.bytesToReadFromRobot &&
                ManagerIO.Instance.Read() == Constants.syncValue)
            {
                GyroscopeRotation = ManagerIO.Instance.ReadFloat();
                UtrasonicDistance = ManagerIO.Instance.ReadFloat();
            }
        }
        else
        {
            ManagerIO.Instance.Initialize();
        }
    }

    public void SendAllData(int forwardSpeedValue, int rotationSpeedValue, int armSpeedValue, int clawSpeedValue)
    {
        if (ManagerIO.Instance.IsOpen)
        {
            ManagerIO.Instance.Write(GetMovementSendString(forwardSpeedValue, rotationSpeedValue));
            ManagerIO.Instance.Write(GetUpperBodySendString(armSpeedValue));
            ManagerIO.Instance.Write(GetClawSendString(clawSpeedValue));
        }
        else
        {
            ManagerIO.Instance.Initialize();
        }
    }

    public int NormalizeValue(int value)
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

    #region Private Methods
    private string GetMovementSendString(int forwardSpeedValue, int rotationSpeedValue)
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

    private string GetUpperBodySendString(int armSpeedValue)
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

    private string GetClawSendString(int clawSpeedValue)
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
    #endregion
}
