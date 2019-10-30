using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelCollisionDetection : MonoBehaviour
{
    //make a list to track collided objects
    List<Collider> collidedObjects = new List<Collider>();

    void FixedUpdate()
    {
        collidedObjects.Clear(); //clear the list of all tracked objects.
    }


    // if there is collision with an object in either Enter or Stay, add them to the list 
    // (you can check if it already exists in the list to avoid double entries, 
    // just in case, as well as the tag).
    void OnCollisionEnter(Collision col)
    {
        Debug.Log(true);
        if (!collidedObjects.Contains(col.collider) && col.collider.gameObject.layer == 9)
        {
            collidedObjects.Add(col.collider);
        }
    }

    void OnCollisionStay(Collision col)
    {
        OnCollisionEnter(col); //same as enter
    }

    void Update()
    {
        var numberOfColliders = collidedObjects.Count; // this should give you the number you need
        collidedObjects.Clear(); // You can also clear the list here
        //Debug.Log(numberOfColliders);
        if (numberOfColliders >= 1)
        {
            //RobotTank.wheelIsTouching = true;
        }
        else
        {
            //RobotTank.wheelIsTouching = false;
        }
    }
}
