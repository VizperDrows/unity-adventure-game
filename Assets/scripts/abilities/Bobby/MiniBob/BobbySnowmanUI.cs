using UnityEngine;

//This script is attached to the UI button for spawning the mini snowman ability for
//Attached to MiniBobButton in Canvas/Abilities/Bobby

public class BobbySnowmanUI : MonoBehaviour
{
    public void SpawnSnowman()
    {
        if (playermovement.Instance != null)
            playermovement.Instance.SpawnMiniSnowman();
    }
}

