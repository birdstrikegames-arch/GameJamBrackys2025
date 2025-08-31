// File: OxygenManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nat
{
    /// <summary>
    /// Handles the player's oxygen UI and depletion/regen logic.
    /// - Slider stays full at start; HUD hidden when full & not changing.
    /// - Depletes while one or more drains are registered (e.g., CarbonMonoxide zones).
    /// - Regenerates when no drains are active, after a delay.
    /// - On zero: trigger animator, freeze player, then pause + show Game Over with TMP message.
    /// </summary>
    public class OxygenManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider oxygenSlider;                  // Assign UI Slider
        [SerializeField] private GameObject oxygenHudRoot;             // Canvas or panel to toggle
        [SerializeField] private float maxOxygen = 100f;

        [Header("Depletion (from hazards)")]
        // Depletion is provided by external sources via RegisterDrain/UnregisterDrain.

        [Header("Regen (outside hazards)")]
        [Tooltip("If true, oxygen will regenerate when no drains are active.")]
        [SerializeField] private bool enableRegen = true;
        [Tooltip("Seconds after leaving all hazards before regen starts.")]
        [SerializeField] private float regenDelay = 1.0f;
        [Tooltip("Oxygen per second recovered during regen.")]
        [SerializeField] private float regenPerSecond = 12f;

        [Header("Zero-Oxygen Consequence")]
        [SerializeField] private Animator consequenceAnimator;         // Optional
        [SerializeField] private string consequenceTrigger = "Suffocate";
        [Tooltip("Freeze duration (seconds) applied to PlayerMovement before showing Game Over.")]
        [SerializeField] private float freezeDuration = 1.0f;
        [Tooltip("Wait this many seconds (unscaled) before pausing and showing Game Over.")]
        [SerializeField] private float timeBeforePause = 1.0f;
        [SerializeField] private bool pauseGameOnZero = true;

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverRoot;              // Enable on zero
        [SerializeField] private TextMeshProUGUI gameOverMessageText;  // Optional TMP label
        [TextArea]
        [SerializeField] private string gameOverMessage = "You suffocated from carbon monoxide!";

        [Header("Player")]
        [Tooltip("Optional: PlayerMovement to freeze. If null, only timeScale pause will stop motion.")]
        [SerializeField] private PlayerMovement playerMovement;

        // Internal state
        private float currentOxygen;
        private readonly Dictionary<object, float> activeDrains = new Dictionary<object, float>();
        private bool zeroHandled = false;

        // Regen timing
        private bool hadDrainLastFrame = false;
        private float noDrainStartUnscaledTime = 0f;

        private void Awake()
        {
            currentOxygen = maxOxygen;

            if (oxygenSlider != null)
            {
                oxygenSlider.minValue = 0f;
                oxygenSlider.maxValue = maxOxygen;
                oxygenSlider.value = maxOxygen;
            }

            // Assume we start outside hazards
            noDrainStartUnscaledTime = Time.unscaledTime;
            UpdateHudVisibility();
        }

        private void Update()
        {
            if (zeroHandled) return;

            // Sum all active drain rates (per second)
            float totalDrain = 0f;
            foreach (var kv in activeDrains) totalDrain += kv.Value;

            if (totalDrain > 0f)
            {
                // Depletion
                currentOxygen = Mathf.Max(0f, currentOxygen - totalDrain * Time.deltaTime);
                hadDrainLastFrame = true; // mark that we were draining this frame
                SyncSlider();
                UpdateHudVisibility();

                if (currentOxygen <= 0f)
                {
                    StartCoroutine(HandleZeroOxygen());
                }
            }
            else
            {
                // We are not draining this frame
                if (hadDrainLastFrame)
                {
                    hadDrainLastFrame = false;
                    noDrainStartUnscaledTime = Time.unscaledTime; // start regen delay window now
                }

                // Regenerate if enabled, below max, and past the regen delay
                if (enableRegen && currentOxygen < maxOxygen)
                {
                    if (Time.unscaledTime - noDrainStartUnscaledTime >= regenDelay)
                    {
                        currentOxygen = Mathf.Min(maxOxygen, currentOxygen + regenPerSecond * Time.deltaTime);
                        SyncSlider();
                    }
                }

                UpdateHudVisibility();
            }
        }

        // --- Public API ---

        /// <summary>Register a depletion source (rate is oxygen per second).</summary>
        public void RegisterDrain(object key, float ratePerSecond)
        {
            if (key == null) return;
            activeDrains[key] = Mathf.Max(0f, ratePerSecond);
            UpdateHudVisibility();
        }

        /// <summary>Unregister a depletion source.</summary>
        public void UnregisterDrain(object key)
        {
            if (key == null) return;
            activeDrains.Remove(key);
            // regen delay will be handled by Update via hadDrainLastFrame flip
            UpdateHudVisibility();
        }

        /// <summary>Set oxygen directly (clamped 0..max) and update HUD.</summary>
        public void SetOxygen(float value)
        {
            if (zeroHandled) return;
            currentOxygen = Mathf.Clamp(value, 0f, maxOxygen);
            // Start (or restart) regen delay when setting a value (useful if called after hazards)
            noDrainStartUnscaledTime = Time.unscaledTime;
            SyncSlider();
            UpdateHudVisibility();
            if (currentOxygen <= 0f) StartCoroutine(HandleZeroOxygen());
        }

        /// <summary>Refill to max and hide HUD if no active drains.</summary>
        public void RefillToMax()
        {
            if (zeroHandled) return;
            currentOxygen = maxOxygen;
            noDrainStartUnscaledTime = Time.unscaledTime;
            SyncSlider();
            UpdateHudVisibility();
        }

        // --- Internals ---

        private void SyncSlider()
        {
            if (oxygenSlider != null)
                oxygenSlider.value = currentOxygen;
        }

        private void UpdateHudVisibility()
        {
            if (oxygenHudRoot == null) return;

            bool hasDrain = activeDrains.Count > 0;
            bool isFull = Mathf.Approximately(currentOxygen, maxOxygen);

            // Show if depleting or not full; hide if full & no drain
            bool shouldShow = hasDrain || !isFull;

            if (oxygenHudRoot.activeSelf != shouldShow)
                oxygenHudRoot.SetActive(shouldShow);
        }

        private System.Collections.IEnumerator HandleZeroOxygen()
        {
            zeroHandled = true;

            // Trigger animation
            if (consequenceAnimator != null && !string.IsNullOrEmpty(consequenceTrigger))
                consequenceAnimator.SetTrigger(consequenceTrigger);

            // Freeze movement briefly (still respects unscaled wait below)
            if (playerMovement != null && freezeDuration > 0f)
                playerMovement.FreezeMovement(freezeDuration);

            // Wait unscaled time
            float t = 0f;
            float wait = Mathf.Max(0f, timeBeforePause);
            while (t < wait)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Pause
            if (pauseGameOnZero)
                Time.timeScale = 0f;

            // Show Game Over UI and message
            if (gameOverRoot != null)
                gameOverRoot.SetActive(true);

            if (gameOverMessageText != null)
                gameOverMessageText.text = gameOverMessage;
        }
    }
}
