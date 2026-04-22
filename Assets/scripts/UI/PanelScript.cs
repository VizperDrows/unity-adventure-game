using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

//This script is for the panel that pops ups when you pause, win, or lose.

public class PanelScript : MonoBehaviour
{
    private GameObject text;
    private TextMeshProUGUI textMesh;
    private Color tempColor;
    GameObject joystick; //controls movement
    GameObject playerObj;
    GameObject resumeButton;
    playermovement player;


    void Start() {
        resumeButton = gameObject.transform.GetChild(2).gameObject; //reference to Panel -> resume gameObject
        joystick = transform.parent.transform.GetChild(1).gameObject; //Reference to FloatingJoystick
    }

    void Update() {
        if (playerObj == null) {
            playerObj = GameObject.FindWithTag("Player");
            player = playerObj.GetComponent<playermovement>();
        }
    }

    public void enable()
    {
        gameObject.SetActive(true);
    }

    public void disable()
    {
        gameObject.SetActive(false);
    }

    public void setTextColor(bool alive)
    {
        if (alive)
        {
            // if alive pop up alive text
            text = gameObject.transform.GetChild(1).gameObject;
            textMesh = text.GetComponent<TextMeshProUGUI>();
            if(textMesh != null)
            {
                tempColor = textMesh.color;
                tempColor.a = 255;
                textMesh.color = tempColor;
            }
        }
        else
        {
            //if dead pop up dead text
            text = gameObject.transform.GetChild(0).gameObject;
            textMesh = text.GetComponent<TextMeshProUGUI>();
            if (textMesh != null)
            {
                tempColor = textMesh.color;
                tempColor.a = 255;
                textMesh.color = tempColor;
            }
        }
    }

    public void resumeGame()
    {
        if (joystick != null) joystick.SetActive(true);
        if (resumeButton != null) resumeButton.SetActive(false);
        Time.timeScale = 1f;
        if (player != null) player.setIsPause(false);
        disable();
    }

    public void restart()
    {
        SceneManager.LoadScene("charselec");
    }

    public void mainmenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void exit()
    {
        Application.Quit();
        //UnityEditor.EditorApplication.isPlaying = false;
    }

}
