using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
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
        ray = new Ray(cameraPOV.transform.position, cameraPOV.transform.forward);
        if (Physics.Raycast(ray, out hit, rayLength, interactableLayer))
        {
            playerUI.UpdateLookAtText(hit.transform.gameObject.name);
            if (playerInput.InputMap.Player.Interact.IsPressed())
            {
                Interactable interaction = hit.collider.GetComponent<Interactable>();
                if(interaction != null)
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
