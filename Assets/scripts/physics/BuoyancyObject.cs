using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class BuoyancyObject : MonoBehaviour
{
    public Transform[] floaters;

    public float underWaterDrag = 3f;
    public float underWaterAngularDrag = 1f;
    public float airDrag = 0f;
    public float airAngularDrag = 0.05f;
    public float floatingPower = 15f;
    public float waterHeight = 0f;

    Rigidbody myRigidbody;
    int floatersUnderWater;
    bool underWater;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        floatersUnderWater = 0;
        for(int i = 0; i < floaters.Length; i++)
        {
            float diff = floaters[i].position.y - waterHeight;
            if (diff < 0)
            {
                myRigidbody.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(diff), floaters[i].position, ForceMode.Force);
                floatersUnderWater++;
                if (!underWater)
                {
                    underWater = true;
                    SwitchState(true);
                }
            }
        }
        
        if (underWater&&floatersUnderWater==0)
        {
            underWater = false;
            SwitchState(false);
        }

    }

    void SwitchState(bool isUnderWater)
    {
        if (isUnderWater)
        {
            myRigidbody.linearDamping = underWaterDrag;
            myRigidbody.angularDamping = underWaterAngularDrag;
        }
        else
        {
            myRigidbody.linearDamping = airDrag;
            myRigidbody.angularDamping = airAngularDrag;
        }
    }
}
