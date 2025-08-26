using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("loudness slider options")]
    public Slider loudnessSLider;
    public float sliderMax;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loudnessSLider.maxValue = sliderMax;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateLoudnessUI(float playerLoudness)
    {
        loudnessSLider.value = playerLoudness;
    }
}
