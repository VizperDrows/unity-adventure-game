using UnityEngine;
using System.Collections;

public class SantaController : MonoBehaviour
{
    public static SantaController Instance;

    public Transform player;
    public float followHeight = 4f;
    public float followSpeed = 6f;
    public float sideOffset = 2f;
    public float descendTime = 1.5f;
    public float bubbleDisplayTime = 6f;

    public GameObject bubbleUI;
    public TMPro.TextMeshProUGUI textUI;

    private bool active = false; //controls movement behavior
    private bool isRunning = false; //event lock to prevent multiple triggers


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        gameObject.SetActive(false);
        bubbleUI.SetActive(false);
    }

    public void SetPlayer(Transform pTrans)
    {
        player = pTrans;
    }

    public void TriggerSanta(int stage)
    {
        if (isRunning) return; // prevent multiple triggers

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        playermovement playerScript = player.GetComponent<playermovement>();

        // CASE: Player IS Santa -> no announcer
        // Other method when playing SANTA
        if (playerScript != null && playerScript.getIsSanta())
        {
            ShowPlayerBubble(stage);
            return; // skip coroutine completely
        }

        gameObject.SetActive(true);
        StartCoroutine(SantaSequence(stage));
    }

    IEnumerator SantaSequence(int stage)
    {
        isRunning = true;
        active = false; //disable update during descend

        // Spawn above player
        transform.position = player.position + Vector3.up * 10f + player.right * sideOffset;

        // FLOAT DOWN
        float timer = 0f;

        while (timer < descendTime)
        {
            Vector3 target = player.position
                           + Vector3.up * followHeight
                           + player.right * sideOffset;

            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                followSpeed * Time.deltaTime
            );

            timer += Time.deltaTime;
            yield return null;
        }

        active = true;

        // SHOW TEXT
        bubbleUI.SetActive(true);
        textUI.text = GetDialogue(stage);

        yield return new WaitForSeconds(bubbleDisplayTime);

        bubbleUI.SetActive(false);

        // FLOAT UP & DISAPPEAR
        active = false;

        timer = 0;
        while (timer < 2f)
        {
            transform.position += Vector3.up * Time.deltaTime * 5f;
            timer += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
        isRunning = false;
    }

    void Update()
    {
        if (!active) return;

        Vector3 target = player.position + Vector3.up * followHeight + player.right * sideOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            target,
            Time.deltaTime * followSpeed
        );
    }

    string GetDialogue(int stage)
    {
        playermovement playerScript = player.GetComponent<playermovement>();

        if (playerScript != null)
        {
            if (playerScript.getIsSanta())
            {
                return "I need to collect " + StageManager.Instance.GetGiftsRequired() + " more gifts";
            }
        }

        return "Wanderer, please help me find " + StageManager.Instance.GetGiftsRequired() + " gifts";
        
    }

    //Method for when playing SANTA
    void ShowPlayerBubble(int stage)
    {
        playermovement playerScript = player.GetComponent<playermovement>();

        if (playerScript != null)
        {
            string msg = GetDialogue(stage);
            playerScript.ShowBubble(msg, bubbleDisplayTime); // call PLAYER coroutine
        }
    }

}
