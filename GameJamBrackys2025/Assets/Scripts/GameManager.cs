using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance { get; private set; }
    public GameObject player;
    public PlayerUI playerUI;

    public float loudnessLevel;

    public bool hasKey;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(gameObject);
        else
        {
            _instance = this;
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        playerUI.GetComponent<PlayerUI>().UpdateLoudnessUI(loudnessLevel);
        
    }
    

    public void IncreaseLoudness(float level)
    {
        loudnessLevel += level;
    }


}
