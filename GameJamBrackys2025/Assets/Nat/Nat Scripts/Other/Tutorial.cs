// File: Tutorial.cs
using UnityEngine;
using TMPro;

namespace Nat
{
    /// <summary>
    /// Enables a TMP object and shows a custom message when the player enters a trigger.
    /// - Drag a TextMeshProUGUI into 'tutorialText' in the Inspector.
    /// - Set the 'message' you want displayed.
    /// - Optionally hide the text again on exit with 'hideOnExit'.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Tutorial : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI tutorialText;
        [TextArea]
        [SerializeField] private string message = "Use WASD to move.";

        [Header("Trigger Settings")]
        [SerializeField] private string playerTag = "Player";
        [Tooltip("If true, hides the TMP object again when the player exits the trigger.")]
        [SerializeField] private bool hideOnExit = false;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            if (tutorialText != null)
            {
                tutorialText.text = message;
                tutorialText.gameObject.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (!hideOnExit) return;

            if (tutorialText != null)
            {
                tutorialText.gameObject.SetActive(false);
            }
        }
    }
}
