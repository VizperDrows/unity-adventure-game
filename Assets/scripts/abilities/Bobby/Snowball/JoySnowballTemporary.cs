using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoySnowballTemporary : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI refs (same as your stun joystick)")]
    public RectTransform background;
    public RectTransform handle;

    [Header("Tuning")]
    public float handleRange = 70f;

    private Vector2 input = Vector2.zero;
    private bool active = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        active = true;

        if (background != null)
            background.gameObject.SetActive(true);

        // place joystick where you touched (optional; remove if you want fixed joystick)
        if (background != null)
            background.position = eventData.position;

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!active) return;
        if (background == null || handle == null) return;

        Vector2 delta = eventData.position - (Vector2)background.position;
        Vector2 clamped = Vector2.ClampMagnitude(delta, handleRange);

        handle.position = background.position + (Vector3)clamped;

        input = clamped / handleRange; // normalized [-1..1]
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!active) return;
        active = false;

        // reset UI
        if (handle != null) handle.position = background.position;
        if (background != null) background.gameObject.SetActive(false);

        // fire snowball using your singleton instance
        if (playermovement.Instance != null)
            playermovement.Instance.spawnSnowball(input);

        input = Vector2.zero;
    }
}

