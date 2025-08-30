using UnityEngine;

public class Broom : Interactable
{
    public AudioSource audioSource;
    public AudioClip interactSFX;
    public override void Interact()
    {
        audioSource.PlayOneShot(interactSFX);
        GameManager._instance.hasBroom = true;
        Destroy(gameObject);
    }
}
