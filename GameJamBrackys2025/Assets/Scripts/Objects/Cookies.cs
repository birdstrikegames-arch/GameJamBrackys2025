using UnityEngine;

public class Cookies : Interactable
{
    public AudioSource audioSource;
    public AudioClip interactSFX;
    public override void Interact()
    {
        audioSource.PlayOneShot(interactSFX);
        GameManager._instance.hasCollectedCookies = true;
    }
}
