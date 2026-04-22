using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletScript : MonoBehaviour
{
    public float speed = 25f;
    public float gravityMultiplier = 1f; // tweak if needed
    public float downwardTilt = 0.1f;

    public GameObject impactSmokePrefab; //My smoke endshoot effect
    [SerializeField] float offsetSmoke = 0.02f;

    private Rigidbody rb;
    private Collider bulletCol;

    public void Init(Vector3 dir, GameObject owner)
    {
        rb = GetComponent<Rigidbody>();
        bulletCol = GetComponent<Collider>();

        // ---- Rigidbody setup ----
        rb.useGravity = true;
        rb.freezeRotation = true;                 // IMPORTANT: no flipping / spinning
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Slight downward tilt if desired
        Vector3 fireDir = dir.normalized;
        fireDir.y -= downwardTilt;                         // SMALL downward bias
        fireDir.Normalize();

        rb.linearVelocity = fireDir * speed;

        // Optional: stronger gravity than default
        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

        // ---- Ignore player collisions ----
        IgnoreOwnerCollision(owner);
    }

    private void IgnoreOwnerCollision(GameObject owner)
    {
        if (owner == null || bulletCol == null) return;

        // Ignore CharacterController explicitly
        CharacterController cc = owner.GetComponent<CharacterController>();
        if (cc != null)
            Physics.IgnoreCollision(bulletCol, cc, true);

        // Ignore all other colliders on the player (children included)
        Collider[] ownerCols = owner.GetComponentsInChildren<Collider>(true);
        foreach (var col in ownerCols)
        {
            if (col != null)
                Physics.IgnoreCollision(bulletCol, col, true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider, collision.contacts[0].point, collision.contacts[0].normal);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only react to bear triggers
        oso bear = other.GetComponentInParent<oso>();
        if (bear == null)
            return; // Ignore iglus, zones, pickups, etc.

        Vector3 hitPoint = other.ClosestPoint(transform.position);

        HandleHit(other, hitPoint, Vector3.up);
    }


    private void HandleHit(Collider hit, Vector3 position, Vector3 normal)
    {
        //Is it a Bear?
        oso bear = hit.GetComponentInParent<oso>();
        if (bear != null) //Yes
        {
            bear.ApplyDamage(1); //Do damage
        }

        SpawnImpactEffect(position, normal);
        Destroy(gameObject); // Destroy my bullet
    }


    // Helper Method to spawn Smoke FX
    private void SpawnImpactEffect(Vector3 position, Vector3 normal)
    {
        if (impactSmokePrefab == null) return;

        position += normal * offsetSmoke; //Slight offset to avoid clipping

        Quaternion rot = Quaternion.LookRotation(normal);

        Instantiate(impactSmokePrefab, position, rot);
    }
    
}