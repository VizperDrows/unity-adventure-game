using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceRigid : MonoBehaviour
{
    [SerializeField]
    private float fMagnitude;

    Rigidbody rB = null;
    Vector3 forceDir = new Vector3(0,0,0);

    ControllerColliderHit hite;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
        hite = hit;
        rB = hit.collider.attachedRigidbody;

    }

    void FixedUpdate()
    {
        if(rB != null)
        {
            forceDir = hite.gameObject.transform.position - transform.position;
            forceDir.x = 0;
            forceDir.z = 0;
            forceDir.Normalize();
            rB.AddForceAtPosition(forceDir * fMagnitude, transform.position, ForceMode.Impulse);
        }
        else
        {
            forceDir.x = 0;
            forceDir.z = 0;
            forceDir.y = 0;
        }
    }
}
