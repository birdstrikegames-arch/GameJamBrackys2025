using UnityEngine;

namespace Nat
{
    public class InteractionDisable : MonoBehaviour, IInteractable
    {
        [Header("UI")]
        [SerializeField] private string interactionText = "Interact";

        [Header("Target Flag")]
        [SerializeField] private BoolFlag flagToSet;

        [Header("Sound")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip soundToPlay;

        public string GetDescription()
        {
            return interactionText;
        }

        public void Interact()
        {
            if (flagToSet != null)
            {
                flagToSet.Value = true;
                Debug.Log("Flag set to TRUE: " + flagToSet.name);
            }

            if (audioSource != null && soundToPlay != null)
                audioSource.PlayOneShot(soundToPlay);

            gameObject.SetActive(false);
        }
    }
}