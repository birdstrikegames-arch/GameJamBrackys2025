// File: LoudInteraction.cs
using UnityEngine;

namespace Nat
{
    /// <summary>
    /// Handles adding loudness when interactions occur.
    /// - If 'isDoor' is true, auto-hooks DoorInteractor events (fast/slow + locked).
    /// - If 'isDoor' is false, call ApplyGeneric() / ApplyCustom(x) from your interaction script.
    /// </summary>
    public class LoudInteraction : MonoBehaviour
    {
        [Header("Meter")]
        [Tooltip("Reference to the LoudnessMeter in the scene.")]
        [SerializeField] private LoudnessMeter meter;

        [Header("Generic (when isDoor = false)")]
        [Tooltip("Amount added when ApplyGeneric() is called.")]
        [SerializeField] private float genericAmount = 10f;

        [Header("Door amounts (when isDoor = true)")]
        [Tooltip("Loudness for a FAST door action (tap).")]
        [SerializeField] private float fastAmount = 12f;
        [Tooltip("Loudness for a SLOW door action (hold).")]
        [SerializeField] private float slowAmount = 8f;
        [Tooltip("Loudness when the player tries to use a LOCKED door.")]
        [SerializeField] private float lockedAmount = 6f;

        [Header("Integration")]
        [Tooltip("If true, this script will auto-hook into DoorInteractor on the same object.")]
        [SerializeField] private bool isDoor = false;

        private DoorInteractor door;

        private void Awake()
        {
            if (meter == null)
                meter = FindObjectOfType<LoudnessMeter>();

            if (isDoor)
                door = GetComponent<DoorInteractor>();
        }

        private void OnEnable()
        {
            if (isDoor && door != null)
            {
                // subscribe to door events
                door.OnDoorOpenedFast += HandleFast;
                door.OnDoorClosedFast += HandleFast;
                door.OnDoorOpenedSlow += HandleSlow;
                door.OnDoorClosedSlow += HandleSlow;
                door.OnDoorLockedAttempt += HandleLocked; // NEW
            }
        }

        private void OnDisable()
        {
            if (isDoor && door != null)
            {
                // unsubscribe to avoid leaks
                door.OnDoorOpenedFast -= HandleFast;
                door.OnDoorClosedFast -= HandleFast;
                door.OnDoorOpenedSlow -= HandleSlow;
                door.OnDoorClosedSlow -= HandleSlow;
                door.OnDoorLockedAttempt -= HandleLocked; // NEW
            }
        }

        // --- Door event handlers ---
        private void HandleFast()   => ApplyFast();
        private void HandleSlow()   => ApplySlow();
        private void HandleLocked() => ApplyLocked();

        // --- Public API ---
        /// <summary>Add the generic amount — use when isDoor is false.</summary>
        public void ApplyGeneric()
        {
            if (meter != null && genericAmount > 0f)
                meter.AddLoudness(genericAmount);
        }

        /// <summary>Add a custom amount.</summary>
        public void ApplyCustom(float amount)
        {
            if (meter != null && amount > 0f)
                meter.AddLoudness(amount);
        }

        /// <summary>Add fast door amount.</summary>
        public void ApplyFast()
        {
            if (meter != null && fastAmount > 0f)
                meter.AddLoudness(fastAmount);
        }

        /// <summary>Add slow door amount.</summary>
        public void ApplySlow()
        {
            if (meter != null && slowAmount > 0f)
                meter.AddLoudness(slowAmount);
        }

        /// <summary>Add locked door amount.</summary>
        public void ApplyLocked()
        {
            if (meter != null && lockedAmount > 0f)
                meter.AddLoudness(lockedAmount);
        }
    }
}
