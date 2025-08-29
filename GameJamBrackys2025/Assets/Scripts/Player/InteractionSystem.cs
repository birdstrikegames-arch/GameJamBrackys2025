/*using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public static Door FocusedDoor;

    [SerializeField]
    private PlayerUI playerUI;
    [SerializeField]
    private InputManager playerInput;
    [Header("ray settings")]
    [SerializeField]
    private GameObject cameraPOV;
    [SerializeField]
    private float rayLength;
    [SerializeField]
    private LayerMask interactableLayer;

    private Ray ray;
    private RaycastHit hit;


    private void Update()
    {
        FocusedDoor = null;

        ray = new Ray(cameraPOV.transform.position, cameraPOV.transform.forward);
        if (Physics.Raycast(ray, out hit, rayLength, interactableLayer))
        {
            playerUI.UpdateLookAtText(hit.transform.gameObject.name);
            if (playerInput.InputMap.Player.Interact.IsPressed())
            {
                Interactable interaction = hit.collider.GetComponent<Interactable>();
                if (hit.collider.TryGetComponent(out Door door))
                    FocusedDoor = door;
                if (interaction != null && !hit.collider.GetComponent<Door>().animator.GetBool("isOpen"))
                {
                    interaction.Interact();
                }
            }
        }
        else
        {
            playerUI.UpdateLookAtText("");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(cameraPOV.transform.position, cameraPOV.transform.forward * rayLength);
    }
}
*/

using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    // What the crosshair is on this frame
    public static Door FocusedDoor { get; private set; }
    // The door we latched when the button was pressed (stays set until release)
    public static Door ActiveDoor { get; private set; }

    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private InputManager playerInput;

    [Header("ray settings")]
    [SerializeField] private GameObject cameraPOV;
    [SerializeField] private float rayLength = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private Ray ray;
    private RaycastHit hit;

    // Track whether the Hold interaction reached its duration
    private bool holdCompleted;

    void OnEnable()
    {
        var act = playerInput.InputMap.Player.Interact;   // MUST have a Hold interaction set in the Input Actions
        act.started += OnInteractStarted;   // pressed
        act.performed += OnInteractPerformed; // hold time reached
        act.canceled += OnInteractCanceled;  // released
        act.Enable();
    }

    void OnDisable()
    {
        var act = playerInput.InputMap.Player.Interact;
        act.started -= OnInteractStarted;
        act.performed -= OnInteractPerformed;
        act.canceled -= OnInteractCanceled;
        act.Disable();
    }

    void Update()
    {
        // 1) Aim raycast → FocusedDoor + UI hover text
        FocusedDoor = null;

        ray = new Ray(cameraPOV.transform.position, cameraPOV.transform.forward);
        if (Physics.Raycast(ray, out hit, rayLength, interactableLayer))
        {
            playerUI.UpdateLookAtText(hit.transform.gameObject.name);

            if (hit.collider.TryGetComponent(out Door door))
                FocusedDoor = door;
        }
        else
        {
            playerUI.UpdateLookAtText("");
        }

        // (Optional): If you want a “charging bar” while holding, you can
        // compute a visual fill here using interact.IsPressed() and feed your UI.
        // The door's logic will still be finalized by started/performed/canceled.
    }

    // ===== Input callbacks =====

    private void OnInteractStarted(InputAction.CallbackContext _)
    {
        // Latch the door you’re looking at on press
        if (FocusedDoor == null) return;
        if (FocusedDoor.animator != null && FocusedDoor.animator.GetBool("isOpen")) return;

        ActiveDoor = FocusedDoor;
        holdCompleted = false;

        // Tell the door to begin its hold (this is your existing Interact() entry point)
        ActiveDoor.Interact();
    }

    private void OnInteractPerformed(InputAction.CallbackContext _)
    {
        // We reached the Hold duration; we’ll still actually “finish” on release
        holdCompleted = true;
    }

    private void OnInteractCanceled(InputAction.CallbackContext _)
    {
        if (ActiveDoor == null) return;

        // If your Door script already handles release in its Update()
        // (checking WasReleasedThisFrame), it will now be allowed to run
        // because you'll add the ActiveDoor guard mentioned at the top.

        // If you later expose a method like ActiveDoor.EndHold(early),
        // you can call it here directly:
        // bool early = !holdCompleted;
        // ActiveDoor.EndHold(early);

        ActiveDoor = null;
        holdCompleted = false;
    }

    private void OnDrawGizmos()
    {
        if (!cameraPOV) return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(cameraPOV.transform.position, cameraPOV.transform.forward * rayLength);
    }
}