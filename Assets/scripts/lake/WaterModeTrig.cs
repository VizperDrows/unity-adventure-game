using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterModeTrig : MonoBehaviour
{
    playermovement player = null;
    float playSpeed;

    // offset if the trigger is not exactly at surface level
    public float surfaceOffset = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject.GetComponent<playermovement>();
            other.gameObject.GetComponent<ForceRigid>().enabled = false;
            if(player != null)
            {
                playSpeed = player.getSpeed();
                player.setSpeed(playSpeed/2);
                player.setWaterMode(true);

                // Tell the player where the water surface is (use this object's Y + optional offset)
                player.waterSurfaceY = transform.position.y + surfaceOffset;
                
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if(other.gameObject.GetComponent<ForceRigid>().enabled == false) other.gameObject.GetComponent<ForceRigid>().enabled = true;
            if (player != null)
            {
                player.setSpeed(playSpeed);
                player.setWaterMode(false);
            }
        }
    }
}
