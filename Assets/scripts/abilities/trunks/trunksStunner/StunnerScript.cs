using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


//script called in stunner hability prefab for trunks  2/25/2024
//trunksStunner prefab on assets > characters > trunks > abilities
public class StunnerScript : MonoBehaviour
{
    public Transform spriteChild;

    Rigidbody rb;
    Vector3 direction;

    public float stunDuration = 2f;

    public float lifeTime = 2f;
    public float speed = 10f;

    private Coroutine lifeRoutine;
    private bool alreadyHit = false; // <--- so the projectile hits only once

    private Collider col;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Set the correct rotation one time
        if (spriteChild != null)
            spriteChild.localRotation = Quaternion.Euler(45f, 90f, 0f);

        lifeRoutine = StartCoroutine(lifeCycle()); //This starts its life cycle
    }

    void FixedUpdate() {
        rb.MovePosition(direction * Time.fixedDeltaTime * speed + transform.position);
    }

    IEnumerator lifeCycle() {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    //character calls this function and sets direction
    public void setDirection(Vector3 dir) {
        this.direction = dir.normalized;

        // Flip the spriteChild based on Z axis (horizontal in your rotated world)
        if (spriteChild != null)
        {
            Vector3 scale = spriteChild.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.z < 0 ? -1 : 1); // flip if moving right
            spriteChild.localScale = scale;
        }
    }

    public Vector3 getDirection() {
        return this.direction;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyHit) return;   // <--- prevents double stun calls

        // Make sure we hit a bear
        if (!other.CompareTag("enemy")) return;

        oso bear = other.GetComponent<oso>();
        if (bear == null) return;


        alreadyHit = true;
        spriteChild.gameObject.SetActive(false); // disable sprite

        // Stop lifetime destroy so it doesn’t kill itself early
        if (lifeRoutine != null)
            StopCoroutine(lifeRoutine);

        // Disable collider so it can't hit twice
        col.enabled = false;

        // Start the stun coroutine (inside THIS script)
        StartCoroutine(StunBear(bear));

        
    }

    private IEnumerator StunBear( oso bear)
    {
        // Call the bear’s own stun logic
        bear.ApplyStun(stunDuration);

        speed = 0f; // Stop projectile movement

        // Stun duration
        yield return new WaitForSeconds(stunDuration);

        // Destroy this projectile after hitting
        Destroy(gameObject);
    }

}
