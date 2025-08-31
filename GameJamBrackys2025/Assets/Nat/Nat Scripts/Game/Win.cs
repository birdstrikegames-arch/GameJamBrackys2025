// File: Win.cs
using UnityEngine;

namespace Nat
{
    /// <summary>
    /// Interactable that shows a Win screen.
    /// - Plays optional SFX
    /// - Triggers a black fade BEFORE pausing (unscaled delay)
    /// - Locks GameManager ESC handling during the sequence
    /// - After delay: pauses game, enables Win panel, shows cursor
    /// </summary>
    public class Win : MonoBehaviour, IInteractable
    {
        [Header("UI")]
        [SerializeField] private GameObject winPanelRoot;   // Canvas element to enable after delay

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip winClip;

        [Header("Black Fade")]
        [Tooltip("Animator that plays a 'Black' trigger when winning.")]
        [SerializeField] private Animator blackFadeAnimator;
        [SerializeField] private string blackFadeTrigger = "Black";

        [Header("Timing")]
        [Tooltip("Unscaled seconds to wait after triggering fade before pausing & showing the Win panel.")]
        [SerializeField] private float prePauseDelaySeconds = 1.0f;

        [Header("Interaction Text")]
        [SerializeField] private string description = "Win [E]";

        private bool triggered = false;

        public string GetDescription() => triggered ? string.Empty : description;

        public void Interact()
        {
            if (triggered) return;
            triggered = true;

            if (audioSource != null && winClip != null)
                audioSource.PlayOneShot(winClip);

            // Start the fade immediately (animator should be set to Update Mode: Unscaled Time)
            if (blackFadeAnimator != null && !string.IsNullOrEmpty(blackFadeTrigger))
                blackFadeAnimator.SetTrigger(blackFadeTrigger);

            // Lock ESC pause/unpause right away
            GameManager.Instance?.SetPauseInputLocked(true);

            // Run the sequence (wait unscaled, then pause & show panel)
            StartCoroutine(WinSequence());
        }

        private System.Collections.IEnumerator WinSequence()
        {
            float t = 0f;
            float wait = Mathf.Max(0f, prePauseDelaySeconds);
            while (t < wait)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Pause & show UI after the fade has started
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (winPanelRoot != null)
                winPanelRoot.SetActive(true);
        }
    }
}
