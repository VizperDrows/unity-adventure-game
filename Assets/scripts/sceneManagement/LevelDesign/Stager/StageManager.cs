using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public int currentStage = 1; // s = 1..5
    private int giftsCollected = 0;

    private int giftsRequired;

    private bool isIntro = true; // controls first trigger at 3 gifts
    private bool isFinalPhase = false; // controls final stage behavior (if needed)

    void Awake()
    {
        Instance = this;
        UpdateRequirement();
    }

    void UpdateRequirement()
    {
        if (isIntro)
            giftsRequired = 3;
        else
            giftsRequired = (int)Mathf.Pow(3, currentStage);
    }

    public int GetGiftsRequired()
    {
        return giftsRequired;
    }

    public void OnGiftCollected()
    {
        if (isFinalPhase) return; // stop normal system

        giftsCollected++;

        // INTRO PHASE
        if (isIntro)
        {
            if (giftsCollected >= 3)
            {
                isIntro = false;
                giftsCollected = 0;

                // Now officially start Stage 1
                currentStage = 1;
                UpdateRequirement();

                //Call SantaAnnouncer
                if(!IsFrank()) SantaController.Instance.TriggerSanta(currentStage);
            }

            return; 
        }

        // NORMAL STAGES
        // Stage completion
        if (giftsCollected >= giftsRequired)
        {
            AdvanceStage();
        }
    }

    //This method calls SantaController
    void AdvanceStage()
    {
        if (currentStage >= 5)
        {
            EnterFinalPhase();
            return;
        }

        currentStage++;
        giftsCollected = 0;

        if (currentStage <= 5)
        {
            UpdateRequirement();
            //Call SantaAnnouncer
            if (!IsFrank()) SantaController.Instance.TriggerSanta(currentStage);
        }
    }

    void EnterFinalPhase()
    {
        isFinalPhase = true;
        giftsCollected = 0;

        Debug.Log("Entering Final Phase!");

        // Santa final dialogue
        //Call SantaAnnouncer
        if (!IsFrank()) SantaController.Instance.TriggerSanta(6);

    }

    // helper method to get Frank
    bool IsFrank()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        playermovement pm = player.GetComponent<playermovement>();
        if (pm == null) return false;

        return pm.getIsFrank(); // assuming you already have this
    }
}
