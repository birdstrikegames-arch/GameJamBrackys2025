// File: PlayerInteraction.cs
// Adds: after calling interactable.Interact(), auto-apply LoudInteraction generic if present (and not a door).
using UnityEngine;
using TMPro;

namespace Nat
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Camera mainCam;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private LayerMask interactMask = ~0;

        [Header("UI")]
        [SerializeField] private GameObject interactionUI;
        [SerializeField] private TextMeshProUGUI interactionText;

        private IInteractable current;
        private Collider currentCollider;

        void Awake()
        {
            if (mainCam == null) mainCam = Camera.main;
        }

        void Update()
        {
            // center ray
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    current = interactable;
                    currentCollider = hit.collider;

                    // show prompt
                    string desc = current.GetDescription();
                    bool show = !string.IsNullOrEmpty(desc);
                    if (interactionUI != null) interactionUI.SetActive(show);
                    if (show && interactionText != null) interactionText.text = desc;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        current.Interact();

                        // NEW: auto-apply generic loudness if a LoudInteraction is present and NOT a door
                        var loud = currentCollider.GetComponentInParent<LoudInteraction>();
                        if (loud != null && !loud.IsDoor)
                        {
                            loud.ApplyGeneric();
                        }
                    }

                    return;
                }
            }

            // nothing hit or no interactable
            current = null;
            currentCollider = null;
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }
}
