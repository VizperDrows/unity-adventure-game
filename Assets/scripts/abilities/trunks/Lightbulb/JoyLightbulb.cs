using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoyLightbulb : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI refs")]
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

        input = clamped / handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!active) return;
        active = false;

        if (handle != null) handle.position = background.position;
        if (background != null) background.gameObject.SetActive(false);

        // Fire lightbulb
        if (playermovement.Instance != null)
            playermovement.Instance.SpawnLightbulb(input);

        input = Vector2.zero;
    }
}
