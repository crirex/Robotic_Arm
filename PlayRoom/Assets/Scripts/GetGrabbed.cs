using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGrabbed : MonoBehaviour
{
    [SerializeField]
    private GameObject claw;

    Rigidbody thisRigidBody;
    // Start is called before the first frame update
    void Start()
    {
        thisRigidBody = gameObject.GetComponent<Rigidbody>();
        thisRigidBody.isKinematic = false;
        gameObject.transform.parent = null;
        gameObject.layer = 9;
    }
    //make a list to track collided objects
    List<Collider> collidedObjects = new List<Collider>();

    void FixedUpdate()
    {
        collidedObjects.Clear(); //clear the list of all tracked objects.
    }

    void OnTriggerEnter(Collider col)
    {
        if (!collidedObjects.Contains(col) && col.gameObject.name == "ClawCollision")
        {
            collidedObjects.Add(col);
        }
    }

    void OnTriggerStay(Collider col)
    {
        OnTriggerEnter(col); //same as enter
    }

    void Update()
    {
        var numberOfColliders = collidedObjects.Count; // this should give you the number you need
        collidedObjects.Clear(); // You can also clear the list here
        //if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        if (numberOfColliders >= 3)
        {
            thisRigidBody.isKinematic = true;
            gameObject.transform.parent = claw.transform;
            Debug.Log(gameObject.transform.parent);
            gameObject.layer = 11;
            RobotTank.hasObject = true;
        }
        if(!OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        {
            thisRigidBody.isKinematic = false;
            gameObject.transform.parent = null;
            gameObject.layer = 9;
            RobotTank.hasObject = false;
        }
    }
}