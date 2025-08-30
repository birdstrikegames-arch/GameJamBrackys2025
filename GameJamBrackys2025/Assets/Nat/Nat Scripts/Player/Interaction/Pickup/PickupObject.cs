using UnityEngine;

namespace Nat
{
    public class PickupObject : MonoBehaviour, IInteractable
    {
        private Rigidbody rb;
        private bool isHeld = false;
        private Transform hand;
        private float throwForce = 10f;
        private string originalLayer;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            originalLayer = LayerMask.LayerToName(gameObject.layer);
        }

        void Update()
        {
            if (!isHeld) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                Drop();
            }

            if (Input.GetMouseButtonDown(0))
            {
                Throw();
            }
        }

        public string GetDescription()
        {
            return isHeld ? null : "Pick up";
        }

        public void Interact()
        {
            if (isHeld) return;

            if (hand == null)
            {
                var cam = Camera.main;
                if (cam != null)
                    hand = cam.transform.Find("Hand");
            }

            if (hand == null) return;

            isHeld = true;

            rb.isKinematic = true;
            rb.useGravity = false;

            transform.SetParent(hand);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Assign to Pickup layer so the PickupCamera sees it
            gameObject.layer = LayerMask.NameToLayer("Pickup");
        }

        private void Drop()
        {
            isHeld = false;

            transform.SetParent(null);
            rb.isKinematic = false;
            rb.useGravity = true;

            // Restore original layer
            gameObject.layer = LayerMask.NameToLayer(originalLayer);
        }

        private void Throw()
        {
            isHeld = false;

            transform.SetParent(null);
            rb.isKinematic = false;
            rb.useGravity = true;

            rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);

            // Restore original layer
            gameObject.layer = LayerMask.NameToLayer(originalLayer);
        }
    }
}