// File: Electricution.cs
using UnityEngine;

namespace Nat
{
    /// <summary>
    /// If the player enters this trigger without the required BoolFlag:
    /// - Immediately freeze their movement
    /// - Play SFX + trigger animation
    /// - Wait (unscaled) for an inspector-set delay
    /// - Then call Death with the message "You were electricuted!"
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Electricution : MonoBehaviour
    {
        [Header("Requirement")]
        [Tooltip("Flag that indicates the player is insulated/safe (e.g., power is off, gloves obtained). If null, area is always lethal.")]
        [SerializeField] private BoolFlag requiredFlag;

        [Header("Player")]
        [Tooltip("PlayerMovement to freeze immediately on entry. If left null, it will be auto-found at runtime.")]
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Trigger")]
        [SerializeField] private string playerTag = "Player";

        [Header("Pre-Death Sequence")]
        [Tooltip("Unscaled seconds to wait after SFX/animation before triggering Death.")]
        [SerializeField] private float preDeathDelaySeconds = 1.0f;
        [Tooltip("Extra freeze buffer to ensure the player stays frozen through the sequence (seconds).")]
        [SerializeField] private float freezeBufferSeconds = 1.0f;

        [Tooltip("Animator to trigger when the shock happens (set Update Mode: Unscaled Time).")]
        [SerializeField] private Animator effectAnimator;
        [SerializeField] private string effectTrigger = "Shock";

        [Tooltip("Audio Source used to play the shock clip.")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shockClip;

        [Header("Death Message")]
        [TextArea]
        [SerializeField] private string deathMessage = "You were electricuted!";

        private bool sequenceStarted = false;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
        }

        private void Awake()
        {
            if (playerMovement == null)
                playerMovement = FindObjectOfType<PlayerMovement>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (sequenceStarted) return;
            if (!other.CompareTag(playerTag)) return;

            // Safe if the required flag exists and is true
            bool isSafe = (requiredFlag != null) && requiredFlag.Value;
            if (isSafe) return;

            sequenceStarted = true;

            // Immediately freeze player movement for the whole pre-death window + a small buffer
            if (playerMovement != null)
            {
                float totalFreeze = Mathf.Max(0f, preDeathDelaySeconds + freezeBufferSeconds);
                playerMovement.FreezeMovement(totalFreeze);
            }

            // Play SFX and trigger animation right away
            if (audioSource != null && shockClip != null)
                audioSource.PlayOneShot(shockClip);

            if (effectAnimator != null && !string.IsNullOrEmpty(effectTrigger))
                effectAnimator.SetTrigger(effectTrigger);

            // Prevent pause/unpause during the sequence
            GameManager.Instance?.SetPauseInputLocked(true);

            StartCoroutine(ShockThenDie());
        }

        private System.Collections.IEnumerator ShockThenDie()
        {
            // Wait using unscaled time so it progresses even if someone pauses elsewhere
            float t = 0f;
            float wait = Mathf.Max(0f, preDeathDelaySeconds);
            while (t < wait)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Hand off to Death manager (Death will handle black fade, final pause, and game over UI)
            Death.Instance?.Die(
                message: deathMessage,
                prePauseUnscaledDelay: 0f,  // use Death's own configured/fallback pre-pause delay
                anim: null,
                animTrigger: null,
                playerMovement: null,       // already frozen above
                freezeDuration: 0f
            );
        }
    }
}
