using UnityEngine;

public class FrankTrapUI : MonoBehaviour
{
    public void PlaceTrap()
    {
        if (playermovement.Instance != null)
            playermovement.Instance.SpawnBearTrap();
    }
}
