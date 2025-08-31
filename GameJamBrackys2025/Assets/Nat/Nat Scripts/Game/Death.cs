// File: Death.cs
using UnityEngine;
using TMPro;

namespace Nat
{
    /// <summary>
    /// Centralized death handler.
    /// - Immediately locks GameManager ESC handling
    /// - Triggers optional character/alert anim and Black Fade anim
    /// - Waits (unscaled) before pausing & showing Game Over UI
    /// - Pauses the game, unlocks cursor, shows message
    /// Other systems should call Death.Instance.Die(...).
    /// </summary>
    public class Death : MonoBehaviour
    {
        public static Death Instance { get; private set; }

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverRoot;             // Panel/Canvas to enable
        [SerializeField] private TextMeshProUGUI gameOverMessageText; // Optional TMP for message
        [TextArea]
        [SerializeField] private string defaultMessage = "You Died";

        [Header("Black Fade Animator")]
        [Tooltip("Animator that plays a 'Black' trigger (set Update Mode: Unscaled Time).")]
        [SerializeField] private Animator blackFadeAnimator;
        [SerializeField] private string blackFadeTrigger = "Black";

        [Header("Fallback Timing")]
        [Tooltip("If Die(...) passes 0, this fallback unscaled delay is used (seconds).")]
        [SerializeField] private float prePauseDelaySeconds = 1.0f;

        private bool hasDied = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            // Optional: DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Triggers death flow.
        /// </summary>
        /// <param name="message">Optional message to show; falls back to defaultMessage if null/empty.</param>
        /// <param name="prePauseUnscaledDelay">Unscaled seconds to wait before pausing & showing UI (0 = use fallback).</param>
        /// <param name="anim">Optional animator to trigger immediately (e.g., character/alert anim).</param>
        /// <param name="animTrigger">Animator trigger name.</param>
        /// <param name="playerMovement">Optional PlayerMovement to freeze briefly.</param>
        /// <param name="freezeDuration">Seconds to freeze PlayerMovement.</param>
        public void Die(
            string message = null,
            float prePauseUnscaledDelay = 0f,
            Animator anim = null,
            string animTrigger = null,
            PlayerMovement playerMovement = null,
            float freezeDuration = 0f)
        {
            if (hasDied) return;
            hasDied = true;

            // Lock ESC immediately so pause/unpause can't happen during death sequence
            GameManager.Instance?.SetPauseInputLocked(true);

            StartCoroutine(DieRoutine(message, prePauseUnscaledDelay, anim, animTrigger, playerMovement, freezeDuration));
        }

        private System.Collections.IEnumerator DieRoutine(
            string message,
            float prePauseUnscaledDelay,
            Animator anim,
            string animTrigger,
            PlayerMovement playerMovement,
            float freezeDuration)
        {
            // Optional: freeze character control immediately
            if (playerMovement != null && freezeDuration > 0f)
                playerMovement.FreezeMovement(freezeDuration);

            // Optional: trigger any supplied animation immediately (e.g., alert)
            if (anim != null && !string.IsNullOrEmpty(animTrigger))
                anim.SetTrigger(animTrigger);

            // Trigger black fade BEFORE pausing
            if (blackFadeAnimator != null && !string.IsNullOrEmpty(blackFadeTrigger))
                blackFadeAnimator.SetTrigger(blackFadeTrigger);

            // Wait unscaled delay (use provided value, or fallback if 0)
            float wait = (prePauseUnscaledDelay > 0f) ? prePauseUnscaledDelay : Mathf.Max(0f, prePauseDelaySeconds);
            float t = 0f;
            while (t < wait)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Pause & show cursor AFTER the fade has started
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Show Game Over UI + message
            if (gameOverRoot != null)
                gameOverRoot.SetActive(true);

            if (gameOverMessageText != null)
                gameOverMessageText.text = string.IsNullOrEmpty(message) ? defaultMessage : message;
        }

        /// <summary>
        /// Clears death state flag (call on restart if needed).
        /// </summary>
        public void ResetDeath() => hasDied = false;
    }
}
