using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotTankModel: MonoBehaviour
{
    #region Public static members
    [SerializeField]
    public static GameObject robotArmTank;
    [SerializeField]
    public static GameObject leftClaw;
    [SerializeField]
    public static GameObject rightClaw;
    [SerializeField]
    public static GameObject upperBodyBelow;
    [SerializeField]
    public static GameObject upperBodyAbove;
    [SerializeField]
    public static GameObject clawSupport;
    [SerializeField]
    public static GameObject movementBody;
    [SerializeField]
    public static GameObject rotationBody;
    [SerializeField]
    public static GameObject motorObject;
    [SerializeField]
    public static GameObject grabObject;

    public static float firstMotorObjectZPosition;
    #endregion

    #region Methods
    void Start()
    {
        robotArmTank = GameObject.Find("RobotArmTank");
        leftClaw = GameObject.Find("LeftClaw");
        rightClaw = GameObject.Find("RightClaw");
        upperBodyBelow = GameObject.Find("UpperBodyBelow");
        upperBodyAbove = GameObject.Find("UpperBodyAbove");
        clawSupport = GameObject.Find("ClawSupport");
        movementBody = GameObject.Find("MovementBody");
        rotationBody = GameObject.Find("RotationBody");
        motorObject = GameObject.Find("MotorAffectedObjects");
        grabObject = GameObject.Find("GrabCube");
        firstMotorObjectZPosition = motorObject.transform.localPosition.z;
        upperBodyBelow.transform.localRotation = Quaternion.Euler(
            upperBodyBelow.transform.localRotation.eulerAngles.x,
            upperBodyBelow.transform.localRotation.eulerAngles.y,
            -50);
    }
    #endregion
}
