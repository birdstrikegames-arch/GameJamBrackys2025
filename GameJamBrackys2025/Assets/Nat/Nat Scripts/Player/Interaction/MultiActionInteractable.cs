// File: MultiActionInteractable.cs
using UnityEngine;
using TMPro;

namespace Nat
{
    /// <summary>
    /// Generic interactable with toggleable actions.
    /// Tick the booleans to choose which actions run when the player presses E.
    ///
    /// Actions supported:
    /// - Play a sound (AudioSource + Clip)
    /// - Disable a GameObject
    /// - Enable a GameObject
    /// - Create an Objective note (UI prefab under a holder, with TMP text)
    /// - Destroy an Objective note by message
    /// - Disable an Objective (by message)  <-- NEW
    /// - Enable an Objective (by message)   <-- NEW
    ///
    /// Notes:
    /// - Interaction text is fully customizable via inspector.
    /// - If both 'Create Objective' and 'Destroy Objective' are ticked, Destroy runs first.
    /// - Use 'Replayable' to make this a one-shot interaction or allow repeats.
    /// </summary>
    public class MultiActionInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction UI")]
        [SerializeField] private string interactionText = "Interact [E]";
        [Tooltip("If false, this interaction only works once; afterwards it won't fire and no text is shown.")]
        [SerializeField] private bool replayable = true;

        [Header("Play Sound")]
        [SerializeField] private bool playSound = false;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip soundClip;

        [Header("Disable Object (GameObject reference)")]
        [SerializeField] private bool disableObject = false;
        [SerializeField] private GameObject objectToDisable;

        [Header("Enable Object (GameObject reference)")]
        [SerializeField] private bool enableObject = false;
        [SerializeField] private GameObject objectToEnable;

        [Header("Objective Holder / Prefab (shared)")]
        [Tooltip("Parent Transform (Canvas/Panel) where notes are placed/found.")]
        [SerializeField] private Transform noteHolder;
        [Tooltip("UI prefab that contains a TextMeshProUGUI in its children (used for Create Objective).")]
        [SerializeField] private GameObject notePrefab;

        [Header("Objective (Create)")]
        [SerializeField] private bool createObjective = false;
        [TextArea]
        [SerializeField] private string objectiveMessage = "New Objective";
        [Tooltip("Avoid spawning another note with the same message.")]
        [SerializeField] private bool preventDuplicateByMessage = true;
        [Tooltip("If false, message comparison when checking duplicates ignores case.")]
        [SerializeField] private bool matchCaseForCreate = true;

        [Header("Objective (Destroy)")]
        [SerializeField] private bool destroyObjective = false;
        [TextArea]
        [SerializeField] private string objectiveMessageToDestroy = "New Objective";
        [Tooltip("Destroy all matching notes; if false, destroys only the first match.")]
        [SerializeField] private bool destroyAllMatches = false;
        [Tooltip("If false, message comparison ignores case.")]
        [SerializeField] private bool matchCaseForDestroy = true;

        [Header("Objective (Disable by message)")]
        [SerializeField] private bool disableObjectiveByMessage = false;  // NEW
        [TextArea]
        [SerializeField] private string objectiveMessageToDisable = "Objective";
        [Tooltip("If false, message comparison ignores case.")]
        [SerializeField] private bool matchCaseForDisableObjective = true;
        [Tooltip("Disable all matches; if false, disables only the first match found.")]
        [SerializeField] private bool disableAllMatchingObjectives = false;

        [Header("Objective (Enable by message)")]
        [SerializeField] private bool enableObjectiveByMessage = false;   // NEW
        [TextArea]
        [SerializeField] private string objectiveMessageToEnable = "Objective";
        [Tooltip("If false, message comparison ignores case.")]
        [SerializeField] private bool matchCaseForEnableObjective = true;
        [Tooltip("Enable all matches; if false, enables only the first match found.")]
        [SerializeField] private bool enableAllMatchingObjectives = false;

        private bool consumed = false;

        // --- IInteractable ---
        public string GetDescription()
        {
            if (consumed && !replayable) return string.Empty;
            return interactionText;
        }

