using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioSource footstepsSound;
    public AudioClip[] footstepClips;

    void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (!footstepsSound.isPlaying)
            {
                PlayRandomFootstep();
            }
        }
        else
        {
            footstepsSound.Stop();
        }
    }

    void PlayRandomFootstep()
    {
        if (footstepClips.Length > 0)
        {
            int index = Random.Range(0, footstepClips.Length);
            footstepsSound.clip = footstepClips[index];
            footstepsSound.Play();
        }
    }
}
