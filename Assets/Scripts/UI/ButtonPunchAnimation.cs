using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPunchAnimation : MonoBehaviour, IPointerDownHandler
{
    private Coroutine punchCoroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
            transform.localScale = Vector3.one;
        }

        punchCoroutine = StartCoroutine(Punch());
    }

    private IEnumerator Punch()
    {
        const float half = 0.05f;
        Vector3 normal = Vector3.one;
        Vector3 small = new(0.93f, 0.93f, 1f);

        for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
        {
            transform.localScale = Vector3.Lerp(normal, small, t / half);
            yield return null;
        }

        for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
        {
            transform.localScale = Vector3.Lerp(small, normal, t / half);
            yield return null;
        }

        transform.localScale = normal;
        punchCoroutine = null;
    }
}
