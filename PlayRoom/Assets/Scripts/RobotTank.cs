using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class RobotTank : MonoBehaviour
{
    public SerialPort serial = new SerialPort("COM7", 9600);

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
    private const float rotationSpeed = 1f;
    private const float movementSpeed = 0.005f;
    private const float slowestSpeed = 113;
    private const float fastestSpeed = 240;

    private bool movementEnter = false;

    private float lastUpperBodyRotationZ;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            serial.Open();
        }
        catch
        {

        }
        serial.ReadTimeout = 1;
        firstMotorObjectZPosition = motorObject.transform.localPosition.z;
        lastUpperBodyRotationZ = 0;
        upperBody.transform.localRotation = Quaternion.Euler(upperBody.transform.localRotation.eulerAngles.x, upperBody.transform.localRotation.eulerAngles.y, -50);
    }

    // Update is called once per frame
    void Update()
    {
        if (movementEnter == false)
            StartCoroutine(MovementUpdates());
    }

    bool isTurned()
    {
        var tankRotations = transform.rotation.eulerAngles;
        if(tankRotations.x > 100 || tankRotations.x < 80)
        {
            return false;
        }
        return true;
    }

    IEnumerator MovementUpdates()
    {
        movementEnter = true;
        try
        {

            buttonPressedForce = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
            leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            leftControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles;
            rightControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch).eulerAngles;
            leftThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
            leftThumbstickLeftRight = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
            rightThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
            rightThumbstickLeftRight = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

            if (!hasObject)
            {
                CloseClawAfterButtonForce();
            }

            MoveUpperBody();

            if (isTurned())
            {
                MoveLeftRight();
                MoveForwardBackwards();
            }

            if (serial.IsOpen)
            {
                try
                {
                    Debug.Log(serial.ReadByte());
                }
                catch (System.Exception)
                {
                }

                if (leftThumbstickUpDown > 0.1f)
                {
                    char sendSpeed = ((char)((int)(leftThumbstickUpDown * (fastestSpeed - slowestSpeed))));
                    serial.Write("F" + sendSpeed);
                }

                else if (leftThumbstickUpDown < -0.1f)
                {
                    char sendSpeed = ((char)((int)((-leftThumbstickUpDown) * (fastestSpeed - slowestSpeed))));
                    serial.Write("B" + sendSpeed);
                }

                else if (leftThumbstickLeftRight > 0.1f)
                {
                    char sendSpeed = ((char)((int)(leftThumbstickLeftRight * (fastestSpeed - slowestSpeed))));
                    serial.Write("R" + sendSpeed);
                }

                else if (leftThumbstickLeftRight < -0.1f)
                {
                    char sendSpeed = ((char)((int)((-leftThumbstickLeftRight) * (fastestSpeed - slowestSpeed))));
                    serial.Write("L" + sendSpeed);
                }
                else
                {
                    serial.Write("S");
                }
            }
            else
            {
                serial.Open();
            }
        }
        catch
        {

        }
        yield return new WaitForSecondsRealtime(0.001f);
        movementEnter = false;
    }

    void MoveLeftRight()
    {
        var wholeBodyAngles = rotationBody.transform.localRotation.eulerAngles;
        float rotateToValue = wholeBodyAngles.z - rotationSpeed * leftThumbstickLeftRight;
        rotationBody.transform.localRotation = Quaternion.Euler(wholeBodyAngles.x, wholeBodyAngles.y, rotateToValue);
    }

    void MoveForwardBackwards()
    {
        var wholeBodyPosition = movementBody.transform.localPosition;
        float moveToValue = wholeBodyPosition.y + movementSpeed * leftThumbstickUpDown;
        movementBody.transform.localPosition = new Vector3(wholeBodyPosition.x, moveToValue, wholeBodyPosition.z);
        this.transform.position = movementBody.transform.position;
        movementBody.transform.localPosition = new Vector3(0, 0, 0);
    }

    void CloseClawAfterButtonForce()
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

    void MoveUpperBody()
    {
        var upperBodyAngles = upperBody.transform.localRotation.eulerAngles;
        upperBody.transform.localRotation = Quaternion.Euler(upperBodyAngles.x, upperBodyAngles.y, getValueInBordersForUpperBody());
    }

    float getValueInBordersForUpperBody()
    {

        float rotateToValue = lastUpperBodyRotationZ + rotationSpeed * -rightThumbstickUpDown;
        //if (lastBottomBodyRotationZ + lastUpperBodyRotationZ < 0)
        {
            if (maximumBackwardsUpperBodyRotation < rotateToValue && rotateToValue < maximumForwardsUpperBodyRotation)
            {
                lastUpperBodyRotationZ = rotateToValue;
            }
        }
        return lastUpperBodyRotationZ;
    }
}