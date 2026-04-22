using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseScript : MonoBehaviour
{
    PanelScript myPanel;
    GameObject joystick;
    GameObject playerObj;
    GameObject resumeButton;
    playermovement player;

    //for fading interface
    public float fadeDuration = 60f;
    public float startTime = 10f;


    // Start is called before the first frame update
    void Start()
    {
        myPanel = transform.parent.GetComponentInChildren<PanelScript>(true);
        resumeButton = myPanel.gameObject.transform.GetChild(2).gameObject;
        joystick = transform.parent.transform.GetChild(1).gameObject;
        StartCoroutine(timer());
    }

    // Update is called once per frame
    void Update()
    {
        if (playerObj == null) {
            playerObj = GameObject.FindWithTag("Player");
            player = playerObj.GetComponent<playermovement>();
        }
    }

    public void pauseGame() {
        fadeButton();
        if(joystick != null) joystick.SetActive(false);
        myPanel.enable();
        if(resumeButton != null) resumeButton.SetActive(true);
        Time.timeScale = 0f;
        if (player != null) player.setIsPause(true);
    }

    //for fading interactive
    void fadeButton() {
        var canvGroup = GetComponent<CanvasGroup>();
        StartCoroutine(doFade(canvGroup, 1, 0));
    }
    IEnumerator doFade(CanvasGroup canvGroup, float start, float end) {
        float counter = 0f;

        while (counter < fadeDuration) {
            counter += Time.deltaTime;
            canvGroup.alpha = Mathf.Lerp(start, end, counter/fadeDuration);

            yield return null;
        }
    }
    IEnumerator timer() {
        yield return new WaitForSeconds(startTime);
        fadeButton();
    }
    //
}
