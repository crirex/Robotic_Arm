using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class MainControl : MonoBehaviour
{
    #region Methods
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RobotTankActions.Instance.UpdateRobot();
    }
    #endregion
}