        public void Interact()
        {
            if (consumed && !replayable) return;

            // 1) Play sound
            if (playSound && audioSource != null && soundClip != null)
            {
                audioSource.PlayOneShot(soundClip);
            }

            // 2) Objective destructive ops first (so create can "replace")
            if (destroyObjective)
            {
                TryDestroyObjective();
            }

            // 3) Objective state toggles (by message)
            if (disableObjectiveByMessage)
            {
                TrySetObjectiveActiveByMessage(
                    objectiveMessageToDisable,
                    activeState: false,
                    affectAll: disableAllMatchingObjectives,
                    matchCase: matchCaseForDisableObjective
                );
            }

            if (enableObjectiveByMessage)
            {
                TrySetObjectiveActiveByMessage(
                    objectiveMessageToEnable,
                    activeState: true,
                    affectAll: enableAllMatchingObjectives,
                    matchCase: matchCaseForEnableObjective
                );
            }

            // 4) Objective create
            if (createObjective)
            {
                TryCreateObjective();
            }

            // 5) Enable/Disable GameObjects (direct refs)
            if (disableObject && objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }

            if (enableObject && objectToEnable != null)
            {
                objectToEnable.SetActive(true);
            }

            // 6) Consumption
            if (!replayable)
            {
                consumed = true;
            }
        }

        // --- Objective helpers ---
        private void TryCreateObjective()
        {
            if (noteHolder == null || notePrefab == null)
            {
                Debug.LogWarning($"[MultiActionInteractable] CreateObjective: Missing Note Holder or Note Prefab on '{name}'.");
                return;
            }

            if (preventDuplicateByMessage)
            {
                for (int i = 0; i < noteHolder.childCount; i++)
                {
                    var child = noteHolder.GetChild(i);
                    var tmp = child.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (tmp != null && IsMatch(tmp.text, objectiveMessage, matchCaseForCreate))
                    {
                        // already present; do nothing
                        return;
                    }
                }
            }

            var instance = Instantiate(notePrefab, noteHolder);
            var text = instance.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.text = objectiveMessage;
            }
            else
            {
                Debug.LogWarning($"[MultiActionInteractable] Note Prefab for '{name}' has no TextMeshProUGUI in children.");
            }
        }

        private void TryDestroyObjective()
        {
            if (noteHolder == null)
            {
                Debug.LogWarning($"[MultiActionInteractable] DestroyObjective: Missing Note Holder on '{name}'.");
                return;
            }

            int destroyed = 0;
            for (int i = noteHolder.childCount - 1; i >= 0; i--)
            {
                var child = noteHolder.GetChild(i);
                var tmp = child.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp == null) continue;

                if (IsMatch(tmp.text, objectiveMessageToDestroy, matchCaseForDestroy))
                {
                    Destroy(child.gameObject);
                    destroyed++;
                    if (!destroyAllMatches)
                        break;
                }
            }
        }

        private void TrySetObjectiveActiveByMessage(string targetMessage, bool activeState, bool affectAll, bool matchCase)
        {
            if (noteHolder == null)
            {
                Debug.LogWarning($"[MultiActionInteractable] {(activeState ? "Enable" : "Disable")}Objective: Missing Note Holder on '{name}'.");
                return;
            }

            // includeInactive = true so we can enable notes that are currently inactive
            var tmps = noteHolder.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
            int changed = 0;

            foreach (var tmp in tmps)
            {
                if (tmp == null) continue;
                if (!IsMatch(tmp.text, targetMessage, matchCase)) continue;

                // Toggle the note's root (assumes TMP sits under the note prefab)
                var noteGO = tmp.transform.root == noteHolder.root
                    ? tmp.transform.gameObject
                    : tmp.transform.parent != null ? tmp.transform.parent.gameObject : tmp.gameObject;

                // Safer: set the topmost child under the holder as the target
                Transform top = tmp.transform;
                while (top.parent != null && top.parent != noteHolder)
                    top = top.parent;

                if (top != null)
                {
                    top.gameObject.SetActive(activeState);
                    changed++;
                }

                if (!affectAll) break;
            }

            // Optional: if (changed == 0) Debug.Log($"[MultiActionInteractable] No objective found to {(activeState ? "enable" : "disable")} with message '{targetMessage}'.");
        }

        private bool IsMatch(string a, string b, bool matchCase)
        {
            if (matchCase) return a == b;
            return string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
