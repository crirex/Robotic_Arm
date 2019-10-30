using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArm : MonoBehaviour
{
    [SerializeField]
    private GameObject leftClaw;
    [SerializeField]
    private GameObject rightClaw;
    [SerializeField]
    private GameObject upDownClaw;
    [SerializeField]
    private GameObject upperBody;
    [SerializeField]
    private GameObject bottomBody;
    [SerializeField]
    private GameObject leftRightRotations;

    float buttonPressedForce;
    float leftThumbstickUpDown;
    float rightThumbstickUpDown;
    Vector3 leftControllerPosition; 
    Vector3 rightControllerPosition;
    Vector3 leftControllerRotation; 
    Vector3 rightControllerRotation;

    private const float maximumOutwardsClawRotation = 90.0f; //Rotations tested for right claw, left claw has oposite rotations
    private const float maximumInwardsClawRotation = -22.0f;
    private const float maximumUpwardsClawRotation = -60;
    private const float maximumDownwardsClawRotation = 125;
    private const float maximumBackwardsBottomBodyRotation = -55;
    private const float maximumForwardsBottomBodyRotation = 90;
    private const float maximumBackwardsUpperBodyRotation = -150;
    private const float maximumForwardsUpperBodyRotation = 3;
    private const float rotationSpeed = 1;

    private float lastClawRotationZ;
    private float lastUpperBodyRotationZ;
    private float lastBottomBodyRotationZ;
    // Start is called before the first frame update
    void Start()
    {
        lastClawRotationZ = upDownClaw.transform.localRotation.eulerAngles.z;
        lastUpperBodyRotationZ = -50;
        upperBody.transform.localRotation = Quaternion.Euler(upperBody.transform.localRotation.eulerAngles.x, upperBody.transform.localRotation.eulerAngles.y, -50);
        lastBottomBodyRotationZ = 45;
        bottomBody.transform.localRotation = Quaternion.Euler(bottomBody.transform.localRotation.eulerAngles.x, bottomBody.transform.localRotation.eulerAngles.y, 45);
    }

    // Update is called once per frame
    void Update()
    {
        buttonPressedForce = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
        leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        leftControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles;
        rightControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch).eulerAngles;
        leftThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        rightThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;


        CloseClawAfterButtonForce();
        MoveWristAfterWristRotation();
        MoveWholeArmLeftRight();
        MoveUpperBody();
        MoveBottomBody();

    }

    void CloseClawAfterButtonForce()
    {
        float resultingValue = (maximumOutwardsClawRotation - maximumInwardsClawRotation) * (1 - buttonPressedForce) + maximumInwardsClawRotation;
        var rightAngles = rightClaw.transform.localRotation.eulerAngles;
        var leftAngles = rightClaw.transform.localRotation.eulerAngles;
        rightClaw.transform.localRotation = Quaternion.Euler(rightAngles.x, resultingValue, rightAngles.z);
        leftClaw.transform.localRotation = Quaternion.Euler(leftAngles.x, -resultingValue, leftAngles.z);
    }

    void MoveWristAfterWristRotation()
    {
        var clawAngles = upDownClaw.transform.localRotation.eulerAngles;
        upDownClaw.transform.localRotation = Quaternion.Euler(clawAngles.x, clawAngles.y, getValueInBordersForClaw());
    }

    float getValueInBordersForClaw()
    {
        const float offset = 20; //For comodity while wielding
        float rotateToValue = 360 - rightControllerRotation.z;
        float difference = 360 + maximumUpwardsClawRotation;
        if ( rotateToValue > difference)
        {
            rotateToValue = -(360 - rotateToValue);
        }
        rotateToValue += offset;
        if ( maximumUpwardsClawRotation < rotateToValue && rotateToValue < maximumDownwardsClawRotation)
        {
            lastClawRotationZ = rotateToValue;
        }
        return lastClawRotationZ;
    }

    void MoveWholeArmLeftRight()
    {
        var armAngles = leftRightRotations.transform.localRotation.eulerAngles;
        leftRightRotations.transform.localRotation = Quaternion.Euler(armAngles.x, rightControllerRotation.y, armAngles.z);
    }

    void MoveUpperBody()
    {
        var upperBodyAngles = upperBody.transform.localRotation.eulerAngles;
        upperBody.transform.localRotation = Quaternion.Euler(upperBodyAngles.x, upperBodyAngles.y, getValueInBordersForUpperBody());
    }

    float getValueInBordersForUpperBody()
    {

        float rotateToValue = lastUpperBodyRotationZ + rotationSpeed * rightThumbstickUpDown;
        //if (lastBottomBodyRotationZ + lastUpperBodyRotationZ < 0)
        {
            if (maximumBackwardsUpperBodyRotation < rotateToValue && rotateToValue < maximumForwardsUpperBodyRotation)
            {
                lastUpperBodyRotationZ = rotateToValue;
            }
        }
        return lastUpperBodyRotationZ;
    }

    void MoveBottomBody()
    {
        var bottomBodyAngles = bottomBody.transform.localRotation.eulerAngles;
        bottomBody.transform.localRotation = Quaternion.Euler(bottomBodyAngles.x, bottomBodyAngles.y, getValueInBordersForBottomBody());
    }

    float getValueInBordersForBottomBody()
    {
        float rotateToValue = lastBottomBodyRotationZ + rotationSpeed * leftThumbstickUpDown;
        //if (lastBottomBodyRotationZ + lastUpperBodyRotationZ < 0)
        {
            if (maximumBackwardsBottomBodyRotation < rotateToValue && rotateToValue < maximumForwardsBottomBodyRotation)
            {
                lastBottomBodyRotationZ = rotateToValue;
            }
        }
        return lastBottomBodyRotationZ;
    }
}