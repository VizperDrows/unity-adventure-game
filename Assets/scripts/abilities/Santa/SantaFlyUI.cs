using UnityEngine;

public class SantaFlyUI : MonoBehaviour
{
    public void FlyUp()
    {
        if (playermovement.Instance != null)
            playermovement.Instance.OnFlyUpPressed();
    }

    public void FlyDown()
    {
        if (playermovement.Instance != null)
            playermovement.Instance.OnFlyDownPressed();
    }

    public void FlyRelease()
    {
        if (playermovement.Instance != null)
            playermovement.Instance.OnFlyReleased();
    }
}
