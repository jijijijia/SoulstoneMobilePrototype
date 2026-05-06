using UnityEngine;

[DisallowMultipleComponent]
public sealed class CombatVisualAnimator : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Animator animator;

    [Header("Procedural Motion")]
    [SerializeField] private float idleBobAmount = 0.025f;
    [SerializeField] private float idleBobSpeed = 2.2f;
    [SerializeField] private float moveBobAmount = 0.055f;
    [SerializeField] private float moveBobSpeed = 9f;
    [SerializeField] private float moveLeanAngle = 5f;
    [SerializeField] private float attackDuration = 0.22f;
    [SerializeField] private float attackSwingAngle = 28f;
    [SerializeField] private float dashLeanAngle = 16f;

    private Transform animatedTransform;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 moveVelocity;
    private float attackTimer;
    private float dashTimer;
    private float dashDuration;

    public Transform WeaponSocket => weaponSocket != null ? weaponSocket : ResolveWeaponSocket();

    private void Awake()
    {
        ResolveTargets();
    }

    private void OnEnable()
    {
        CaptureBasePose();
    }

    private void Update()
    {
        ResolveTargets();

        float deltaTime = Time.deltaTime;
        attackTimer = Mathf.Max(0f, attackTimer - deltaTime);
        dashTimer = Mathf.Max(0f, dashTimer - deltaTime);

        ApplyAnimatorParameters();
        ApplyProceduralPose();
    }

    public void ConfigureVisualRoot(Transform root)
    {
        visualRoot = root;
        ResolveTargets();
        CaptureBasePose();
    }

    public void ConfigureWeaponSocket(Transform socket)
    {
        weaponSocket = socket;
    }

    public void SetMoveVelocity(Vector3 velocity)
    {
        velocity.y = 0f;
        moveVelocity = velocity;
    }

    public void PlayAttack()
    {
        attackTimer = Mathf.Max(attackTimer, attackDuration);

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void PlayDash(float duration)
    {
        dashDuration = Mathf.Max(0.05f, duration);
        dashTimer = dashDuration;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger("Dash");
        }
    }

    private void ResolveTargets()
    {
        if (visualRoot == null)
        {
            visualRoot = ResolveBestVisualRoot(transform);
        }

        if (animatedTransform == null)
        {
            animatedTransform = visualRoot != null ? visualRoot : transform;
            CaptureBasePose();
        }

        if (animator == null && visualRoot != null)
        {
            animator = visualRoot.GetComponentInChildren<Animator>(true);
        }
    }

    private void CaptureBasePose()
    {
        animatedTransform = visualRoot != null ? visualRoot : transform;

        if (animatedTransform == null)
        {
            return;
        }

        baseLocalPosition = animatedTransform.localPosition;
        baseLocalRotation = animatedTransform.localRotation;
    }

    private void ApplyAnimatorParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        float speed = moveVelocity.magnitude;
        animator.SetFloat("MoveSpeed", speed);
        animator.SetBool("IsMoving", speed > 0.05f);
    }

    private void ApplyProceduralPose()
    {
        if (animatedTransform == null)
        {
            return;
        }

        float speed = moveVelocity.magnitude;
        float move01 = Mathf.Clamp01(speed / 4f);
        float bob = Mathf.Sin(Time.time * Mathf.Lerp(idleBobSpeed, moveBobSpeed, move01));
        float bobAmount = Mathf.Lerp(idleBobAmount, moveBobAmount, move01);

        float attack01 = attackDuration > 0f ? Mathf.Clamp01(attackTimer / attackDuration) : 0f;
        float attackSwing = Mathf.Sin((1f - attack01) * Mathf.PI) * attackSwingAngle;

        float dash01 = dashDuration > 0f ? Mathf.Clamp01(dashTimer / dashDuration) : 0f;
        float dashLean = Mathf.Sin(dash01 * Mathf.PI) * dashLeanAngle;

        Vector3 localPosition = baseLocalPosition + Vector3.up * bob * bobAmount;
        Quaternion localRotation =
            baseLocalRotation *
            Quaternion.Euler(dashLean + move01 * moveLeanAngle, attackSwing, -attackSwing * 0.35f);

        animatedTransform.localPosition = localPosition;
        animatedTransform.localRotation = localRotation;
    }

    private Transform ResolveWeaponSocket()
    {
        if (weaponSocket != null)
        {
            return weaponSocket;
        }

        Transform root = visualRoot != null ? visualRoot : transform;
        weaponSocket = FindDeepChild(root, "WeaponSocket") ??
                       FindDeepChild(root, "RightHand") ??
                       FindDeepChild(root, "Hand_R") ??
                       FindDeepChild(root, "hand.R");

        if (weaponSocket != null)
        {
            return weaponSocket;
        }

        GameObject socketObject = new("WeaponSocket");
        weaponSocket = socketObject.transform;
        weaponSocket.SetParent(root, false);
        weaponSocket.localPosition = new Vector3(0.42f, 0.95f, 0.2f);
        weaponSocket.localRotation = Quaternion.Euler(18f, 18f, -78f);
        weaponSocket.localScale = Vector3.one;
        return weaponSocket;
    }

    private static Transform ResolveBestVisualRoot(Transform owner)
    {
        if (owner == null)
        {
            return null;
        }

        Transform importedVisual = owner.Find("ImportedVisual");

        if (importedVisual != null)
        {
            return importedVisual;
        }

        for (int i = 0; i < owner.childCount; i++)
        {
            Transform child = owner.GetChild(i);

            if (child != null && child.GetComponentInChildren<Renderer>(true) != null)
            {
                return child;
            }
        }

        return owner;
    }

    private static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        if (root.name == childName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeepChild(root.GetChild(i), childName);

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
