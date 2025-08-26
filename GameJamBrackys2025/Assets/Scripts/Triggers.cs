using UnityEngine;

public class Triggers : MonoBehaviour
{
    [SerializeField]
    private float loudnessLevel;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip sound;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager._instance.IncreaseLoudness(loudnessLevel);
            if (sound == null || audioSource == null)
                audioSource.PlayOneShot(sound);
        }
    }
}
