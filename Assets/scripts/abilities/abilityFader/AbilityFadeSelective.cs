using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AbilityFadeSelective : MonoBehaviour
{
    [Header("Fade Settings")]
    public float delayBeforeFade = 30f;
    public float fadeDuration = 6f;

    [Header("Images To Fade")]
    public Image buttonImage;   // this object's Image
    public Image iconImage;         // child icon you want to fade

    void Start()
    {

        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        float t = 0f;
        float startAlphaButton = 0.5f;
        float startAlphaIcon = 1f;
        float endAlpha = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float aBu = Mathf.Lerp(startAlphaButton, endAlpha, t / fadeDuration);
            float aIc = Mathf.Lerp(startAlphaIcon, endAlpha, t / fadeDuration);

            // Fade only these two
            if (buttonImage != null)
            {
                Color c = buttonImage.color;
                buttonImage.color = new Color(c.r, c.g, c.b, aBu);
            }

            if (iconImage != null)
            {
                Color c = iconImage.color;
                iconImage.color = new Color(c.r, c.g, c.b, aIc);
            }
            yield return null;
        }
    }

}

