using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [Tooltip("Minimum intensity of the light.")]
    public float minIntensity = 0.5f;

    [Tooltip("Maximum intensity of the light.")]
    public float maxIntensity = 1.5f;

    [Tooltip("Speed of the flickering effect.")]
    public float flickerSpeed = 10f;

    private Light spotLight;

    void Start()
    {
        spotLight = GetComponent<Light>();
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        spotLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}