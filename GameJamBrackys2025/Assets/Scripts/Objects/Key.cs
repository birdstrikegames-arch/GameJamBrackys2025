using UnityEngine;

public class Key : Interactable
{
    public AudioSource audioSource;
    public AudioClip interactSFX;
    public override void Interact()
    {
        audioSource.PlayOneShot(interactSFX);
        GameManager._instance.hasKey = true;
        Destroy(gameObject);
    }


}
