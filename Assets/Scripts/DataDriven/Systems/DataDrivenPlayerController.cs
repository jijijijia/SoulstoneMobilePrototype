using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterSystem))]
public class DataDrivenPlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private KeyCode dashKey = KeyCode.Space;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.24f;
    [SerializeField] private float dashRechargeTime = 1.2f;
    [SerializeField] private float dashInvulnerabilityDuration = 0.28f;
    [SerializeField] private int maxDashCharges = 2;

    private CharacterController characterController;
    private CharacterSystem characterSystem;
    private Vector3 velocity;
    private Vector3 lastMoveDirection;
    private Vector3 currentMoveDirection;
    private float dashTimeRemaining;
    private float dashRechargeTimer;
    private int currentDashCharges;
    private bool isDashing;

    public Vector3 LastMoveDirection => lastMoveDirection;
    public int CurrentDashCharges => currentDashCharges;
    public int MaxDashCharges
    {
        get
        {
            int configuredCharges = Mathf.Max(1, maxDashCharges);

            if (characterSystem?.RuntimeStats == null)
            {
                return configuredCharges;
            }

            int statCharges = Mathf.RoundToInt(characterSystem.RuntimeStats.GetValue(StatType.DashCharges));
            return statCharges > 0 ? Mathf.Max(1, statCharges) : configuredCharges;
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        characterSystem = GetComponent<CharacterSystem>();
        currentDashCharges = MaxDashCharges;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        UpdateDashRecharge();

        if (isDashing)
        {
            UpdateDash();
            return;
        }

        Move();
        TryStartDash();
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new(horizontal, 0f, vertical);
        Vector3 moveDirection = inputDirection.normalized;

        if (cameraTransform != null)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            moveDirection = (cameraForward.normalized * vertical + cameraRight.normalized * horizontal).normalized;
        }

        currentMoveDirection = moveDirection;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.forward = moveDirection;
            lastMoveDirection = moveDirection;
        }

        float moveSpeed = characterSystem.RuntimeStats.GetValue(StatType.MoveSpeed);
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void TryStartDash()
    {
        if (!Input.GetKeyDown(dashKey) || currentDashCharges <= 0)
        {
            return;
        }

        Vector3 dashDirection = currentMoveDirection.sqrMagnitude > 0.001f
            ? currentMoveDirection
            : lastMoveDirection;

        if (dashDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        isDashing = true;
        dashTimeRemaining = dashDuration;
        currentDashCharges = Mathf.Max(0, currentDashCharges - 1);
        dashRechargeTimer = dashRechargeTime;
        lastMoveDirection = dashDirection.normalized;
        transform.forward = lastMoveDirection;
        characterSystem.TriggerInvulnerability(dashInvulnerabilityDuration);
    }

    private void UpdateDash()
    {
        if (dashTimeRemaining <= 0f)
        {
            isDashing = false;
            return;
        }

        dashTimeRemaining -= Time.deltaTime;
        characterController.Move(lastMoveDirection * dashSpeed * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (dashTimeRemaining <= 0f)
        {
            isDashing = false;
        }
    }

    private void UpdateDashRecharge()
    {
        if (currentDashCharges >= MaxDashCharges)
        {
            currentDashCharges = MaxDashCharges;
            dashRechargeTimer = 0f;
            return;
        }

        if (dashRechargeTimer > 0f)
        {
            dashRechargeTimer -= Time.deltaTime;
        }

        if (dashRechargeTimer <= 0f)
        {
            currentDashCharges = Mathf.Min(MaxDashCharges, currentDashCharges + 1);

            if (currentDashCharges < MaxDashCharges)
            {
                dashRechargeTimer = dashRechargeTime;
            }
        }
    }
}
