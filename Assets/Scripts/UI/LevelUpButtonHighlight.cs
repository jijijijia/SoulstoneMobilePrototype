using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUpButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly Color OutlineVisible = new(0.86f, 0.68f, 0.32f, 0.85f);
    private static readonly Color OutlineHidden = new(0.86f, 0.68f, 0.32f, 0f);

    private Outline outline;
    private Coroutine fadeCoroutine;

    public void SetOutline(Outline o)
    {
        outline = o;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetFade(OutlineVisible);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetFade(OutlineHidden);
    }

    private void OnDisable()
    {
        if (outline != null)
        {
            outline.effectColor = OutlineHidden;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    private void SetFade(Color target)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeOutline(target));
    }

    private IEnumerator FadeOutline(Color target)
    {
        if (outline == null)
        {
            yield break;
        }

        Color start = outline.effectColor;
        const float duration = 0.1f;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            outline.effectColor = Color.Lerp(start, target, t / duration);
            yield return null;
        }

        outline.effectColor = target;
        fadeCoroutine = null;
    }
}
