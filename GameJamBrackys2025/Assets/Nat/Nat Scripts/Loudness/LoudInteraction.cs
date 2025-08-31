// File: LoudInteraction.cs
using UnityEngine;

namespace Nat
{
    /// <summary>
    /// Handles adding loudness when interactions occur.
    /// - If 'isDoor' is true, auto-hooks DoorInteractor events (fast/slow + locked).
    /// - If 'isDoor' is false, call ApplyGeneric()/ApplyCustom() or let PlayerInteraction auto-call ApplyGeneric().
    /// </summary>
    public class LoudInteraction : MonoBehaviour
    {
        [Header("Meter")]
        [SerializeField] private LoudnessMeter meter;

        [Header("Generic (when isDoor = false)")]
        [SerializeField] private float genericAmount = 10f;

        [Header("Door amounts (when isDoor = true)")]
        [SerializeField] private float fastAmount = 12f;
        [SerializeField] private float slowAmount = 8f;
        [SerializeField] private float lockedAmount = 6f;

        [Header("Integration")]
        [Tooltip("If true, this script will auto-hook into DoorInteractor on the same object.")]
        [SerializeField] private bool isDoor = false;
        public bool IsDoor => isDoor; // <-- public read-only so other scripts can check

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
                door.OnDoorOpenedFast  += HandleFast;
                door.OnDoorClosedFast  += HandleFast;
                door.OnDoorOpenedSlow  += HandleSlow;
                door.OnDoorClosedSlow  += HandleSlow;
                door.OnDoorLockedAttempt += HandleLocked;
            }
        }

        private void OnDisable()
        {
            if (isDoor && door != null)
            {
                door.OnDoorOpenedFast  -= HandleFast;
                door.OnDoorClosedFast  -= HandleFast;
                door.OnDoorOpenedSlow  -= HandleSlow;
                door.OnDoorClosedSlow  -= HandleSlow;
                door.OnDoorLockedAttempt -= HandleLocked;
            }
        }

        private void HandleFast()   => ApplyFast();
        private void HandleSlow()   => ApplySlow();
        private void HandleLocked() => ApplyLocked();

        // --- Public API ---
        public void ApplyGeneric()
        {
            if (meter != null && genericAmount > 0f)
                meter.AddLoudness(genericAmount);
        }

        public void ApplyCustom(float amount)
        {
            if (meter != null && amount > 0f)
                meter.AddLoudness(amount);
        }

        public void ApplyFast()
        {
            if (meter != null && fastAmount > 0f)
                meter.AddLoudness(fastAmount);
        }

        public void ApplySlow()
        {
            if (meter != null && slowAmount > 0f)
                meter.AddLoudness(slowAmount);
        }

        public void ApplyLocked()
        {
            if (meter != null && lockedAmount > 0f)
                meter.AddLoudness(lockedAmount);
        }
    }
}
