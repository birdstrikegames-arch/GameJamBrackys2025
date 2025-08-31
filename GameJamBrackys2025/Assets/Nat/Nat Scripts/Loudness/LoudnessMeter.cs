// File: LoudnessMeter.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Nat
{
    /// <summary>
    /// Loudness meter with decay. On max loudness, calls Death.
    /// </summary>
    public class LoudnessMeter : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider loudnessSlider;
        [SerializeField] private float maxLoudness = 100f;

        [Header("Gain & Decay")]
        [SerializeField] private float decayDelay = 1.0f;
        [SerializeField] private float decayPerSecond = 15f;

        [Header("On Max Loudness")]
        [SerializeField] private Animator consequenceAnimator;     // passed to Death
        [SerializeField] private string consequenceTrigger = "Alert";
        [SerializeField] private float freezeDuration = 1.5f;      // PlayerMovement freeze time (passed to Death)
        [SerializeField] private PlayerMovement playerMovement;    // optional, passed to Death
        [SerializeField] private float timeBeforePause = 1.0f;     // unscaled delay before pausing/GO UI
        [TextArea]
        [SerializeField] private string gameOverMessage = "You made too much noise!";

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
            UpdateSlider();
        }

        private void Update()
        {
            if (gameOverTriggered) return;

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

            // Hand off to Death manager (anim trigger now; actual pause/UI after delay)
            Death.Instance?.Die(
                gameOverMessage,
                timeBeforePause,
                consequenceAnimator,
                consequenceTrigger,
                playerMovement,
                freezeDuration
            );

            // No further local handling — Death manages pause/UI.
            yield break;
        }

        public void ResetMeter(float toValue = 0f)
        {
            gameOverTriggered = false;
            currentLoudness = Mathf.Clamp(toValue, 0f, maxLoudness);
            lastIncreaseUnscaledTime = Time.unscaledTime;
            UpdateSlider();
        }
    }
}
