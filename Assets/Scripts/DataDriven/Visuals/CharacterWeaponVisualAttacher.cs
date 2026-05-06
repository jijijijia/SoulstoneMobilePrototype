using UnityEngine;

[DisallowMultipleComponent]
public sealed class CharacterWeaponVisualAttacher : MonoBehaviour
{
    private const string RuntimeWeaponName = "RuntimeEquippedWeapon";

    [SerializeField] private CombatVisualAnimator visualAnimator;
    [SerializeField] private Transform explicitWeaponSocket;
    [SerializeField] private Vector3 fallbackLocalPosition = new(0.42f, 0.95f, 0.2f);
    [SerializeField] private Vector3 fallbackLocalEuler = new(18f, 18f, -78f);
    [SerializeField] private Vector3 weaponLocalScale = Vector3.one;

    private GameObject currentWeaponVisual;
    private WeaponData currentWeaponData;

    private void Awake()
    {
        ResolveAnimator();
    }

    public void AttachWeapon(WeaponData weaponData)
    {
        ResolveAnimator();

        if (currentWeaponData == weaponData && currentWeaponVisual != null)
        {
            return;
        }

        ClearWeapon();
        currentWeaponData = weaponData;

        if (weaponData == null || weaponData.VisualPrefab == null)
        {
            return;
        }

        Transform socket = ResolveSocket();
        currentWeaponVisual = Instantiate(weaponData.VisualPrefab, socket);
        currentWeaponVisual.name = RuntimeWeaponName;
        currentWeaponVisual.transform.localPosition = Vector3.zero;
        currentWeaponVisual.transform.localRotation = Quaternion.identity;
        currentWeaponVisual.transform.localScale = weaponLocalScale;
    }

    public void ClearWeapon()
    {
        if (currentWeaponVisual == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(currentWeaponVisual);
        }
        else
        {
            DestroyImmediate(currentWeaponVisual);
        }

        currentWeaponVisual = null;
        currentWeaponData = null;
    }

    private Transform ResolveSocket()
    {
        if (explicitWeaponSocket != null)
        {
            return explicitWeaponSocket;
        }

        if (visualAnimator != null)
        {
            Transform animatorSocket = visualAnimator.WeaponSocket;

            if (animatorSocket != null)
            {
                return animatorSocket;
            }
        }

        GameObject socketObject = new("WeaponSocket");
        explicitWeaponSocket = socketObject.transform;
        explicitWeaponSocket.SetParent(transform, false);
        explicitWeaponSocket.localPosition = fallbackLocalPosition;
        explicitWeaponSocket.localRotation = Quaternion.Euler(fallbackLocalEuler);
        explicitWeaponSocket.localScale = Vector3.one;

        if (visualAnimator != null)
        {
            visualAnimator.ConfigureWeaponSocket(explicitWeaponSocket);
        }

        return explicitWeaponSocket;
    }

    private void ResolveAnimator()
    {
        if (visualAnimator == null)
        {
            visualAnimator = GetComponent<CombatVisualAnimator>() ?? GetComponentInChildren<CombatVisualAnimator>(true);
        }

        if (visualAnimator == null)
        {
            visualAnimator = gameObject.AddComponent<CombatVisualAnimator>();
        }
    }
}
