using UnityEngine;

//Script attached to the UI button for activating the speed boost ability for Rudolph
//Attached to SpeedButton in Canvas/Abilities/Rudolph

public class RudolphSpeedUI : MonoBehaviour
{
    public float speed = 12f;

    public void SpeedBoost()
    {
        if (playermovement.Instance != null)
            playermovement.Instance.speedBoost(speed);
    }
}
