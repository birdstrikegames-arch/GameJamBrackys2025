// File: LoudnessMeter.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Nat
{
    /// <summary>
    /// Tracks and displays the player's loudness on a UI Slider.
    /// - Call AddLoudness(amount) from interactions or triggers.
    /// - Gradually decays after a configurable delay.
    /// - When it reaches max, it freezes the player (via PlayerMovement),
    ///   optionally plays an animator trigger, then after a delay pauses the game and shows a Game Over menu.
    /// </summary>
    public class LoudnessMeter : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider loudnessSlider;    // Assign your UI Slider here
        [SerializeField] private float maxLoudness = 100f; // Logical max; slider's maxValue will be set to this

        [Header("Gain & Decay")]
        [Tooltip("Seconds to wait after the last loudness increase before decay starts.")]
        [SerializeField] private float decayDelay = 1.0f;
        [Tooltip("How many loudness points to remove per second once decay starts.")]
        [SerializeField] private float decayPerSecond = 15f;

        [Header("On Max Loudness")]
        [Tooltip("Animator to trigger when max loudness is reached (e.g., alert animation). Optional.")]
        [SerializeField] private Animator consequenceAnimator;
        [Tooltip("Animator trigger fired when max loudness is reached.")]
        [SerializeField] private string consequenceTrigger = "Alert";
        [Tooltip("Freeze player movement for this duration before pausing/showing menu.")]
        [SerializeField] private float freezeDuration = 1.5f;
        [Tooltip("If assigned, the PlayerMovement will be frozen via FreezeMovement(freezeDuration). If null, we just rely on Time.timeScale later.")]
        [SerializeField] private PlayerMovement playerMovement;
        [Tooltip("Seconds after the animation trigger before game is paused and the Game Over menu is enabled.")]
        [SerializeField] private float timeBeforePause = 1.0f;
        [Tooltip("Menu to enable when the game is paused. Typically a canvas root.")]
        [SerializeField] private GameObject gameOverMenu;

        [Header("Pause Behavior")]
        [Tooltip("Whether to pause the game using Time.timeScale = 0 when game over occurs.")]
        [SerializeField] private bool pauseGameOnMax = true;

        // Internal state
        private float currentLoudness = 0f;
        private float lastIncreaseUnscaledTime = -999f;
        private bool gameOverTriggered = false;

        private void Awake()
        {
            if (loudnessSlider != null)
            {
                loudnessSlider.minValue = 0f;
                loudnessSlider.maxValue = maxLoudness;
                loudnessSlider.value = 0f;
            }
        }

        private void OnEnable()
        {
            // Keep slider in sync if re-enabled
            UpdateSlider();
        }

        private void Update()
        {
            if (gameOverTriggered) return;

            // Start decaying if enough time has passed since last increase
            if (Time.unscaledTime - lastIncreaseUnscaledTime >= decayDelay)
            {
                if (currentLoudness > 0f && decayPerSecond > 0f)
                {
                    float delta = decayPerSecond * Time.deltaTime;
                    currentLoudness = Mathf.Max(0f, currentLoudness - delta);
                    UpdateSlider();
                }
            }
        }

        /// <summary>
        /// Adds loudness and checks for game-over threshold.
        /// Use positive values; method clamps to [0, max].
        /// </summary>
        public void AddLoudness(float amount)
        {
            if (gameOverTriggered) return;
            if (amount <= 0f) return;

            currentLoudness = Mathf.Min(maxLoudness, currentLoudness + amount);
            lastIncreaseUnscaledTime = Time.unscaledTime;
            UpdateSlider();

            if (currentLoudness >= maxLoudness)
            {
                StartCoroutine(HandleMaxReached());
            }
        }

        /// <summary>
        /// Set loudness directly (0..max). Updates UI and state.
        /// </summary>
        public void SetLoudness(float value)
        {
            if (gameOverTriggered) return;
            currentLoudness = Mathf.Clamp(value, 0f, maxLoudness);
            lastIncreaseUnscaledTime = Time.unscaledTime;
            UpdateSlider();

            if (currentLoudness >= maxLoudness)
            {
                StartCoroutine(HandleMaxReached());
            }
        }

        private void UpdateSlider()
        {
            if (loudnessSlider != null)
                loudnessSlider.value = currentLoudness;
        }

        private IEnumerator HandleMaxReached()
        {
            gameOverTriggered = true;

            // Freeze player movement if we have a reference
            if (playerMovement != null && freezeDuration > 0f)
            {
                playerMovement.FreezeMovement(freezeDuration);
            }

            // Fire animator trigger immediately (if present)
            if (consequenceAnimator != null && !string.IsNullOrEmpty(consequenceTrigger))
            {
                consequenceAnimator.SetTrigger(consequenceTrigger);
            }

            // Wait real-time seconds (not affected by timeScale)
            float wait = Mathf.Max(0f, timeBeforePause);
            float t = 0f;
            while (t < wait)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Pause game if desired
            if (pauseGameOnMax)
            {
                Time.timeScale = 0f;
            }

            // Show game over menu
            if (gameOverMenu != null)
            {
                gameOverMenu.SetActive(true);
            }
        }

        // Convenience to reset the meter (e.g., on restart)
        public void ResetMeter(float toValue = 0f)
        {
            gameOverTriggered = false;
            currentLoudness = Mathf.Clamp(toValue, 0f, maxLoudness);
            lastIncreaseUnscaledTime = Time.unscaledTime;
            UpdateSlider();
        }
    }
}
