using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class RobotTank : MonoBehaviour
{
    public SerialPort serial = new SerialPort("COM8", 9600);

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
    private const float fastestSpeed = 160;
    private const float maximumRadius = 1;

    private float speedMultiplyer = 0;
    private float arithmeticMedian = 1;
    private float xNomalized = 0;
    private float yNomalized = 0;

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

            speedMultiplyer = (float)Math.Sqrt(Math.Pow(Math.Sin(Math.PI * leftThumbstickUpDown), 2) + Math.Pow(Math.Cos(Math.PI * leftThumbstickLeftRight), 2)) / maximumRadius;

            arithmeticMedian = Math.Abs(leftThumbstickUpDown) + Math.Abs(leftThumbstickLeftRight);
            if (arithmeticMedian != 0)
            {
                yNomalized = leftThumbstickUpDown / arithmeticMedian;
                xNomalized = leftThumbstickLeftRight / arithmeticMedian;

                if (yNomalized < 0)
                {
                    xNomalized *= -1;
                }
            }
            else
            {
                xNomalized = 0;
                yNomalized = 0;
            }

            if (!hasObject)
            {
                CloseClawAfterButtonForce();
            }

            MoveUpperBody();

            if (isTurned())
            {
                MoveLeftRight();
                MoveForwardBackwards();


                if (serial.IsOpen)
                {
                    try
                    {
                        Debug.Log("motor1SpeedSign:" + (char)serial.ReadByte());
                        Debug.Log("motor1Speed:" + (char)serial.ReadByte());
                        Debug.Log("motor1SpeedPart2:" + (char)serial.ReadByte());
                        Debug.Log("motor2SpeedSign:" + (char)serial.ReadByte());
                        Debug.Log("motor2Speed:" + (char)serial.ReadByte());
                        Debug.Log("motor2SpeedPart2:" + (char)serial.ReadByte());
                    }
                    catch (System.Exception)
                    {
                    }

                    float forwardSpeedValue = yNomalized * fastestSpeed;
                    float rotationSpeedValue = xNomalized * fastestSpeed;

                    if (forwardSpeedValue != 0 && rotationSpeedValue != 0)
                    {
                        float motor1Speed = ((forwardSpeedValue - rotationSpeedValue) / 2) * speedMultiplyer; // R part 1
                        float motor1SpeedPart2 = motor1Speed; // R part 2

                        float motor2Speed = (-(forwardSpeedValue + rotationSpeedValue) / 2) * speedMultiplyer; // L part 1
                        float motor2SpeedPart2 = motor2Speed; // L part 2

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

                        char motor1SpeedSend = ((char)((int)(motor1Speed))); // R part 1
                        char motor1SpeedPart2Send = ((char)((int)(motor1SpeedPart2))); // R part 2
                        
                        char motor2SpeedSend = ((char)((int)(motor2Speed))); // L part 1
                        char motor2SpeedPart2Send = ((char)((int)(motor2SpeedPart2))); // L part 2
                        
                        string completeSend = "M" + motor1SpeedSign + motor1SpeedSend + motor1SpeedPart2Send + 
                            motor2SpeedSign + motor2SpeedSend + motor2SpeedPart2Send;
                        Debug.Log(completeSend);
                        serial.Write(completeSend);
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
            else
            {
                serial.Write("S");
            }
        }
        catch
        {

        }
        yield return new WaitForSecondsRealtime(0.0000f);
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