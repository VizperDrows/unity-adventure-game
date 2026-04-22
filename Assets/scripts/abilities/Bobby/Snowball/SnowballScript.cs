using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class SnowballScript : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 18f;
    public float lifeTime = 0.5f;

    [Header("Slow")]
    public float slowAmount = 5f;
    public float slowDuration = 1f;

    private Vector3 direction;
    private bool initialized;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!initialized) return;

        transform.position += direction * speed * Time.deltaTime;
    }

    // Called from playermovement when spawned
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        initialized = true;
    }

    void OnTriggerEnter(Collider other)
    {

        // Make sure we hit a bear
        if (!other.CompareTag("enemy")) {
            return; 
        }

        oso bear = other.GetComponent<oso>();
        if (bear == null)
        {

            return;
        }

        bear.ApplySlow(slowAmount, slowDuration);

        Destroy(gameObject);
    }
}

