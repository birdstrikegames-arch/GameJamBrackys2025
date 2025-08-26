using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance { get; private set; }
    public GameObject player;

    public float loudnessLevel;

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
        player.GetComponent<PlayerUI>().updateLoudnessUI(loudnessLevel);
        
    }
    

    public void IncreaseLoudness(float level)
    {
        loudnessLevel += level;
    }


}
