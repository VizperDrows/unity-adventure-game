using UnityEngine;

public class BearTrapScript : MonoBehaviour
{
    private oso trappedBear;
    private SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trap bears
        oso bear = other.GetComponentInParent<oso>();
        if (bear == null) return;

        // If already trapping someone, ignore
        if (trappedBear != null) return;

        trappedBear = bear;

        // Hide the trap visually
        if (sprite != null) sprite.enabled = false;

        // Apply PERMANENT stun
        bear.EnterPermanentTrapStun(this);

        // Optional: snap bear to trap position slightly
        Vector3 pos = bear.transform.position;
        pos.x = transform.position.x;
        pos.z = transform.position.z;
        bear.transform.position = pos;
    }

    public void ForceDestroyTrap()
    {
        // Optional: play break animation / sound here

        Destroy(gameObject);
    }


    private void OnDestroy()
    {
        // If this trap is destroyed and a bear was trapped, release it
        if (trappedBear != null)
        {
            trappedBear.ExitPermanentTrapStun(this);
        }
    }
}

