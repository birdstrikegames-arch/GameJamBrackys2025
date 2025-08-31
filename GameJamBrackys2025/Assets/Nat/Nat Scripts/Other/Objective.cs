// File: Objective.cs
using UnityEngine;
using TMPro;

namespace Nat
{
    /// <summary>
    /// When the player enters this trigger:
    /// - Mode Create: Instantiates a note prefab (UI) under a Note Holder and sets its TMP text.
    /// - Mode Destroy: Finds note(s) in the Note Holder whose TMP text matches and destroys them.
    ///
    /// Notes:
    /// - The note prefab should be a UI object (with a TextMeshProUGUI somewhere in its hierarchy).
    /// - The Note Holder should be a Canvas/Panel (any Transform under a Canvas).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Objective : MonoBehaviour
    {
        public enum Mode { Create, Destroy }

        [Header("Mode")]
        [SerializeField] private Mode mode = Mode.Create;

        [Header("Player Detection")]
        [SerializeField] private string playerTag = "Player";

        [Header("Note Setup")]
        [Tooltip("Parent transform (Canvas/Panel) where notes will be placed/removed.")]
        [SerializeField] private Transform noteHolder;

        [Tooltip("UI prefab to spawn when Mode = Create. Must contain a TextMeshProUGUI in its children.")]
        [SerializeField] private GameObject notePrefab;

        [TextArea]
        [Tooltip("Message to assign to the spawned note, or to match when destroying.")]
        [SerializeField] private string message = "New Objective: Find a way out.";

        [Header("Destroy Options (used only in Destroy mode)")]
        [Tooltip("If true, destroy all matching notes; otherwise only the first match is destroyed.")]
        [SerializeField] private bool destroyAllMatches = false;

        [Tooltip("If false, the message match will ignore case.")]
        [SerializeField] private bool matchCase = true;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (noteHolder == null)
            {
                Debug.LogWarning($"[{nameof(Objective)}] Note Holder is not assigned.", this);
                return;
            }

            switch (mode)
            {
                case Mode.Create:
                    CreateNote();
                    break;

                case Mode.Destroy:
                    DestroyNotes();
                    break;
            }
        }

        private void CreateNote()
        {
            if (notePrefab == null)
            {
                Debug.LogWarning($"[{nameof(Objective)}] Note Prefab is not assigned for Create mode.", this);
                return;
            }

            // Instantiate under the holder, keeping UI RectTransform settings intact
            GameObject instance = Instantiate(notePrefab, noteHolder);
            var tmp = instance.GetComponentInChildren<TextMeshProUGUI>(true);

            if (tmp != null)
            {
                tmp.text = message;
            }
            else
            {
                Debug.LogWarning($"[{nameof(Objective)}] Spawned note prefab has no TextMeshProUGUI in children.", instance);
            }
        }

        private void DestroyNotes()
        {
            int destroyed = 0;

            // Iterate children of the holder and match on TMP text
            for (int i = noteHolder.childCount - 1; i >= 0; i--)
            {
                var child = noteHolder.GetChild(i);
                var tmp = child.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp == null) continue;

                if (IsMatch(tmp.text, message))
                {
                    Destroy(child.gameObject);
                    destroyed++;

                    if (!destroyAllMatches)
                        break; // only first match
                }
            }

            // Optional: log if nothing was found
            // if (destroyed == 0) Debug.Log($"[{nameof(Objective)}] No matching notes found to destroy.", this);
        }

        private bool IsMatch(string a, string b)
        {
            if (matchCase) return a == b;
            return string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
