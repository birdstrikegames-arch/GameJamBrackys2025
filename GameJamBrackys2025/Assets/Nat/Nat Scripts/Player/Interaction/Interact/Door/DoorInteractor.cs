// File: DoorInteractor.cs
using UnityEngine;
using System.Collections;

namespace Nat
{
    public class DoorInteractor : MonoBehaviour, IInteractable
    {
        [Header("Animation (Instant)")]
        [SerializeField] private Animator doorAnimator;
        [SerializeField] private string openTrigger = "Open";     // instant open
        [SerializeField] private string closeTrigger = "Close";   // instant close

        [Header("Animation (Hold)")]
        [SerializeField] private float holdDurationSeconds = 1.5f;    // hold time to trigger slow action
        [SerializeField] private string holdOpenTrigger = "OpenSlow"; // slow open trigger
        [SerializeField] private string holdCloseTrigger = "";         // optional slow close trigger (leave empty to use instant close)

        [Header("Locking")]
        [SerializeField] private BoolFlag unlockCondition; // if assigned, overrides manual lock
        [SerializeField] private bool isLocked = false;    // used only when unlockCondition is null

        [Header("Cooldown")]
        [SerializeField] private float cooldownTime = 0.75f;
        private bool isCoolingDown = false;

        [Header("Sound Clips")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip fastClip;    // used for both fast open/close
        [SerializeField] private AudioClip slowClip;    // used for both slow open/close
        [SerializeField] private AudioClip lockedClip;  // plays when interacting with a locked door
        [SerializeField] private float closeSoundDelay = 0.0f;

        [Header("UI Text (fully manual)")]
        [SerializeField] private string descriptionWhenClosed = "Open [E] / Hold [E] for slow open";
        [SerializeField] private string descriptionWhenOpen = "Close [E]";
        [SerializeField] private string descriptionWhenLocked = "Locked";

        // State
        [SerializeField] private bool isOpen = false;
        private bool isInteracting = false;

        // Events for external hooks (e.g., LoudInteraction with isDoor = true)
        public event System.Action OnDoorOpenedFast;
        public event System.Action OnDoorClosedFast;
        public event System.Action OnDoorOpenedSlow;
        public event System.Action OnDoorClosedSlow;
        public event System.Action OnDoorLockedAttempt; // NEW: fired when player tries to use a locked door

        // --- IInteractable ---
        public string GetDescription()
        {
            if (IsDoorLocked()) return descriptionWhenLocked;
            return isOpen ? descriptionWhenOpen : descriptionWhenClosed;
        }

        public void Interact()
        {
            if (isCoolingDown || isInteracting) return;
            StartCoroutine(HandleInteractionSession());
        }

        private IEnumerator HandleInteractionSession()
        {
            isInteracting = true;

            if (IsDoorLocked())
            {
                // Locked feedback
                PlayOneShot(lockedClip);
                OnDoorLockedAttempt?.Invoke(); // notify listeners (e.g., LoudInteraction) to add loudness
                isInteracting = false;
                yield break;
            }

            float timer = 0f;

            // wait to determine tap vs hold (uses unscaled time so it works if someone pauses time elsewhere)
            while (Input.GetKey(KeyCode.E))
            {
                if (isCoolingDown || IsDoorLocked())
                {
                    isInteracting = false;
                    yield break;
                }

                timer += Time.unscaledDeltaTime;
                if (timer >= holdDurationSeconds) break;
                yield return null;
            }

            bool didHoldLongEnough = timer >= holdDurationSeconds;

            if (didHoldLongEnough)
            {
                if (!isOpen) OpenDoorSlow();
                else CloseDoorSlow();
            }
            else
            {
                if (!isOpen) OpenDoorFast();
                else CloseDoorFast();
            }

            isInteracting = false;
        }

        // --- Logic helpers ---
        private bool IsDoorLocked()
        {
            if (unlockCondition != null) return !unlockCondition.Value;
            return isLocked;
        }

        private void OpenDoorFast()
        {
            isOpen = true;

            if (doorAnimator != null && !string.IsNullOrEmpty(openTrigger))
                doorAnimator.SetTrigger(openTrigger);

            PlayOneShot(fastClip);

            OnDoorOpenedFast?.Invoke();
            StartCoroutine(Cooldown());
        }

        private void CloseDoorFast()
        {
            isOpen = false;

            if (doorAnimator != null)
            {
                if (!string.IsNullOrEmpty(closeTrigger))
                    doorAnimator.SetTrigger(closeTrigger);
                else if (!string.IsNullOrEmpty(openTrigger))
                    doorAnimator.ResetTrigger(openTrigger);
            }

            if (fastClip != null)
            {
                if (closeSoundDelay <= 0f) PlayOneShot(fastClip);
                else StartCoroutine(DelayedCloseSound(fastClip));
            }

            OnDoorClosedFast?.Invoke();
            StartCoroutine(Cooldown());
        }

        private void OpenDoorSlow()
        {
            isOpen = true;

            if (doorAnimator != null)
            {
                if (!string.IsNullOrEmpty(holdOpenTrigger))
                    doorAnimator.SetTrigger(holdOpenTrigger);
                else if (!string.IsNullOrEmpty(openTrigger))
                    doorAnimator.SetTrigger(openTrigger); // fallback
            }

            PlayOneShot(slowClip);

            OnDoorOpenedSlow?.Invoke();
            StartCoroutine(Cooldown());
        }

        private void CloseDoorSlow()
        {
            isOpen = false;

            if (doorAnimator != null)
            {
                if (!string.IsNullOrEmpty(holdCloseTrigger))
                    doorAnimator.SetTrigger(holdCloseTrigger);
                else if (!string.IsNullOrEmpty(closeTrigger))
                    doorAnimator.SetTrigger(closeTrigger);
                else if (!string.IsNullOrEmpty(openTrigger))
                    doorAnimator.ResetTrigger(openTrigger);
            }

            if (slowClip != null)
            {
                if (closeSoundDelay <= 0f) PlayOneShot(slowClip);
                else StartCoroutine(DelayedCloseSound(slowClip));
            }

            OnDoorClosedSlow?.Invoke();
            StartCoroutine(Cooldown());
        }

        // --- Audio helpers ---
        private void PlayOneShot(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        private IEnumerator DelayedCloseSound(AudioClip clip)
        {
            yield return new WaitForSeconds(closeSoundDelay);
            PlayOneShot(clip);
        }

        private IEnumerator Cooldown()
        {
            isCoolingDown = true;
            yield return new WaitForSeconds(cooldownTime);
            isCoolingDown = false;
        }

        // Optional helper if not using BoolFlag externally
        public void UnlockDoor() => isLocked = false;
    }
}
