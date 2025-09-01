// File: CleanInteraction.cs
using UnityEngine;

namespace Nat
{
    /// <summary>
    /// "Clean" style interactable (e.g., push shards aside).
    /// - Inspector prompt text (locked & available).
    /// - Requires a BoolFlag (item possessed). If missing: play locked SFX and do nothing.
    /// - If present: play success SFX, then disable/destroy target (optional delay).
    /// - Loudness: on locked attempt -> ApplyLocked(); on success -> ApplyFast() and ApplySlow().
    /// </summary>
    public class CleanInteraction : MonoBehaviour, IInteractable
    {
        [Header("UI Prompt")]
        [SerializeField] private string promptWhenAvailable = "Push shards aside [E]";
        [SerializeField] private string promptWhenLocked   = "It's too sharp to touch [E]";

        [Header("Requirement")]
        [Tooltip("Flag that indicates the player has the required item (e.g., gloves). If null, it's considered available.")]
        [SerializeField] private BoolFlag requiredFlag;

        [Header("Result")]
        [Tooltip("Object to hide when cleaning succeeds. Defaults to this GameObject.")]
        [SerializeField] private GameObject targetToDisable;
        [Tooltip("If true, destroy the target instead of SetActive(false).")]
        [SerializeField] private bool destroyOnClean = false;
        [Tooltip("Optional delay before disabling/destroying the target, to let SFX start.")]
        [SerializeField] private float successDisableDelay = 0f;

        [Header("Audio")]
        [Tooltip("Used for both locked and success SFX (can be on another object). If null, falls back to PlayClipAtPoint.")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Played when trying to clean without the required flag.")]
        [SerializeField] private AudioClip lockedClip;
        [Tooltip("Played when cleaning succeeds.")]
        [SerializeField] private AudioClip successClip;

        // Internal
        private bool isCleaned = false;
        private LoudInteraction loud; // optional

        private void Awake()
        {
            if (targetToDisable == null) targetToDisable = gameObject;
            loud = GetComponent<LoudInteraction>(); // optional
        }

        public string GetDescription()
        {
            if (isCleaned) return string.Empty;
            return IsLocked() ? promptWhenLocked : promptWhenAvailable;
        }

        public void Interact()
        {
            if (isCleaned) return;

            if (IsLocked())
            {
                // Feedback when missing the required item
                PlaySfx(lockedClip);

                // Loudness: treat as "locked door attempt"
                if (loud != null) loud.ApplyLocked();
                return;
            }

            // Success SFX first, so it can start before the target is hidden/destroyed
            PlaySfx(successClip);

            // Loudness: treat as both fast and slow opening
            if (loud != null)
            {
                loud.ApplyFast();
                loud.ApplySlow();
            }

            // Then perform the clean (optionally after a small delay)
            if (successDisableDelay > 0f)
                StartCoroutine(DoCleanAfterDelay(successDisableDelay));
            else
                DoClean();
        }

        private bool IsLocked()
        {
            if (requiredFlag == null) return false;  // available if no requirement
            return !requiredFlag.Value;               // locked when flag is NOT set
        }

        private System.Collections.IEnumerator DoCleanAfterDelay(float delay)
        {
            float t = 0f;
            while (t < delay)
            {
                t += Time.deltaTime;
                yield return null;
            }
            DoClean();
        }

        private void DoClean()
        {
            isCleaned = true;

            if (targetToDisable != null)
            {
                if (destroyOnClean)
                    Destroy(targetToDisable);
                else
                    targetToDisable.SetActive(false);
            }
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null) return;

            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                // Fallback: spawn a temp audio source at this position so SFX still plays
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
    }
}
