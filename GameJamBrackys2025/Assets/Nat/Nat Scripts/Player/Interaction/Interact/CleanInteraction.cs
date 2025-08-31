// File: CleanInteraction.cs
using UnityEngine;

namespace Nat
{
    /// <summary>
    /// "Clean" style interactable (e.g., push shards aside).
    /// - Inspector prompt text (locked & available).
    /// - Requires a BoolFlag (item possessed). If missing: play locked SFX and do nothing.
    /// - If present: disable/destroy target.
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

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Played when trying to clean without the required flag.")]
        [SerializeField] private AudioClip lockedClip;

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

            if (IsLocked())
                return promptWhenLocked;

            return promptWhenAvailable;
        }

        public void Interact()
        {
            if (isCleaned) return;

            if (IsLocked())
            {
                // Feedback when missing the required item
                PlayOneShot(lockedClip);

                // Loudness: treat as "locked door attempt"
                if (loud != null)
                    loud.ApplyLocked();

                return;
            }

            // Success: clean it
            DoClean();

            // Loudness: treat as both fast and slow opening
            if (loud != null)
            {
                loud.ApplyFast();
                loud.ApplySlow();
            }
        }

        private bool IsLocked()
        {
            // If no flag assigned, consider it available
            if (requiredFlag == null) return false;

            // Locked when the required flag is NOT set
            return !requiredFlag.Value;
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

        private void PlayOneShot(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
}
