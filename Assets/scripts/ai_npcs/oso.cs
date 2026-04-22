using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class oso : MonoBehaviour
{
    public enum BearState
    {
        Patrol,
        Chase,
        Attack,
        Stunned
    }

    public BearState currentState = BearState.Patrol;

    public NavMeshAgent agent;
    private GameObject player=null;
    public LayerMask whatisplayer;
    private playermovement pcont=null;

    [Header("Frenzy Mode (Frank)")]
    [SerializeField] private RuntimeAnimatorController defaultController;
    [SerializeField] private RuntimeAnimatorController frenzyOverrideController;
    [SerializeField] private float frenzySpeedMultiplier = 2f;
    private bool frenzyApplied = false;


    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Stunned")]
    [HideInInspector] public bool isStunned = false;
    float stunTimer = 0f;
    float stunDuration = 0f;

    [Header("Trap Stun")]
    private bool permanentlyTrapped = false;
    private BearTrapScript activeTrap = null;

    [Header("Slow")]
    private bool isSlowed = false;
    private float slowTimer = 0f;
    private float slowDuration = 0f;
    private float slowAmount = 0f;
    private float originalSpeed;
    [SerializeField] private Color slowTintColor = new Color(0.6f, 0.8f, 1f); // icy blue
    private Color originalSpriteColor;

    [Header("Death")]
    [SerializeField] private bool isDead = false;


    //ranges
    [Header("Ranges")]
    public float sightrange, attackrange;
    public bool playerinsightrange = false, playerinattackrange = false;
    //

    //patrolling
    [Header("Patrol")]
    public float walkpointrange;
    public Vector3 walkpoint;
    bool walkpointset = false;
    float time = 0f;
    public float timedelay;
    public Vector3 initpos;
    bool setPosition = true;
    bool patrolling = true;
    int walkableMask;
    //

    //chasing
    [Header("Chase")]
    [SerializeField] private string decoyTag = "decoy";
    [SerializeField] private float targetRefreshRate = 0.15f; // seconds
    private float targetRefreshTimer = 0f;
    private Transform currentTarget;

    //for attacking
    [Header("Attack")]
    public float difMult = 1000;
    public float attackspeed;
    float atktime = 10;
    bool attack = true;

    

    //variables para anim
    [Header("Animation")]
    public Vector3 lastpos;
    public Vector3 dif;
    public SpriteRenderer sprite;
    public Animator anim;
    [Tooltip("Percent BEFORE stun ends when backward anim should begin")]
    public float stunBackwardsLeadPercent = 0.25f;

    bool stunBackwardsTriggered = false;
    //

    //for audio
    [Header("Audio")]
    [SerializeField] AudioSource src;
    public AudioClip roarClip;
    //

    // debug & protections
    [Header("DEBUG")]
    public bool debugMode = false;
    public float arriveThreshold = 1.0f;
    public float stuckTimeout = 1.0f; // seconds of low movement before we consider stuck

    float stuckTimer = 0f;
    Vector3 lastAgentPosition;

    private void Start()
    {
        currentHealth = maxHealth;


        //referencia para navmeshagent y player
        agent = GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed;

        //for audio
        src = GameObject.Find("gameSound").GetComponent<AudioSource>();

        //para anim
        lastpos = transform.position;
        sprite = GetComponentInChildren<SpriteRenderer>();
        originalSpriteColor = sprite.color;
        //

        // safety defaults
        agent.autoBraking = true; // you can toggle this if you prefer continuous movement
        lastAgentPosition = transform.position;

        // Apply frenzy if Frank is active
        if (playermovement.Instance != null && playermovement.Instance.getIsFrank())
        {
            EnableFrenzy();
        }
    }

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        defaultController = anim.runtimeAnimatorController;
        int walkIndex = NavMesh.GetAreaFromName("Walkable");
        walkableMask = 1 << walkIndex;
    }

    // Update is called once per frame
    void Update()
    {
        // If permanently trapped, do nothing except animation
        if (permanentlyTrapped || isDead)
        {
            HandleAnimations();   // keep idle / stun visuals alive
            return;
        }

        //if (isDead) return;

        //Update slow logic
        HandleSlow();

        //to get initial position
        if (setPosition && transform.position != Vector3.zero) { //get initial position this way because transform.position doesn't work in start
            initpos = transform.position;
            setPosition = false;
            if (debugMode) Debug.Log($"{name} initpos set {initpos}");
        }

        //get player ref
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player != null)
                pcont = player.GetComponent<playermovement>(); //reference to player script

        }

        // Refresh target periodically (avoids heavy Find calls every frame)
        targetRefreshTimer -= Time.deltaTime;
        if (targetRefreshTimer <= 0f)
        {
            targetRefreshTimer = targetRefreshRate;
            RefreshTarget();
        }

        // Safety: if target got destroyed between refreshes
        if (currentTarget == null)
        {
            // If we are chasing/attacking and target is gone, fall back immediately
            if (currentState == BearState.Chase || currentState == BearState.Attack)
                ChangeState(BearState.Patrol);
        }

        //to get change in position and to get direction

        //asignar rango de vision y ataque
        Collider[] hits = Physics.OverlapSphere(transform.position, sightrange, whatisplayer);
        playerinsightrange = hits.Length > 0;
        playerinattackrange = Physics.CheckSphere(transform.position, attackrange, whatisplayer);
        //

        // debug detection
        if (debugMode && playerinsightrange)
        {
            Debug.DrawLine(transform.position, player.transform.position, Color.yellow);
        }

        HandleAnimations();
        MonitorAgentStuck();

        // MAIN STATE FLOW
        switch (currentState)
        {
            case BearState.Patrol: PatrolLogic(); break;
            case BearState.Chase: ChaseLogic(); break;
            case BearState.Attack: AttackLogic(); break;
            case BearState.Stunned: StunnedLogic(); break;
        }

    }


    // ---------------------------------------------------------
    // STATE LOGIC FUNCTIONS
    // ---------------------------------------------------------
    void PatrolLogic()
    {
        // immediate transitions
        // If we have any valid target, start chasing
        if (currentTarget != null)
        {
            ChangeState(BearState.Chase);
            return;
        }

        // find patrol point
        if (!walkpointset)
        {
            FindAndSetPatrolPoint();
        }

        // arrival test using agent info (more reliable)
        if (walkpointset)
        {
            if (!agent.pathPending)
            {
                float remaining = agent.remainingDistance;
                // debug
                if (debugMode) Debug.Log($"{name} remainingDistance={remaining} hasPath={agent.hasPath} pathPending={agent.pathPending} velocity.magnitude={agent.velocity.magnitude}");

                if (agent.hasPath && remaining <= arriveThreshold)
                {
                    // reached
                    if (debugMode) Debug.Log($"{name} reached walkpoint {walkpoint}");
                    // wait
                    if (time <= timedelay) time += Time.deltaTime;
                    else { walkpointset = false; time = 0f; }
                }
                else if (!agent.hasPath)
                {
                    // agent has no path -> regenerate
                    if (debugMode) Debug.Log($"{name} agent has no path, forcing new wp");
                    walkpointset = false;
                }
            }
        }
    }

    // tries n times to find a valid navmesh patrol point and set agent destination immediately
    bool FindAndSetPatrolPoint(int tries = 10, float sampleRadius = 2f)
    {
        for (int i = 0; i < tries; i++)
        {
            float randomZ = Random.Range(-walkpointrange, walkpointrange);
            float randomX = Random.Range(-walkpointrange, walkpointrange);
            Vector3 candidate = new Vector3(initpos.x + randomX, initpos.y, initpos.z + randomZ);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, sampleRadius, walkableMask))
            {
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(hit.position, path);

                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    walkpoint = hit.position;
                    agent.SetDestination(walkpoint);
                    walkpointset = true;
                    if (debugMode) Debug.Log($"{name} patrol wp set -> {walkpoint}");
                    return true;
                }
                else
                {
                    if (debugMode) Debug.Log($"{name} candidate invalid (no complete path): {candidate}");
                }
            }
        }

        if (debugMode) Debug.LogWarning($"{name} couldn't find a valid patrol point after tries.");
        return false;
    }


    void ChaseLogic()
    {
        if (currentTarget == null)
        {
            ChangeState(BearState.Patrol);
            return;
        }

        // If target is player AND in attack range -> Attack
        if (currentTarget == player?.transform)
        {
            bool inAttack = Physics.CheckSphere(transform.position, attackrange, whatisplayer);
            if (inAttack)
            {
                ChangeState(BearState.Attack);
                return;
            }
        }

        agent.isStopped = false;

        // Project target to navmesh
        NavMeshHit hit;
        Vector3 targetPos = currentTarget.position;

        bool found = NavMesh.SamplePosition(targetPos, out hit, 6f, NavMesh.AllAreas);

        if (!found)
        {
            if (debugMode)
                Debug.Log("[oso] Player is off-navmesh (maybe on a platform). Chase paused.");

            return;
        }

        Vector3 navMeshTarget = hit.position;

        // 2 Debug (only when debugMode)
        if (debugMode)
        {
            Debug.Log("[oso] ChaseLogic(): trying SetDestination to " + navMeshTarget);
        }

        bool ok = agent.SetDestination(navMeshTarget);

        if (!ok)
        {
            if (debugMode)
                Debug.LogWarning("[oso] SetDestination FAILED (Unity returned false). Will retry next frame.");
            return;
        }

        if (debugMode)
        {
            Debug.Log("[oso] ChaseLogic(): SetDestination returned: " + ok);
            Debug.Log("[oso] ChaseLogic(): agent.destination = " + agent.destination);
            Debug.Log("[oso] CHASE DEBUG -- isStopped=" + agent.isStopped +
                      ", speed=" + agent.speed +
                      ", vel=" + agent.velocity +
                      ", hasPath=" + agent.hasPath +
                      ", pathStatus=" + agent.pathStatus);
        }

        if (debugMode && agent.hasPath && agent.velocity.sqrMagnitude < 0.01f )
        {
            Debug.Log("[oso] Warning: agent has path but is barely moving.");
        }
    }

    void AttackLogic()
    {
        // If a decoy exists, prefer chasing it instead of attacking
        if (currentTarget != null && currentTarget != player?.transform)
        {
            ChangeState(BearState.Chase);
            return;
        }

        if (!playerinattackrange) { ChangeState(BearState.Chase); return; }

        if (attack && pcont != null && !pcont.dead)
        {
            // Hit the player
            pcont.setYSpeed(pcont.jumpspeed);
            dif = player.transform.position - transform.position;
            pcont.setAttackDir(dif.x * difMult, dif.z * difMult);
            pcont.setAttacked(true);
            pcont.life--;

            // Roar
            src?.PlayOneShot(roarClip);

            atktime = 0f;

            if (debugMode) Debug.Log($"{name} attacked player");
        }

        // attack cooldown
        if (atktime <= attackspeed) { attack = false; atktime += Time.deltaTime; }
        else { attack = true; }
    }

    void StunnedLogic()
    {
        // PERMANENT TRAP STUN do nothing, never release automatically
        if (permanentlyTrapped)
        {
            return;
        }

        // NORMAL TEMPORARY STUN (Trunks)
        stunTimer += Time.deltaTime;
        if (debugMode) Debug.Log($"{name} stunned: {stunTimer:F2}/{stunDuration:F2}");

        // 1) Trigger backwards animation when we reach near the end
        float triggerTime = stunDuration * (1f - stunBackwardsLeadPercent);

        if (!stunBackwardsTriggered && stunTimer >= triggerTime)
        {
            stunBackwardsTriggered = true;
            anim.SetTrigger("startStunEnd");   // play backwards animation
        }

        if (stunTimer >= stunDuration) { ChangeState(BearState.Patrol); }
    }


    // ---------------------------------------------------------
    // STATE MACHINE CONTROL
    // ---------------------------------------------------------
    public void ChangeState(BearState newState)
    {
        if (isDead) return;

        if (debugMode) Debug.Log($"{name} State {currentState} -> {newState}");
        ExitState(currentState);
        currentState = newState;
        EnterState(newState);
    }

    void EnterState(BearState state)
    {
        switch (state)
        {
            case BearState.Patrol:
                patrolling = true;
                agent.isStopped = false;
                // clear any old path and pick a new patrol destination immediately
                walkpointset = false;
                agent.ResetPath();
                FindAndSetPatrolPoint();
                break;

            case BearState.Chase:
                agent.isStopped = false;
                agent.ResetPath();

                // NEW: immediately set the chase destination once
                if (player != null)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(player.transform.position, out hit, 6f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                        if (debugMode)
                            Debug.Log("[oso] ENTER CHASE: immediate SetDestination -> " + hit.position);
                    }
                }

                if (debugMode)
                {
                    Debug.Log("[oso] ENTER CHASE -- speed=" + agent.speed +
                              ", accel=" + agent.acceleration +
                              ", isStopped=" + agent.isStopped +
                              ", vel=" + agent.velocity);
                }
                break;

            case BearState.Attack:
                agent.isStopped = true;
                agent.ResetPath();
                break;

            case BearState.Stunned:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                stunTimer = 0f;
                stunBackwardsTriggered = false;
                anim.ResetTrigger("startStunEnd");
                anim.SetBool("stunned", true);   // start forward animation
                break;
        }
    }
    void ExitState(BearState state)
    {
        switch (state)
        {
            case BearState.Chase:
                // clear chase path and make sure a patrol point will be found on enter
                walkpointset = false;
                agent.ResetPath();
                break;

            case BearState.Stunned:
                isStunned = false;
                agent.isStopped = false;
                stunBackwardsTriggered = false;
                anim.SetBool("stunned", false);  // leave stunned animation
                break;
        }
    }

    // ---------------------------------------------------------
    // STUN PUBLIC FUNCTION (CALLED BY STUNNER SCRIPT)
    // This is the first function stun that happens first before the other stun related functions
    // ---------------------------------------------------------
    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunDuration = duration;
        stunTimer = 0f;
        ChangeState(BearState.Stunned);
        if (debugMode) Debug.Log($"{name} ApplyStun {duration}");
    }

    


    // ---------------------------------------------------------
    // SLOW PUBLIC FUNCTION (CALLED BY SnowballScript)
    // ---------------------------------------------------------
    public void ApplySlow(float amount, float duration)
    {
        // If already slowed, refresh duration instead of stacking
        isSlowed = true;
        slowAmount = amount;
        slowDuration = duration;
        slowTimer = 0f;

        agent.speed = Mathf.Max(0.5f, originalSpeed - slowAmount);

        // Apply visual tint
        if (sprite != null)
            sprite.color = slowTintColor;
    }

    void HandleSlow()
    {
        if (!isSlowed) return;

        slowTimer += Time.deltaTime;

        if (slowTimer >= slowDuration)
        {
            isSlowed = false;
            slowTimer = 0f;
            agent.speed = originalSpeed;

            // Restore sprite color
            if (sprite != null)
                sprite.color = originalSpriteColor;
        }
    }

    // ---------------------------------------------------------
    // DECOY FUNCTIONALITY - find closest decoy, helper method for MiniBobby ability
    // ---------------------------------------------------------
    Transform FindClosestDecoy(float maxRange)
    {
        GameObject[] decoys = GameObject.FindGameObjectsWithTag(decoyTag);
        if (decoys.Length == 0) return null;

        Transform closest = null;
        float minSqr = float.MaxValue;
        Vector3 pos = transform.position;

        float maxSqr = maxRange * maxRange;

        foreach (GameObject d in decoys)
        {
            if (d == null) continue;

            float sqr = (d.transform.position - pos).sqrMagnitude;
            if (sqr < minSqr && sqr <= maxSqr)
            {
                minSqr = sqr;
                closest = d.transform;
            }
        }

        return closest;
    }

    void RefreshTarget()
    {
        if (debugMode)
        {
            string t = currentTarget == null ? "NONE" : currentTarget.name;
            Debug.Log($"[oso] Target = {t}");
        }

        // 1) Prefer decoy (if within range)
        Transform decoy = FindClosestDecoy(sightrange);
        if (decoy != null)
        {
            currentTarget = decoy;
            return;
        }

        // 2) Fallback to player
        if (player != null && pcont != null && !pcont.dead)
        {
            float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
            if (sqrDist <= sightrange * sightrange)
            {
                currentTarget = player.transform;
                return;
            }
        }

        // 3) Nothing to chase
        currentTarget = null;
    }

    // ---------------------------------------------------------
    // FRENZY MODE FUNCTIONS (FOR FRANK ABILITY)
    // ---------------------------------------------------------
    public void EnableFrenzy()
    {
        if (frenzyApplied) return;

        frenzyApplied = true;

        // Animator override
        if (anim != null && frenzyOverrideController != null)
            anim.runtimeAnimatorController = frenzyOverrideController;

        // Speed boost
        agent.speed = originalSpeed * frenzySpeedMultiplier;

        if (debugMode)
            Debug.Log($"{name} Frenzy ENABLED");
    }

    public void DisableFrenzy()
    {
        if (!frenzyApplied) return;

        frenzyApplied = false;

        // Restore animator
        if (anim != null && defaultController != null)
            anim.runtimeAnimatorController = defaultController;

        // Restore speed
        agent.speed = originalSpeed;

        if (debugMode)
            Debug.Log($"{name} Frenzy DISABLED");
    }


    // ---------------------------------------------------------
    // PERMANENT TRAP STUN (Frank ability)
    // ---------------------------------------------------------
    public void EnterPermanentTrapStun(BearTrapScript trap)
    {
        if (isDead) return;

        permanentlyTrapped = true;
        activeTrap = trap;

        // Stop everything
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        // Force state
        currentState = BearState.Stunned;

        // Animator
        stunTimer = 0f;
        stunDuration = Mathf.Infinity;   // never auto-release
        stunBackwardsTriggered = false;

        anim.ResetTrigger("startStunEnd");
        anim.SetBool("stunned", true);

        if (debugMode)
            Debug.Log($"{name} PERMANENTLY TRAPPED");
    }

    public void ExitPermanentTrapStun(BearTrapScript trap)
    {
        // Only release if this is the same trap
        if (!permanentlyTrapped) return;
        if (activeTrap != trap) return;

        permanentlyTrapped = false;
        activeTrap = null;

        // Resume AI
        isStunned = false;

        agent.isStopped = false;
        agent.ResetPath();

        anim.SetBool("stunned", false);
        anim.SetTrigger("startStunEnd");

        ChangeState(BearState.Patrol);

        if (debugMode)
            Debug.Log($"{name} RELEASED FROM TRAP");
    }
    //END permanent trap stun functions


    // ---------------------------------------------------------
    // DEATH FUNCTIONALITY
    // ---------------------------------------------------------
    public void Die()
    {
        // Release trap if trapped
        if (permanentlyTrapped && activeTrap != null)
        {
            activeTrap.ForceDestroyTrap();   // tell trap to clean itself
            activeTrap = null;
            permanentlyTrapped = false;
        }

        if (isDead) return;

        isDead = true;

        // Stop AI completely
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        // Disable collisions if needed
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // Trigger animation
        anim.ResetTrigger("killed");
        anim.SetTrigger("killed");

    }

    // Apply damage to the bear
    public void ApplyDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (debugMode)
            Debug.Log($"{name} took damage. HP = {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }




    // ---------------------------------------------------------
    // ANIMATION MOVEMENT
    // ---------------------------------------------------------
    void HandleAnimations()
    {
        dif = transform.position - lastpos;

        if (dif.z > 0.01f)
        {
            anim.SetFloat("blend", 1);
            sprite.flipX = true;
        }
        else if (dif.z < -0.01f)
        {
            anim.SetFloat("blend", 1);
            sprite.flipX = false;
        }
        else
        {
            anim.SetFloat("blend", 0);
        }

        lastpos = transform.position;
    }

    void MonitorAgentStuck()
    {
        // simple stuck detection based on agent movement
        float moved = (transform.position - lastAgentPosition).magnitude;
        if (moved < 0.01f && (agent.hasPath || agent.pathPending))
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }

        if (stuckTimer > stuckTimeout)
        {
            if (debugMode) Debug.LogWarning($"{name} STUCK detected. hasPath={agent.hasPath}, pathPending={agent.pathPending}, remaining={agent.remainingDistance}, velocity={agent.velocity.magnitude}");
            // force regenerate a patrol point if on patrol, or force re-path to player if chasing
            stuckTimer = 0f;

            if (currentState == BearState.Patrol)
            {
                walkpointset = false;
                agent.ResetPath();
                FindAndSetPatrolPoint();
            }
            else if (currentState == BearState.Chase && player != null)
            {
                agent.ResetPath();
                // project player onto navmesh and attempt a nav-target (same logic as ChaseLogic)
                NavMeshHit hit;
                if (NavMesh.SamplePosition(player.transform.position, out hit, 6f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    // fallback: try to set destination directly to player's xz at our y
                    Vector3 fallback = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                    agent.SetDestination(fallback);
                }
            }
        }

        lastAgentPosition = transform.position;
    }

    private void OnDrawGizmos()
    {

    }
}
