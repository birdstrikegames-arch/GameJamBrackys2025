// File: CarbonMonoxide.cs
using UnityEngine;

namespace Nat
{
    /// <summary>
    /// Trigger area that reduces oxygen while the player is inside.
    /// The depletion speed (per second) is set in the inspector.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CarbonMonoxide : MonoBehaviour
    {
        [Header("Oxygen")]
        [SerializeField] private OxygenManager oxygenManager;
        [Tooltip("How much oxygen to drain per second while the player is inside this trigger.")]
        [SerializeField] private float drainPerSecond = 10f;

        [Header("Trigger")]
        [Tooltip("Player tag to detect on enter/exit.")]
        [SerializeField] private string playerTag = "Player";

        // We use ourselves as the unique key when registering with the manager
        private bool isRegistered = false;

        private void Reset()
        {
            // Ensure collider is set as trigger
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Awake()
        {
            if (oxygenManager == null)
                oxygenManager = FindObjectOfType<OxygenManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (oxygenManager == null) return;

            oxygenManager.RegisterDrain(this, Mathf.Max(0f, drainPerSecond));
            isRegistered = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (oxygenManager == null) return;

            oxygenManager.UnregisterDrain(this);
            isRegistered = false;
        }

        private void OnDisable()
        {
            // Safety: if this zone is disabled while the player is inside, remove the drain.
            if (isRegistered && oxygenManager != null)
            {
                oxygenManager.UnregisterDrain(this);
                isRegistered = false;
            }
        }
    }
}
