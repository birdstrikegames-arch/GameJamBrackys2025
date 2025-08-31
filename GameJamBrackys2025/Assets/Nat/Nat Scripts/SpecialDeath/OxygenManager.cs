// File: OxygenManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nat
{
    /// <summary>
    /// Oxygen depletion/regen with HUD. On zero oxygen, calls Death.
    /// </summary>
    public class OxygenManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider oxygenSlider;
        [SerializeField] private GameObject oxygenHudRoot;
        [SerializeField] private float maxOxygen = 100f;

        [Header("Regen (outside hazards)")]
        [SerializeField] private bool enableRegen = true;
        [SerializeField] private float regenDelay = 1.0f;
        [SerializeField] private float regenPerSecond = 12f;

        [Header("Zero-Oxygen Consequence")]
        [SerializeField] private Animator consequenceAnimator;   // passed to Death
        [SerializeField] private string consequenceTrigger = "Suffocate";
        [SerializeField] private float freezeDuration = 1.0f;    // PlayerMovement freeze time (passed to Death)
        [SerializeField] private float timeBeforePause = 1.0f;   // unscaled delay before pausing/GO UI
        [TextArea]
        [SerializeField] private string gameOverMessage = "You suffocated from carbon monoxide!";

        [Header("Player")]
        [SerializeField] private PlayerMovement playerMovement;  // optional, passed to Death

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

            noDrainStartUnscaledTime = Time.unscaledTime;
            UpdateHudVisibility();
        }

        private void Update()
        {
            if (zeroHandled) return;

            float totalDrain = 0f;
            foreach (var kv in activeDrains) totalDrain += kv.Value;

            if (totalDrain > 0f)
            {
                currentOxygen = Mathf.Max(0f, currentOxygen - totalDrain * Time.deltaTime);
                hadDrainLastFrame = true;
                SyncSlider();
                UpdateHudVisibility();

                if (currentOxygen <= 0f)
                {
                    zeroHandled = true;
                    // Hand off to Death manager
                    Death.Instance?.Die(
                        gameOverMessage,
                        timeBeforePause,
                        consequenceAnimator,
                        consequenceTrigger,
                        playerMovement,
                        freezeDuration
                    );
                }
            }
            else
            {
                if (hadDrainLastFrame)
                {
                    hadDrainLastFrame = false;
                    noDrainStartUnscaledTime = Time.unscaledTime;
                }

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
        public void RegisterDrain(object key, float ratePerSecond)
        {
            if (key == null) return;
            activeDrains[key] = Mathf.Max(0f, ratePerSecond);
            UpdateHudVisibility();
        }

        public void UnregisterDrain(object key)
        {
            if (key == null) return;
            activeDrains.Remove(key);
            UpdateHudVisibility();
        }

        public void SetOxygen(float value)
        {
            if (zeroHandled) return;
            currentOxygen = Mathf.Clamp(value, 0f, maxOxygen);
            noDrainStartUnscaledTime = Time.unscaledTime;
            SyncSlider();
            UpdateHudVisibility();

            if (currentOxygen <= 0f && !zeroHandled)
            {
                zeroHandled = true;
                Death.Instance?.Die(
                    gameOverMessage,
                    timeBeforePause,
                    consequenceAnimator,
                    consequenceTrigger,
                    playerMovement,
                    freezeDuration
                );
            }
        }

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
            bool shouldShow = hasDrain || !isFull;

            if (oxygenHudRoot.activeSelf != shouldShow)
                oxygenHudRoot.SetActive(shouldShow);
        }
    }
}
