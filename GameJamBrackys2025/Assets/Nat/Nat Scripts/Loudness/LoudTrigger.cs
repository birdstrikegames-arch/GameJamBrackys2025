// File: LoudTrigger.cs
using UnityEngine;
using System.Collections;

namespace Nat
{
    /// <summary>
    /// Place this on a trigger collider. When the player enters:
    /// - Adds a configurable loudness amount
    /// - Plays a sound (with inspector volume control)
    /// - Optionally fires an Animator trigger
    /// Includes a cooldown to avoid spamming if the player stays inside.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LoudTrigger : MonoBehaviour
    {
        [Header("Meter")]
        [SerializeField] private LoudnessMeter meter;
        [SerializeField] private float loudnessAmount = 10f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip triggerClip;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1.0f;

        [Header("Optional Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string animatorTrigger = "Play";

        [Header("Trigger Settings")]
        [Tooltip("Tag used to identify the player.")]
        [SerializeField] private string playerTag = "Player";
        [Tooltip("Minimum seconds between activations while the player remains in the trigger.")]
        [SerializeField] private float cooldownSeconds = 0.5f;
        [Tooltip("If true, the trigger can fire multiple times (with cooldown). If false, it fires once and disables itself.")]
        [SerializeField] private bool repeatable = true;

        private bool isCoolingDown = false;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Awake()
        {
            if (meter == null)
            {
                meter = FindObjectOfType<LoudnessMeter>();
            }
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            TryFire();
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            // Optional: also allow periodic firing while inside (cooldown-gated)
            TryFire();
        }

        private void TryFire()
        {
            if (isCoolingDown) return;

            // Meter
            if (meter != null && loudnessAmount > 0f)
                meter.AddLoudness(loudnessAmount);

            // Sound
            if (audioSource != null && triggerClip != null)
                audioSource.PlayOneShot(triggerClip, Mathf.Clamp01(volume));

            // Animation
            if (animator != null && !string.IsNullOrEmpty(animatorTrigger))
                animator.SetTrigger(animatorTrigger);

            // Cooldown + repeat behavior
            StartCoroutine(Cooldown());

            if (!repeatable)
                gameObject.SetActive(false);
        }

        private IEnumerator Cooldown()
        {
            isCoolingDown = true;
            float t = 0f;
            float wait = Mathf.Max(0f, cooldownSeconds);
            while (t < wait)
            {
                t += Time.deltaTime;
                yield return null;
            }
            isCoolingDown = false;
        }
    }
}
