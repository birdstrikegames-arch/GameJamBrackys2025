using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField]
    private float rayLength;
    [SerializeField]
    private LayerMask interactableLayer;

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward * rayLength);
        if (Physics.Raycast(ray, rayLength, interactableLayer))
        {

        }
    }
}
