using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using System;  // this lib is to access Math class

//*NOTE: The water mode functions are with the other general functions below everything

[RequireComponent(typeof(CharacterController))]
public class playermovement : MonoBehaviour
{
    public static playermovement Instance;

    [Header("Animation")]
    public Animator animator;
    public RuntimeAnimatorController anim1;
    public RuntimeAnimatorController anim2;
    public RuntimeAnimatorController anim3;
    public RuntimeAnimatorController anim4;
    public RuntimeAnimatorController anim5;
    public RuntimeAnimatorController anim6;
    private Vector3 scalet;

    //Movement
    [Header("Movement")]
    public float speed;
    public float jumpspeed = 5f;
    public FloatingJoystick joystick; // //reference to the floating joystick script that controls movement
    [SerializeField] private Vector3 move;
    private float horizontal = 0f;
    private float vertical = 0f;
    private bool ch = false;
    private bool cv = false;
    private int charselec = 1;
    private CharacterController characterController;

    //for gravity and jumping
    [Header("Gravity & Jumping")]
    private float yspeed = 0f;
    private bool hasGravity = false;
    private bool hadGravity = false;
    private bool wasGrounded;
    [SerializeField] private float time = 0f;
    public float timeDelayYReset;
    public float yspeedReset = -.1f;
    //

    //for water
    [Header("Lakes")]
    bool inWater = false;
    public float waterGrav = -1.5f;
    public float uWatVelMaxP = -2f;
    public float watDensity;
    //

    //buoyancy
    [Header("Buoyancy")]
    public float buoyancyK = 8f;         // spring constant (restoring strength)
    public float buoyancyDamping = 6f;   // damping (removes oscillation)
    public float maxFloatSpeed = 2f;     // cap upward speed
    public float maxSinkSpeed = -3f;     // cap downward sink speed
    public float settleSpeedThreshold = 0.05f; // below this we consider vertical motion "settled"
    public float settleDisplacementThreshold = 0.03f; // distance to surface considered "settled"
    public float bobAmplitude = 0.02f;   // tiny bobbing amplitude when settled
    public float bobSpeed = 1.2f;        // bob frequency
    [HideInInspector] public float waterSurfaceY = 0f; // assigned by WaterModeTrig


    //for pickUps
    [Header("Pick-Ups")]
    public short pickUpCount = 0;
    //



    //when attacked
    [Header("Attacked State")]
    [SerializeField] private Vector3 attackdir = new Vector3(0, 0, 0);
    bool attacked = false;
    bool resetAtk = false;
    public byte life = 1;
    public bool dead = false;
    public Canvas canvas;

    //UI
    [Header("UI")]
    PanelScript finalPanel;
    GameObject fJoy; // gameobject of character movement, dont confuse with script
    GameObject pauseButton;
    bool isPause = false;
    float leftRdelimiter = .6f;
    [SerializeField]
    float rightRdelimiter = .87f;
    [SerializeField] float bottomDelimiter = 0.25f; // bottom 25% blocked


    //for abilities
    [Header("Abilities")]
    bool isTrunks = false;
    bool isRudolph = false;
    bool isElf = false;
    bool isSanta = false;
    bool isBobby = false;
    bool isFrank = false;

    //RUDOLPH abilities
    GameObject speedButton;

    //TRUNKS abilities
    GameObject stunButton;    // Reference to Abilities/Trunks/StunButton gameObject button
    JoyStunTemporary stunJoystick;
    [SerializeField] private float trunksSpawnOffset = 1;
    [SerializeField] private float trunksSpawnVertical = .5f;
    public GameObject trunksStunner;
    [Header("Trunks Lightbulb")]
    GameObject lightbulbButton; // Reference to Abilities/Trunks/LightBulbButton gameObject button
    public GameObject lightbulbPrefab;
    public float bulbSpawnOffset = 1.2f;
    public float bulbSpawnHeight = 0.3f;
    public float bulbDownTilt = 0.05f;   // small downward arc if you want


    //SANTA abilities
    [Header("Find Gift Ability")]
    public GameObject findGiftButton;
    public GameObject searchTrailPrefab;
    public float closeDistance = 29f;
    public float mediumDistance = 50f;

