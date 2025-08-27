using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Audio stuff")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip openDoorSFX;
    [SerializeField]
    private AudioClip openDoorLoudSFX;
    [Header("loudness slider options")]
    public Slider loudnessSlider;
    public float sliderMax;

    [Header("look at thingies")]
    [SerializeField]
    private TextMeshProUGUI lookatText;

    [Header("DoorUI slider options")]
    public Slider openMeterSlider;
    public float maxHoldTime;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loudnessSlider.maxValue = sliderMax;
        UpdateDoorUI(0);


    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateLoudnessUI(float playerLoudness)
    {
        loudnessSlider.value = playerLoudness;
    }
    public void UpdateLookAtText(string text)
    {
        lookatText.text = text;
    }

    public void UpdateDoorUI(float heldCounter)
    {
        if (heldCounter == 0)
            openMeterSlider.gameObject.SetActive(false);
        else
            openMeterSlider.gameObject.SetActive(true);
        openMeterSlider.value = heldCounter;
    }
}
