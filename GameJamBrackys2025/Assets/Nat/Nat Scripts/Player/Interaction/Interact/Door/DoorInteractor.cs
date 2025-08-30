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
        [SerializeField] private float holdDurationSeconds = 1.5f; // hold time to trigger slow action
        [SerializeField] private string holdOpenTrigger = "OpenSlow";   // slow open trigger
        [SerializeField] private string holdCloseTrigger = "";          // optional slow close trigger (leave empty to use instant close)

        [Header("Locking")]
        [SerializeField] private BoolFlag unlockCondition;        // if assigned, overrides manual lock
        [SerializeField] private bool isLocked = false;           // used only when unlockCondition is null

        [Header("Cooldown")]
        [SerializeField] private float cooldownTime = 0.75f;
        private bool isCoolingDown = false;

        [Header("Sound")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private float closeSoundDelay = 0.0f;

        [Header("UI Text (fully manual)")]
        [SerializeField] private string descriptionWhenClosed = "Open [E] / Hold [E] for slow open";
        [SerializeField] private string descriptionWhenOpen   = "Close [E]";
        [SerializeField] private string descriptionWhenLocked = "Locked";

        // State
        [SerializeField] private bool isOpen = false;
        private bool isInteracting = false;  // prevents overlapping sessions

        // --- IInteractable ---
        public string GetDescription()
        {
            if (IsDoorLocked()) return descriptionWhenLocked;
            return isOpen ? descriptionWhenOpen : descriptionWhenClosed;
        }

        public void Interact()
        {
            if (isCoolingDown || isInteracting) return;

            // Start a short interaction session that decides: instant vs slow based on hold time.
            StartCoroutine(HandleInteractionSession());
        }

        // --- Interaction decision (tap vs hold) ---
        private IEnumerator HandleInteractionSession()
        {
            isInteracting = true;

            // If locked, bail (UI already shows "Locked")
            if (IsDoorLocked())
            {
                isInteracting = false;
                yield break;
            }

            float timer = 0f;

            // Wait while E is held, up to threshold. We use unscaled time so it also works if the game gets paused elsewhere.
            while (Input.GetKey(KeyCode.E))
            {
                // If something changed mid-hold (lock triggered or cooldown started), abort gracefully
                if (isCoolingDown || IsDoorLocked())
                {
                    isInteracting = false;
                    yield break;
                }

                timer += Time.unscaledDeltaTime;
                if (timer >= holdDurationSeconds) break;
                yield return null;
            }

            // Decide action
            bool didHoldLongEnough = timer >= holdDurationSeconds;

            if (didHoldLongEnough)
            {
                // SLOW path (open/close)
                if (!isOpen) OpenDoorSlow();
                else CloseDoorSlow();  // uses holdCloseTrigger if provided, else instant close fallback
            }
            else
            {
                // INSTANT path (open/close)
                if (!isOpen) OpenDoorInstant();
                else CloseDoorInstant();
            }

            isInteracting = false;
        }

        // --- Logic helpers ---
        private bool IsDoorLocked()
        {
            if (unlockCondition != null) return !unlockCondition.Value;
            return isLocked;
        }

        private void OpenDoorInstant()
        {
            isOpen = true;

            if (doorAnimator != null && !string.IsNullOrEmpty(openTrigger))
                doorAnimator.SetTrigger(openTrigger);

            PlayOneShot(openSound);
            StartCoroutine(Cooldown());
        }

        private void CloseDoorInstant()
        {
            isOpen = false;

            if (doorAnimator != null)
            {
                if (!string.IsNullOrEmpty(closeTrigger))
                    doorAnimator.SetTrigger(closeTrigger);
                else if (!string.IsNullOrEmpty(openTrigger))
                    doorAnimator.ResetTrigger(openTrigger); // fallback if you only animate one-way
            }

            if (closeSound != null)
            {
                if (closeSoundDelay <= 0f) PlayOneShot(closeSound);
                else StartCoroutine(DelayedCloseSound());
            }

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
                    doorAnimator.SetTrigger(openTrigger); // fallback to instant trigger if slow not set
            }

            PlayOneShot(openSound);
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

            if (closeSound != null)
            {
                if (closeSoundDelay <= 0f) PlayOneShot(closeSound);
                else StartCoroutine(DelayedCloseSound());
            }

            StartCoroutine(Cooldown());
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        private IEnumerator DelayedCloseSound()
        {
            yield return new WaitForSeconds(closeSoundDelay);
            PlayOneShot(closeSound);
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