    // ----------------- SANTA FLYING -----------------
    [Header("Santa Flying")]
    public GameObject flyUpButton;
    public GameObject flyDownButton;

    private bool isFlying = false;
    private float flyingInput = 0f; // -1 down, 0 hover, +1 up
    [SerializeField] private float maxFlyHeight = 220f;
    [SerializeField] private float flyUpSpeed = 6f;
    [SerializeField] private float flyDownSpeed = -5f;
    [SerializeField] private float flyHoverSpeed = 0f;

    [Header("Bubble Comment")]
    public GameObject bubbleUI;
    public TMPro.TextMeshProUGUI textUI;
    private Vector3 textOriginalScale;


    // BOBBY abilities
    // ----------------- BOBBY MELT -----------------
    [Header("Bobby Melt")]
    [SerializeField] private bool isMelted = false;
    [SerializeField] private string bobbyPostProcessName = "waterPPupHead";
    [SerializeField] private PostProcessVolume postProcessVolume;
    public float showGOTime = 4f;

    [Header("Bobby Snowball")]
    public GameObject snowballPrefab;
    [SerializeField] private float snowballSpawnOffset = 1.2f;
    [SerializeField] private float snowballSpawnHeight = 0.8f;
    [SerializeField] private float snowballDownTilt = 0.15f; // small downward component
    private GameObject snowballButton;
    private JoySnowballTemporary snowballJoystick;

    [Header("Bobby Decoy")]
    public GameObject miniSnowmanPrefab;
    public float snowmanSpawnOffset = 1.5f;
    [SerializeField] float snowmanSpawnYOffset = 0.05f;
    [SerializeField] float verticalRayOrigin = 5f;
    private GameObject miniBobbyButton;

    //FRANK abilities
    [Header("Frank Gun")]
    public GameObject bulletPrefab;
    [SerializeField] private float bulletSpawnOffset = 1.2f;
    [SerializeField] private float bulletVerticalOffset = 0.6f;
    [SerializeField] private bool isShooting = false;
    private Vector2 shootInput = Vector2.zero;
    private GameObject gunButton;

    [Header("Frank Bear Trap")]
    public GameObject bearTrapPrefab;
    [SerializeField] private float trapSpawnOffset = 0.5f;
    [SerializeField] private float trapHeightOffset = -0.2f;
    private GameObject bearTrapButton;

    private Queue<GameObject> activeTraps = new Queue<GameObject>();
    [SerializeField] private int maxTraps = 3;
    public bool getIsFrank() => isFrank;

    // Start is called before the first frame update
    void Start()
    {

        //to set frameRate to specific different devices
        int otherDeviceFrameRate = (int)Math.Round(Screen.currentResolution.refreshRateRatio.value);
        Application.targetFrameRate = otherDeviceFrameRate;
        //


        Time.timeScale = 1f;

        scalet = transform.localScale;
        textOriginalScale = textUI.transform.localScale; // for santa's bubble comment scale

        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in scene!");
            return; // stop execution if canvas missing
        }
        finalPanel = canvas.GetComponentInChildren<PanelScript>(true);
        if (finalPanel == null) Debug.LogWarning("PanelScript not found in Canvas children!");

        //reference to the floating joystick script that controls movement
        // |                                           |
        // v                                           v
        joystick = canvas.GetComponentInChildren<FloatingJoystick>(true);
        if (canvas.transform.childCount > 1) fJoy = canvas.transform.GetChild(1).gameObject; //joy that controlls character movement. Left input
        if (canvas.transform.childCount > 2) pauseButton = canvas.transform.GetChild(2).gameObject;

        //para abilities
        // Trunks ability buttons
        //Aqui se asigna el boton joystick de Trunks
        stunButton = canvas.transform.Find("Abilities/Trunks/StunButton")?.gameObject; //StunButton gameObject
        lightbulbButton = canvas.transform.Find("Abilities/Trunks/LightBulbButton")?.gameObject; //LightbulbButton gameObject

        // Rudolph ability buttons
        //boton para speed ability de Rudolph
        speedButton = canvas.transform.Find("Abilities/Rudolph/SpeedButton")?.gameObject;

