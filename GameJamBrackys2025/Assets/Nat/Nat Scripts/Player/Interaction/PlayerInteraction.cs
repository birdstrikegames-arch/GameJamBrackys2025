using System.Collections;
using UnityEngine;
using TMPro;

namespace Nat
{
    public class PlayerInteraction : MonoBehaviour
    {
        public Camera mainCam;
        public float interactionDistance = 2f;

        public GameObject interactionUI;
        public TextMeshProUGUI interactionText;

        private void Update()
        {
            InteractionRay();
        }

        void InteractionRay()
        {
            Ray ray = mainCam.ViewportPointToRay(Vector3.one / 2f);
            RaycastHit hit;

            bool hitSomething = false;

            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    string description = interactable.GetDescription();
                    bool show = !string.IsNullOrEmpty(description);

                    if (show)
                    {
                        interactionText.text = description;
                        interactionUI.SetActive(true);
                    }
                    else
                    {
                        interactionUI.SetActive(false);
                    }

                    hitSomething = show;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactable.Interact();
                    }
                }
            }

            if (!hitSomething)
            {
                interactionUI.SetActive(false);
            }
        }
    }
}