using UnityEngine;

public class BoltCutters : Interactable
{
    public AudioSource audioSource;
    public AudioClip interactSFX;
    public override void Interact()
    {
        audioSource.PlayOneShot(interactSFX);
        GameManager._instance.hasBoltCutters = true;
    }
}
