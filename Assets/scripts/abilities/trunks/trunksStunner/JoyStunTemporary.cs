using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//this is a script temporary from video how to make fixed and floating joystick for mobile input by maana
//Script for trunks stun hability

public class JoyStunTemporary : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    playermovement player;

    public RectTransform background;
    public RectTransform handle;
    [Range(0, 2f)] public float handleLimit = 1f;

    Vector2 input = Vector2.zero;
    Vector2 joyPosition = Vector2.zero;
    public Vector2 Direction => input.normalized;

    //Output
    public float vertical { get { return input.y; } }
    public float horizontal { get { return input.x; } }



    void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<playermovement>();
    }
    

    public void OnPointerDown(PointerEventData eventData) {
        background.gameObject.SetActive(true);
        joyPosition = eventData.position;
        background.position = eventData.position;
        handle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 joyDirection = eventData.position - joyPosition;
        float radius = background.sizeDelta.x / 2f;

        input = (joyDirection.magnitude> radius)
            ? joyDirection.normalized 
            : joyDirection/radius;
        handle.anchoredPosition = input * radius * handleLimit;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        background.gameObject.SetActive(false);

        //send direction to player
        player.spawnStunner(Direction); //this instantiates projectile

        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
