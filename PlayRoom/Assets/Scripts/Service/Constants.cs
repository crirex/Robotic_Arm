using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    #region Boundaries
    public const int maximumSpeed = 150; // This can be put all the way to 255 only but the tests are going to be for 150 as it's speed is adequate and it would be okay for simulations that the speed is the same.
    public const int minimumSpeed = 150;

    public const float maximumOutwardsClawRotation = 0f; //Rotations tested for right claw, left claw has oposite rotations
    public const float maximumInwardsClawRotation = -32.8f;
    public const float OutwardsXPosition = 0.0654f;
    public const float InwardsRightClawXPosition = 0.0716f;
    public const float InwardsLeftClawXPosition = OutwardsXPosition - (InwardsRightClawXPosition - OutwardsXPosition);
    public const float OutwardsZPosition = 0.2178f;
    public const float InwardsRightClawZPosition = 0.2062f;
    public const float InwardsLeftClawZPosition = OutwardsZPosition - (OutwardsZPosition - InwardsRightClawZPosition);
    public const float maximumMotorObjectZPositionOffset = -0.01f;
    public const float maximumBackwardsUpperBodyRotation = -95.3f;
    public const float maximumForwardsUpperBodyRotation = 16f;
    public const float maximumXRotationInWhichTheTankCanMove = 100;
    public const float minimumXRotationInWhichTheTankCanMove = 80;

    public const float deadZone = 0.2f;
    public const float maximumRadius = 1f;
    #endregion

    #region Correctors
    public const float rotationRightSpeedCorrector = 0.0095f; // It seems rotation to the left and right are different, this time i don't think it's gravity tho... idk what it is.
    public const float rotationLeftSpeedCorrector = 0.01f; // It seems rotation to the left and right are different, this time i don't think it's gravity tho... idk what it is.
    public const float forwardSpeedCorrector = 0.00002724f; // This value is sync
    public const float armGoingDownSpeedCorrector = 0.0053f; // Because of gravity it moves up slower. //This value is sync
    public const float armGoingUpSpeedCorrector = 0.0051f; // This value is sync
    public const float clawSpeedCorrector = 0.0021f; // This value is sync
    public const float objectDistanceCorrector = 0.162f;
    #endregion

    #region Data Conversation
    public const int firstSafeAsciiCharacter = 33;
    public const int lastSafeAsciiCharacter = 126;
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