        // Santa ability buttons
        flyUpButton = canvas.transform.Find("Abilities/Santa/FlyUp")?.gameObject;
        flyDownButton = canvas.transform.Find("Abilities/Santa/FlyDown")?.gameObject;
        findGiftButton = canvas.transform.Find("Abilities/Santa/FindGift")?.gameObject;

        //Bobby ability buttons
        snowballButton = canvas.transform.Find("Abilities/Bobby/SnowballButton")?.gameObject;
        miniBobbyButton = canvas.transform.Find("Abilities/Bobby/MiniBobButton")?.gameObject;

        //Frank ability buttons
        gunButton = canvas.transform.Find("Abilities/Frank/GunButton")?.gameObject;
        bearTrapButton = canvas.transform.Find("Abilities/Frank/BearTrapButton")?.gameObject;



        //para seleccionar personaje
        speed = PlayerPrefs.GetFloat("speed");
        if (speed == 0)
            speed = 9;  // default
        charselec = PlayerPrefs.GetInt("CharacterSelected");

        Animator anim = GetComponent<Animator>();
        if (anim == null) Debug.LogError("Animator missing on Player prefab!");

        //characters selection settings
        if (charselec == 1) //bobby
        {
            anim.runtimeAnimatorController = anim1 as RuntimeAnimatorController;
            isBobby = true;

            CachePostProcess();               // ensure reference for water postProcess
            if (postProcessVolume != null)
                postProcessVolume.enabled = false;

            if (snowballButton != null)  //activate snowball button
            {
                snowballButton.SetActive(true);
                snowballJoystick = snowballButton.GetComponent<JoySnowballTemporary>();
            }

            //activate mini snowman button
            if (miniBobbyButton != null) miniBobbyButton.SetActive(true);
            

        } else if(charselec == 2) //santa
        {
            anim.runtimeAnimatorController = anim2 as RuntimeAnimatorController;
            isSanta = true;

            if (flyUpButton != null) flyUpButton.SetActive(true);
            if (flyDownButton != null) flyDownButton.SetActive(true);
            
            if (findGiftButton != null)
            {
                findGiftButton.SetActive(true);
                findGiftButton.GetComponent<Button>().onClick.AddListener(() => SpawnSearchTrail());
            }           

        } else if(charselec == 3) //elf
        {
            anim.runtimeAnimatorController = anim3 as RuntimeAnimatorController;
            isElf = true;
        } else if (charselec == 4) //frank
        {
            anim.runtimeAnimatorController = anim4 as RuntimeAnimatorController;
            isFrank = true;

            ApplyBearFrenzy();
            if (gunButton != null) gunButton.SetActive(true);
            if (bearTrapButton != null) bearTrapButton.SetActive(true);

        } else if (charselec == 5) //rudolph
        {
            anim.runtimeAnimatorController = anim5 as RuntimeAnimatorController;
            isRudolph = true;
            speedButton?.SetActive(true);
        } else if (charselec == 6) //trunks
        {
            anim.runtimeAnimatorController = anim6 as RuntimeAnimatorController;
            isTrunks = true;
            if (stunButton != null) {
                stunButton.SetActive(true);
                stunJoystick = stunButton.GetComponent<JoyStunTemporary>();
            }

            if (lightbulbButton != null)
            {
                lightbulbButton.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Unknown character selection!");
        }

    }

