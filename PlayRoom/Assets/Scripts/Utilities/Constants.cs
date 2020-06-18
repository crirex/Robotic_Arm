using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    #region Boundaries
    public const int minimumSpeed = 150;
    public const int maximumSpeed = 150; // This can be put all the way to 255 only but the tests are going to be for 150 as it's speed is adequate and it would be okay for simulations that the speed is the same.

    public const float minimumClawRotation = -67.0f;
    public const float maximumClawRotation = 0f; //Rotations tested for right claw, left claw has oposite rotations

    public const float minimumClawXPosition = 0.027f;
    public const float maximumClawXPosition = 0.0f;

    public const float minimumClawZPosition = -0.04f;
    public const float maximumClawZPosition = -0.0165f;

    public const float minimumClawGrabberXPosition = 0.018f;
    public const float maximumClawGrabberXPosition = 0.0553f;
    
    public const float minimumXRotationInWhichTheTankCanMove = 80;
    public const float maximumXRotationInWhichTheTankCanMove = 100;

    public const float minimumMotorObjectZPosition = 0.22f;
    public const float maximumMotorObjectZPosition = 0.236f;

    public const float minimumUpperBodyRotation = -95.3f;
    public const float maximumUpperBodyRotation = 16f;

    public const float deadZone = 0.2f;
    public const float maximumRadius = 1f;
    #endregion

    #region Correctors
    public const float rotationRightSpeedCorrector = 0.0095f; // It seems rotation to the left and right are different, this time i don't think it's gravity tho... idk what it is.
    public const float rotationLeftSpeedCorrector = 0.01f; // It seems rotation to the left and right are different, this time i don't think it's gravity tho... idk what it is.
    public const float forwardSpeedCorrector = 0.00002724f; // This value is sync
    public const float armGoingDownSpeedCorrector = 0.0058f; // Because of gravity it moves up slower. //This value is sync
    public const float armGoingUpSpeedCorrector = 0.0056f; // This value is sync
    public const float clawSpeedCorrector = 0.0049f;
    public const float objectDistanceCorrector = 0.23f;
    #endregion

    #region Data Conversation
    public const int firstSafeAsciiCharacter = 33;
    public const int lastSafeAsciiCharacter = 126;
    public const int normalizationValueForSendData = 3;
    public const int bytesForFloat = 4;
    public const int bytesForDouble = 8;
    public const int bytesToReadFromRobot = 2 * bytesForFloat;
    public const float oneCM = 0.01917f;
    public const float maximumUtrasonicDistance = 500;

    public const int syncValue = 255;
    public const char positiveSign = 'P';
    public const char negativeSign = 'N';
    public const char movementOption = 'M';
    public const char armOption = 'A';
    public const char clawOption = 'C';
    #endregion
}
