using UnityEngine;

public class SearchTrail : MonoBehaviour
{
    public float speed = 12f;        // How fast the trail moves
    public float lifetime = 1f;      // How many seconds before it disappears

    private Vector3 direction;
    private float timer;
    private Animator anim;

    // HARD-CODED rotation (never changes)
    private static readonly Quaternion LOCKED_ROTATION =
        Quaternion.Euler(45f, 90f, 0f);

    private Vector3 baseScale;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        // Force correct rotation immediately
        transform.rotation = LOCKED_ROTATION;

        // Cache scale ONLY (scale is safe)
        baseScale = transform.localScale;
    }

    // Called right after spawning to configure the trail
    public void Init(Vector3 dir, string animState)
    {
        direction = dir.normalized;

        // --- HARD LOCK ROTATION ---
        transform.rotation = LOCKED_ROTATION;

        // --- FLIP USING Z AXIS (your world orientation) ---
        Vector3 scale = baseScale;
        scale.x = Mathf.Abs(scale.x) * (direction.z > 0 ? -1 : 1);
        transform.localScale = scale;

        // Play chosen animation state
        if (anim != null)
            anim.Play(animState, 0, 0f);
    }

    void Update()
    {
        // Move straight toward the target direction
        transform.position += direction * speed * Time.deltaTime;

        // Count down lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

