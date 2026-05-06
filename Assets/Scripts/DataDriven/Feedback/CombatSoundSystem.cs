using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class CombatSoundSystem : MonoBehaviour
{
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip enemyHitClip;
    [SerializeField] private AudioClip criticalHitClip;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;
    [SerializeField] [Range(0f, 0.3f)] private float pitchVariance = 0.1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning($"{nameof(CombatSoundSystem)} expects a scene-authored AudioSource. A fallback AudioSource was added at runtime.", this);
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        CombatFeedbackEvents.DamageTaken += HandleDamageTaken;
    }

    private void OnDisable()
    {
        CombatFeedbackEvents.DamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken(CombatFeedbackEvent feedbackEvent)
    {
        if (feedbackEvent.DamageAmount <= 0f)
        {
            return;
        }

        AudioClip clip = SelectClip(feedbackEvent);

        if (clip == null)
        {
            return;
        }

        audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        audioSource.PlayOneShot(clip, volume);
    }

    private AudioClip SelectClip(in CombatFeedbackEvent feedbackEvent)
    {
        if (feedbackEvent.IsCritical && criticalHitClip != null)
        {
            return criticalHitClip;
        }

        return feedbackEvent.IsPlayerTarget ? playerHitClip : enemyHitClip;
    }
}
