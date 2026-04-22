using System.Linq;
using UnityEngine;

public class IgluManager : MonoBehaviour
{
    public static IgluManager Instance;

    private IgluTeleport[] iglus;
    public GameObject mapUI;            // assign map panel

    //for disable input
    public GameObject joystickUI;
    public GameObject pauseButtonUI;


    private int currentIglu;
    private playermovement playerRef;

    private void Awake()
    {
        Instance = this;
        mapUI.SetActive(false);

        // Find all iglus in scene
        iglus = FindObjectsOfType<IgluTeleport>();
    }

    public void OpenMap(int currentIgluID, playermovement player)
    {
        currentIglu = currentIgluID;
        playerRef = player;

        // Freeze gameplay controls
        playerRef.setIsPause(true);          // disables jumping + input
        joystickUI.SetActive(false);         // hide joystick UI
        pauseButtonUI.SetActive(false);      // hide pause button


        Time.timeScale = 0; //pause game
        mapUI.SetActive(true);
    }

    public void SelectIglu(int selectedIgluID)
    {
        mapUI.SetActive(false);

        //selecting same iglu -> no teleport
        if(selectedIgluID != currentIglu)
        {
            // teleport to the selected iglu’s spawn point
            foreach (var iglu in iglus)
            {
                if (iglu.igluID == selectedIgluID)
                {
                    playerRef.transform.position = iglu.spawnPoint.position;
                    break;
                }
            }
        }

        // Restore gameplay controls
        joystickUI.SetActive(true);
        pauseButtonUI.SetActive(true);
        playerRef.setIsPause(false);         // re-enable input


        Time.timeScale = 1; //unpause
    }
}
