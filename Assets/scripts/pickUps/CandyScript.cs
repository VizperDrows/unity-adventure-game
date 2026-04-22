using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyScript : MonoBehaviour
{
    public float speedIncrease;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playermovement player = other.gameObject.GetComponent<playermovement>();
            if(player != null)
            {
                player.speedBoost(speedIncrease);
                Destroy(gameObject);
            }
        }
    }
}
