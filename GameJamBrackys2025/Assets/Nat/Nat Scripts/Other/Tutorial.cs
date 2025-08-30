using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [Tooltip("The player's collider (e.g., CharacterController or CapsuleCollider).")]
    public Collider playerCollider;

    [Tooltip("The UI element to show while the player is in the trigger.")]
    public GameObject tutorialUI;

    private void Start()
    {
        if (tutorialUI != null)
            tutorialUI.SetActive(false); // Hide by default
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == playerCollider)
        {
            if (tutorialUI != null)
                tutorialUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == playerCollider)
        {
            if (tutorialUI != null)
                tutorialUI.SetActive(false);
        }
    }
}