    void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        isPause = false;
        Instance = this;
        characterController = this.GetComponent<CharacterController>();
        wasGrounded = characterController.isGrounded;

    }

    // Update is called once per frame
    void Update()
    {
        if (!dead && !isPause)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            if (joystick != null)
            {
                if (joystick.Horizontal > .9) horizontal = 1;
                else if (joystick.Horizontal < -.9) horizontal = -1;
                else horizontal = joystick.Horizontal;
                if (joystick.Vertical > .9) vertical = 1;
                else if (joystick.Vertical < -.9) vertical = -1;
                else vertical = joystick.Vertical;
            }
        }
        else if(dead)//case I am dead
        {
            horizontal = 0;
            vertical = 0;
            if (fJoy != null) fJoy.SetActive(false); //disabling joystick when dead
        }



        //FLIPPING SPRITE BASED ON DIRECTION
        UpdateFlipSprite();

        //to apply movement
        if(!attacked)
        {
            move = new Vector3(vertical, yspeed, -horizontal);//key statement referenced on FixedUpdate()
        } 
        else
        {
            move = new Vector3(attackdir.x, yspeed, attackdir.z);
        }

        //jumping and gravity
        if (characterController.isGrounded&&!inWater) //ground state
        {
            hasGravity = false;
            if (hadGravity)
            {
                if (time < timeDelayYReset)
                {
                    time = time + 1f * Time.deltaTime;
                }
                else
                {
                    yspeed = yspeedReset;
                    time = 0f;
                    hadGravity = false;
                }
            }

            if (!isFlying)
            {
                //jumping
                if (Input.GetButtonDown("Jump") && !dead && !isPause)//for keyboard approach, doesnt work if mobile
                {
                    yspeed = jumpspeed;
                }

                //Screen delimiters for right input (jumping & abilities)
                for (int i = 0; i < Input.touchCount && !dead && !isPause; i++) { //for mobile approach
                    Touch touch = Input.GetTouch(i);
                    float xPos = touch.position.x, yPos = touch.position.y;

                    bool pauseZone = yPos > Screen.height * 0.8f;
                    bool bottomZone = yPos < Screen.height * bottomDelimiter;

                    //Screen delimiters for Trunks & Rudolph characters
                    if (isTrunks || isRudolph) {
                        bool rightAbilityZone =
                            xPos > Screen.width * leftRdelimiter &&
                            xPos < Screen.width * rightRdelimiter;

                        if (rightAbilityZone && !pauseZone) yspeed = jumpspeed;
                    }
                    //Screen delimiters for Santa, bobby & Frank
                    else if (isSanta || isBobby || isFrank)
                    {
                        bool rightSafeZone =
                            xPos > Screen.width * leftRdelimiter &&
                            xPos < Screen.width * rightRdelimiter;

                        if (rightSafeZone && !pauseZone && !bottomZone) yspeed = jumpspeed;
                    }
                    //Screen delimiters for default
                    else if (xPos > (float)(Screen.width * leftRdelimiter) && !pauseZone) yspeed = jumpspeed;
                }
                //
            }

            if (resetAtk)
            {
                attacked = false;
                resetAtk = false;
            }
        } else if (!inWater) // jump state
        {
            hasGravity = true;
            hadGravity = true;
            if (attacked)
            {
                resetAtk = true;
            }
        }
        //when entering Water
        else
        {
            if (isFlying) StopFlying();

            if (!isFlying)
            {
                if (Input.GetButtonDown("Jump") && !dead&&!isPause)
                {
                    yspeed = jumpspeed;
                }

                for (int i = 0; i < Input.touchCount && !dead; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    float xPos = touch.position.x, yPos = touch.position.y;
                    bool pauseZone = false;
                    if (yPos > (float)(Screen.height * .8)) pauseZone = true;
                    if (xPos > (float)(Screen.width * leftRdelimiter) && !pauseZone) yspeed = jumpspeed;
                }
            }
            
        }


        // -----------------------------------------------
        // ANIMATION
        // -----------------------------------------------
        if (isFlying)
        {
            // disable ALL ground-based animations
            ch = false;
            cv = false;
            animator.SetBool("isFlying", true);
            animator.SetFloat("blend", 0);
            animator.SetBool("isjumping", false);
        }
        else
        {
            // walking
            ch = horizontal != 0f;
            cv = vertical != 0f;

            // jumping
            animator.SetBool("isjumping", yspeed > 0f);

            // blend for walk
            animator.SetFloat("blend", (ch || cv) ? 1 : 0);

            animator.SetBool("isFlying", false);
        }

        if (life <= 0)
        {
            dead = true;
            life = 1;
            ShowGameOver();
        }
        
        //Finishing condition if Gifts are higher than 25
        if(pickUpCount >= 25)
        {
            dead = true;
            pickUpCount = 0;
            pauseButton.SetActive(false);
            finalPanel.enable();
            finalPanel.setTextColor(true);
        }
    }

    void FixedUpdate()
    {
        //BOBBY MELTED when touching water
        if (isMelted)
        {
            characterController.Move(Vector3.zero);
            return;
        }

        // FLYING OVERRIDE (no gravity) for Santa
        if (isFlying && !inWater)
        {
            // Ceiling clamp
            if (transform.position.y >= maxFlyHeight && flyingInput > 0f)
            {
                flyingInput = 0f;
            }

            // vertical speed
            if (flyingInput > 0f) yspeed = flyUpSpeed;
            else if (flyingInput < 0f) yspeed = flyDownSpeed;
            else yspeed = flyHoverSpeed;

            move = new Vector3(vertical, yspeed, -horizontal);

            // horizontal uses speed, vertical does not
            Vector3 flyMove = new Vector3(vertical * speed, yspeed, -horizontal * speed);

            CollisionFlags flags = characterController.Move(flyMove * Time.deltaTime);

            // Landing check
            if ((flags & CollisionFlags.Below) != 0 && yspeed <= 0f)
            {
                StopFlying();
            }

            return; // skip gravity
        }

        // NORMAL PHYSICS BELOW
        if (hasGravity&&!inWater) 
        {
            yspeed += Physics.gravity.y * Time.deltaTime;//applying gravity
        }
        else if (inWater)
        {
            if (isFlying) StopFlying();

            // If trunks, apply damped-spring buoyancy toward surface
            if (isTrunks)
            {
                // ---- FLOATING LOGIC ----
                // displacement: positive when below surface (we want upward restoring force)
                float displacement = waterSurfaceY - transform.position.y;

                // Calculate spring-based acceleration: F = k * displacement - c * velocity
                // Treat yspeed as velocity (positive up)
                float accel = buoyancyK * displacement - buoyancyDamping * yspeed;

                // integrate accel into vertical speed
                yspeed += accel * Time.deltaTime;

                // clamp rise/sink speeds
                if (yspeed > maxFloatSpeed) yspeed = maxFloatSpeed;
                if (yspeed < maxSinkSpeed) yspeed = maxSinkSpeed;

                // if near equilibrium (small displacement and small velocity) zero vertical speed and do tiny bobbing
                if (Mathf.Abs(displacement) < settleDisplacementThreshold && Mathf.Abs(yspeed) < settleSpeedThreshold)
                {
                    // Snap yspeed to zero to stop oscillation
                    yspeed = 0f;

                    // Optional tiny procedural bobbing so Trunks looks alive on water
                    float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
                    // apply a very small offset to yspeed to create bobbing effect (or you could directly offset position)
                    yspeed = bob;
                }
            }
            else
            {
                // ---- NORMAL WATER SINKING FOR OTHER CHARACTERS ----
                if (yspeed > uWatVelMaxP) yspeed += waterGrav * Time.deltaTime;
                else yspeed += (uWatVelMaxP - yspeed) * Time.deltaTime * watDensity;
            }
        }

        characterController.Move(move*Time.deltaTime*speed);//apply movement
    }
    // END FixedUpdate


    // ----------------------------------------------------------
    // ANIMATION EVENTS
    // ----------------------------------------------------------
    private void UpdateFlipSprite()
    {
        // NORMAL FLIP (DISABLED WHILE FRANK IS SHOOTING)
        if (!(isFrank && isShooting))
        {
            if (horizontal < 0f)
            {
                transform.localScale = new Vector3(-scalet.x, scalet.y, scalet.z);
                if (isSanta)
                {
                    textUI.transform.localScale = new Vector3(-textOriginalScale.x, textOriginalScale.y, textOriginalScale.z);
                }
            }
            else if (horizontal > 0f)
            {
                transform.localScale = scalet;
                if (isSanta)
                {
                    textUI.transform.localScale = textOriginalScale;
                }
            }
        }

    }









    //for abilities

    //TRUNKS abilities
    // instantiate projectile prefab that is on assets > characters > trunks > abilities
    public void spawnStunner(Vector2 joyDir) {
        if (joyDir.sqrMagnitude < 0.01f)
            return; // No direction, don't spawn

        // Convert joystick direction to world space
        Vector3 worldDir = new Vector3(joyDir.y, trunksSpawnVertical, -joyDir.x);
        worldDir.Normalize();

        // Calculate spawn position in front of player based on joystick direction
        Vector3 spawnPos = transform.position + (worldDir * trunksSpawnOffset);

        // DO NOT rotate the parent — keep Quaternion.identity
        GameObject proj = Instantiate(trunksStunner, spawnPos, Quaternion.identity);

        // Set movement direction
        proj.GetComponent<StunnerScript>().setDirection(worldDir);
    }

    // Spawn lightbulb for TRUNKS
    public void SpawnLightbulb(Vector2 joyDir)
    {
        if (!isTrunks || dead || isPause) return;
        if (lightbulbPrefab == null) return;

        if (joyDir.sqrMagnitude < 0.01f)
            return; // no direction

        // Same mapping style as snowball
        Vector3 worldDir = new Vector3(joyDir.y, -bulbDownTilt, -joyDir.x).normalized;

        // Spawn slightly in front and above ground
        Vector3 spawnPos = transform.position
                         + Vector3.up * bulbSpawnHeight
                         + worldDir * bulbSpawnOffset;

        GameObject proj = Instantiate(lightbulbPrefab, spawnPos, Quaternion.identity);

        LightbulbScript lb = proj.GetComponent<LightbulbScript>();
        if (lb != null)
            lb.SetDirection(worldDir);
    }

    //

    //ELF abilities
    public bool getIsElf() { return isElf; }



    //SANTA abilities

    public bool getIsSanta() { return isSanta; }

    //Santa BUBBLE-COMMENT feature
    public void ShowBubble(string message, float duration)
    {

        bubbleUI.SetActive(true);
        textUI.text = message;

        StartCoroutine(HideBubbleAfterTime(duration));
    }

    IEnumerator HideBubbleAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        bubbleUI.SetActive(false);
    }

    //

    //Santa ability to SPAWN-TRAIL that points towards the nearest gift,
    //with different animation states based on distance
    public void SpawnSearchTrail()
    {
        // Find all active GiftScript objects
        GiftScript[] gifts = FindObjectsOfType<GiftScript>();
        if (gifts.Length == 0) return;

        // Find the closest gift
        GiftScript nearest = gifts[0];
        float minDist = Vector3.Distance(transform.position, nearest.transform.position);

        foreach (GiftScript g in gifts)
        {
            float dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist < minDist)
            {
                nearest = g;
                minDist = dist;
            }
        }

        // Determine direction
        Vector3 dir = (nearest.transform.position - transform.position).normalized;

        // Determine animation state based on distance
        string animState;
        if (minDist <= closeDistance)
            animState = "Hot";
        else if (minDist <= mediumDistance)
            animState = "Warm";
        else
            animState = "Cold";

        // Instantiate the trail
        GameObject trail = Instantiate(searchTrailPrefab, transform.position, Quaternion.identity);
        trail.GetComponent<SearchTrail>().Init(dir, animState);
    }
    //


    // ----------------------------------------------------------
    //                  FLYING SYSTEM  SANTA
    // ----------------------------------------------------------
    public void StartFlying()
    {
        if (dead || isPause) return;
        if (inWater) return;                       // cannot fly in water
        if (!isSanta) return;

        isFlying = true;
        hasGravity = false;
        hadGravity = false;

        flyingInput = 0f; // start hovering
        yspeed = 0f;

        animator.SetBool("isFlying", true);
    }

    private void EnsureCharacterController()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }


    public void OnFlyUpPressed()
    {

        EnsureCharacterController();

        if (dead || isPause) {
            Debug.Log("1");
            return;
        } 
        if (inWater) {
            Debug.Log("2");
            return;
        }
        if (!isSanta) {
            Debug.Log("3 Santa: "+isSanta);
            return; 
        }

        if (!isFlying) StartFlying();
        flyingInput = 1f;
        Debug.Log($"flyingInput={flyingInput} isFlying={isFlying} grounded={characterController.isGrounded}");

    }

    public void TestEvent()
    {
        Debug.Log("EVENT TRIGGER FIRED");
    }


    public void OnFlyDownPressed()
    {
        if (!isFlying) return;
        flyingInput = -1f;
    }

    public void OnFlyReleased()
    {
        flyingInput = 0f; // hover
    }


    public void FlyDown()
    {
        if (!isFlying) return;                     // only works if currently flying
        flyingInput = -1f;
    }

    // Called when touching ground
    private void StopFlying()
    {
        isFlying = false;
        flyingInput = 0f;
        hasGravity = true;
        animator.SetBool("isFlying", false);
    }

    //

    //FOR BOBBY ABILITIES
    private void CachePostProcess()
    {
        if (postProcessVolume != null) return;

        GameObject go = GameObject.Find(bobbyPostProcessName);
        if (go == null)
        {
            Debug.LogWarning($"Post-process GameObject '{bobbyPostProcessName}' not found");
            return;
        }

        postProcessVolume = go.GetComponent<PostProcessVolume>();
        if (postProcessVolume == null)
            Debug.LogWarning($"'{bobbyPostProcessName}' has no PostProcessVolume component");
    }

    public void spawnSnowball(Vector2 joyDir)
    {
        if (!isBobby) return;
        if (snowballPrefab == null) return;

        if (joyDir.sqrMagnitude < 0.01f)
            return; // no direction

        // Map joystick -> world direction (matching your stunner mapping)
        // x = forward/back, z = left/right (your code uses -horizontal on z)
        Vector3 worldDir = new Vector3(joyDir.y, -snowballDownTilt, -joyDir.x).normalized;

        // Spawn slightly above ground so it doesn't immediately clip
        Vector3 spawnPos = transform.position + Vector3.up * snowballSpawnHeight + worldDir * snowballSpawnOffset;

        GameObject proj = Instantiate(snowballPrefab, spawnPos, Quaternion.identity);

        SnowballScript sb = proj.GetComponent<SnowballScript>();
        if (sb != null)
            sb.SetDirection(worldDir);
    }

    public void SpawnMiniSnowman()
    {
        if (!isBobby || dead || isPause) return;

        // Start slightly above player to raycast down safely
        Vector3 rayOrigin = transform.position
                            + transform.forward * snowmanSpawnOffset
                            + Vector3.up * verticalRayOrigin;

        Vector3 spawnPos = rayOrigin;

        // Raycast down to find ground
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 5f))
        {
            spawnPos.y = hit.point.y;
        }

        // Small lift so it doesn't clip into ground
        spawnPos.y += snowmanSpawnYOffset;

        // Spawn with fixed rotation
        Instantiate(miniSnowmanPrefab, spawnPos, Quaternion.identity);
    }

    // BOBBY MELT when he touches water, called from WaterModeTrig
    private void TriggerMelt()
    {
        if (isMelted) return;

        isMelted = true;
        dead = true;

        // Stop all movement
        speed = 0f;
        yspeed = 0f;
        flyingInput = 0f;
        isFlying = false;
        hasGravity = false;

        // Animator
        animator.ResetTrigger("melt");
        animator.SetTrigger("melt");

        // Disable joystick & abilities
        if (fJoy != null) fJoy.SetActive(false);
        if (flyUpButton != null) flyUpButton.SetActive(false);
        if (flyDownButton != null) flyDownButton.SetActive(false);
        if (findGiftButton != null) findGiftButton.SetActive(false);

        // Disable Bobby water post-process
        CachePostProcess();
        if (postProcessVolume != null)
            postProcessVolume.enabled = false;

        // Optional: delay game over to let animation play
        Invoke(nameof(ShowGameOver), showGOTime);
    }
    //

    //

    //FOR FRANK ABILITIES
    //  **There is a getter for Frank on top of the script
    private void ApplyBearFrenzy()
    {
        oso[] bears = FindObjectsOfType<oso>();

        foreach (oso bear in bears)
        {
            bear.EnableFrenzy();
        }
    }

    //------------------------------------------------
    // Spawn bullet for Frank
    //------------------------------------------------
    public void spawnBullet(Vector2 joyDir)
    {
        if (!isFrank || dead || isPause) return;
        if (bulletPrefab == null) return;
        if (joyDir.sqrMagnitude < 0.001f) return;

        // Convert joystick to world direction (same convention you already use)
        Vector3 worldDir = new Vector3(joyDir.y, 0f, -joyDir.x).normalized;

        // 2) Decide which SIDE the muzzle is on (LEFT/RIGHT only)
        // If player is aiming mostly vertical, keep current facing based on scale
        if (Mathf.Abs(joyDir.x) > 0.05f)
            UpdateFrankFacingFromShoot(joyDir); // your existing flip logic

        bool facingLeft = transform.localScale.x < 0f;

        // IMPORTANT: In your coordinate system, "right" is -Z, "left" is +Z
        Vector3 sideDir = facingLeft ? Vector3.forward : Vector3.back; // +Z or -Z

        // 3) Spawn from the side muzzle, NOT from aim direction
        Vector3 spawnPos =
            transform.position +
            Vector3.up * bulletVerticalOffset +
            sideDir * bulletSpawnOffset;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        BulletScript bs = bullet.GetComponent<BulletScript>();
        if (bs != null)
            bs.Init(worldDir, gameObject);
    }

    //------------------------------------------------
    // Hanlde animation shooting state for Frank
    //------------------------------------------------
    public void StartShooting(Vector2 input)
    {
        if (!isFrank || dead || isPause) return;

        isShooting = true;
        shootInput = input;

        animator.SetBool("isShooting", true);

        UpdateFrankFacingFromShoot(input);
    }

    public void StopShooting()
    {
        if (!isFrank) return;

        isShooting = false;
        shootInput = Vector2.zero;

        animator.SetBool("isShooting", false);
    }

    private void UpdateFrankFacingFromShoot(Vector2 input)
    {
        if (!isFrank) return;
        if (Mathf.Abs(input.x) < 0.01f) return; // ignore near-zero

        Vector3 scale = transform.localScale;

        if (input.x < 0f)
            scale.x = -Mathf.Abs(scale.x);   // face left
        else
            scale.x = Mathf.Abs(scale.x);    // face right

        transform.localScale = scale;
    }

    // ------------------------------------------------
    // SPAWN BEAR TRAP FOR FRANK
    // ------------------------------------------------
    public void SpawnBearTrap()
    {
        if (!isFrank || dead || isPause) return;
        if (bearTrapPrefab == null) return;

        // Spawn slightly in front of Frank, on ground
        Vector3 forwardPos = transform.position + transform.forward * trapSpawnOffset;

        // Raycast down to ground (important for slopes)
        if (Physics.Raycast(forwardPos + Vector3.up * 3f, Vector3.down, out RaycastHit hit, 10f))
        {
            forwardPos = hit.point;

            //Manual height adjustment
            forwardPos.y += trapHeightOffset;

            // Instantiate first at hit point
            GameObject trap = Instantiate(bearTrapPrefab, forwardPos, Quaternion.identity);

            // Register trap
            activeTraps.Enqueue(trap);

            // Enforce max traps = 3
            if (activeTraps.Count > maxTraps)
            {
                GameObject oldest = activeTraps.Dequeue();
                if (oldest != null)
                    Destroy(oldest);
            }
        }
    }

    //


    //



    //for UI, (pause function is in PauseScript)
    public void setIsPause(bool val) {
        this.isPause = val;
    }
    public bool getIsPause() {
        return isPause;
    }
    //


    //for water
    public void setWaterMode(bool water)
    {
        inWater = water;

        // Bobby melts instantly when touching water
        if (water && isBobby && !isMelted)
        {
            TriggerMelt();
        }
    }

    public float getSpeed()
    {
        return speed;
    }
    
    public void setSpeed(float sp)
    {
        speed = sp;
    }

    


    //for bear attacks
    public void setYSpeed(float yspeed)
    {
        this.yspeed = yspeed;
    }

    public void setAttackDir(float x, float z)
    {
        this.attackdir.x = x;
        this.attackdir.z = z;
    }

    public void setAttacked(bool attacknew)
    {
        this.attacked = attacknew;
    }
    //

    //for PICK UPS
    public void OnGiftCollected(float amount) {
        if (isTrunks) {
            //Trunks ignores speed
            return;
        }

        increasePickUps(amount);
    }

    //This tracks the Gifts I have so far to trigger finish game
    public void increasePickUps(float speedIncrease)
    {
        pickUpCount++;
        StartCoroutine(speedTimer(speedIncrease));
    }

    public void speedBoost(float speedIncrease)
    {
        StartCoroutine(speedTimer(speedIncrease));
    }

    IEnumerator speedTimer(float speedIncrease)
    {
        speed += speedIncrease;
        yield return new WaitForSeconds((float)3);
        speed -= speedIncrease;
    }
    //

    public Vector3 getDir() {
        return move;
    }

    //for Game Over
    private void ShowGameOver()
    {
        finalPanel.enable();
        finalPanel.setTextColor(false);
    }

    //
}
