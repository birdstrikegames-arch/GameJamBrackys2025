// File: Win.cs
using UnityEngine;

namespace Nat
{
    /// <summary>
    /// Interactable that pauses the game and shows a Win panel when used.
    /// - Customizable interaction text.
    /// - Plays a sound (optional).
    /// - Pauses the game, enables the provided win panel,
    ///   and locks GameManager's pause input (no ESC toggling).
    /// </summary>
    public class Win : MonoBehaviour, IInteractable
    {
        [Header("UI")]
        [SerializeField] private GameObject winPanelRoot;   // Canvas element to enable

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip winClip;

        [Header("Interaction Text")]
        [SerializeField] private string description = "Win [E]";

        private bool triggered = false;

        public string GetDescription()
        {
            return triggered ? string.Empty : description;
        }

        public void Interact()
        {
            if (triggered) return;
            triggered = true;

            // Play sound first
            if (audioSource != null && winClip != null)
                audioSource.PlayOneShot(winClip);

            // Show Win UI
            if (winPanelRoot != null)
                winPanelRoot.SetActive(true);

            // Lock GameManager pause input so ESC does nothing during win
            GameManager.Instance?.SetPauseInputLocked(true);

            // Pause game & show cursor
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
