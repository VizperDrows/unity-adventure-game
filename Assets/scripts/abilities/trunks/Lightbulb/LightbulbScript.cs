using UnityEngine;

public class LightbulbScript : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 18f;
    public float lifeTime = 1.5f;

    private Vector3 direction;
    private bool initialized = false;

    [Header("Visual")]
    private SpriteRenderer sprite;
    private Animator anim;

    public GameObject impactParticlePrefab;
    private Color bulbColor = Color.white;

    void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);

        // Pick random color animation at spawn
        PickRandomBulb();
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

        // Flip sprite based on LEFT / RIGHT only
        // You use Z as horizontal axis in your world
        if (sprite != null)
        {
            if (direction.z > 0.01f)
                sprite.flipX = true;     // facing right
            else if (direction.z < -0.01f)
                sprite.flipX = false;    // facing left
        }
    }

    void PickRandomBulb()
    {
        if (anim == null) return;

        int r = Random.Range(0, 3);

        // Assumes your Animator has these states as DEFAULT states
        switch (r)
        {
            case 0:
                anim.Play("blueBulb");
                bulbColor = Color.blue;
                break;
            case 1:
                anim.Play("redBulb");
                bulbColor = Color.red;
                break;
            case 2:
                anim.Play("yellowBulb");
                bulbColor = Color.yellow;
                break;
        }
    }

    // For now: no effect on bears yet
    private void OnTriggerEnter(Collider other)
    {
        // Later we will add behavior here
        // For now just destroy on hitting anything solid

        //Ignore other triggers (like pickup zones)
        if (other.isTrigger) return;

        // Check if it's a tree
        if (other.CompareTag("Tree"))
        {
            TreeBehavior tree = other.GetComponent<TreeBehavior>();

            if (tree != null)
            {
                tree.ConvertTree();
            }
        }

        SpawnImpactFX();

        Destroy(gameObject);
    }

    void SpawnImpactFX()
    {
        if (impactParticlePrefab == null) return;

        // Spawn at current position
        GameObject fx = Instantiate(impactParticlePrefab, transform.position, Quaternion.identity);

        // Tint particles to match bulb color
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = bulbColor;
        }
    }
}
