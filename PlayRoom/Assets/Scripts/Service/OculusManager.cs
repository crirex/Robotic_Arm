using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusManager: Singleton<OculusManager>
{
    #region Public proprieties
    public float RightHandTrigger { get; private set; }
    public float LeftHandTrigger { get; private set; }
    public float RightIndexTrigger { get; private set; }
    public float LeftIndexTrigger { get; private set; }
    public float RightThumbstickUpDown { get; private set; }
    public float LeftThumbstickUpDown { get; private set; }
    public float RightThumbstickLeftRight { get; private set; }
    public float LeftThumbstickLeftRight { get; private set; }
    public bool RightThumbstickPressed { get; private set; }
    public bool LeftThumbstickPressed { get; private set; }
    public bool RightAPressed { get; private set; } //1-A
    public bool LeftXPressed { get; private set; } // 3-X
    public bool RightBPressed { get; private set; } // 2-B
    public bool LeftYPressed { get; private set; } // 4-Y
    public bool RightStartPressed { get; private set; } // 2-B
    public bool LeftBackPressed { get; private set; } // 4-Y
    public Vector3 LeftControllerPosition { get; private set; }
    public Vector3 RightControllerPosition { get; private set; }
    public Vector3 LeftControllerRotation { get; private set; }
    public Vector3 RightControllerRotation { get; private set; } 
    #endregion

    #region Constructors
    private OculusManager()
    {
    }
    #endregion

    #region Public methods
    public void UpdateValues()
    {
        RightHandTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
        LeftHandTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);

        RightIndexTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        LeftIndexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);

        RightThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
        LeftThumbstickUpDown = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

        RightThumbstickLeftRight = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;
        LeftThumbstickLeftRight = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;

        RightThumbstickPressed = OVRInput.Get(OVRInput.Button.SecondaryThumbstick);
        LeftThumbstickPressed = OVRInput.Get(OVRInput.Button.PrimaryThumbstick);

        RightAPressed = OVRInput.Get(OVRInput.Button.One);
        LeftXPressed = OVRInput.Get(OVRInput.Button.Three);

        RightBPressed = OVRInput.Get(OVRInput.Button.Two);
        LeftYPressed = OVRInput.Get(OVRInput.Button.Four);

        RightStartPressed = OVRInput.Get(OVRInput.Button.Start);
        LeftBackPressed = OVRInput.Get(OVRInput.Button.Back);

        LeftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        RightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

        LeftControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles;
        RightControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch).eulerAngles;
    }
    #endregion
}
