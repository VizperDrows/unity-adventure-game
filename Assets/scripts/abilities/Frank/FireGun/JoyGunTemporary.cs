using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoyGunTemporary : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI refs")]
    public RectTransform background;
    public RectTransform handle;

    [Header("Tuning")]
    public float handleRange = 70f;
    public float fireRate = 2f; // bullets per second

    private Vector2 input = Vector2.zero;
    private bool active = false;
    private Coroutine fireRoutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        active = true;

        if (background != null)
        {
            background.gameObject.SetActive(true);
            background.position = eventData.position;
        }

        OnDrag(eventData);

        // Start continuous firing
        fireRoutine = StartCoroutine(FireLoop());
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!active || background == null || handle == null)
            return;

        Vector2 delta = eventData.position - (Vector2)background.position;
        Vector2 clamped = Vector2.ClampMagnitude(delta, handleRange);

        handle.position = background.position + (Vector3)clamped;
        input = clamped / handleRange; // normalized

        if(playermovement.Instance != null) playermovement.Instance.StartShooting(input);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        active = false;

        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }

        if (handle != null)
            handle.position = background.position;

        if (background != null)
            background.gameObject.SetActive(false);

        input = Vector2.zero;

        if(playermovement.Instance != null) playermovement.Instance.StopShooting();
    }

    IEnumerator FireLoop()
    {
        float interval = 1f / fireRate;

        while (active)
        {
            Vector2 fireDir = input;

            // If joystick is not being dragged, use character's facing
            if (fireDir.sqrMagnitude < 0.001f && playermovement.Instance != null)
            {
                bool facingLeft = playermovement.Instance.transform.localScale.x < 0f;
                fireDir = facingLeft ? Vector2.left : Vector2.right; // left/right in joystick space
            }

            if (playermovement.Instance != null)
                playermovement.Instance.spawnBullet(fireDir);

            yield return new WaitForSeconds(interval);
        }
    }
}

