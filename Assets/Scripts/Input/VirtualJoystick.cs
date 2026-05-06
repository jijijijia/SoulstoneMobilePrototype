using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class VirtualJoystick : MonoBehaviour, IInputProvider, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform knob;

    private Camera uiCamera;
    private Vector2 moveInput;
    private int dashPressFramesLeft;

    public Vector2 MoveInput => moveInput;
    public bool DashPressed => dashPressFramesLeft > 0;

    private void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
    }

    private void LateUpdate()
    {
        if (dashPressFramesLeft > 0)
        {
            dashPressFramesLeft--;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null)
        {
            return;
        }

        float radius = background.rect.width * 0.5f;

        if (radius <= 0f)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, uiCamera, out Vector2 localPoint))
        {
            return;
        }

        Vector2 direction = localPoint / radius;
        float magnitude = direction.magnitude;

        if (magnitude > 1f)
        {
            direction /= magnitude;
        }

        if (knob != null)
        {
            knob.anchoredPosition = direction * radius;
        }

        moveInput = direction;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        moveInput = Vector2.zero;

        if (knob != null)
        {
            knob.anchoredPosition = Vector2.zero;
        }
    }

    public void NotifyDashPressed()
    {
        dashPressFramesLeft = 2;
    }
}
