using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance { get; private set; }
    public GameObject player;
    public PlayerUI playerUI;

    private float loudnessLevel;
    public float loudnessDecreaseSpeed;

    public bool hasKey;
    public bool hasBoltCutters;
    public bool hasBroom;

    public bool hasLost;
    public bool hasCollectedCookies;

    public GameObject winPanel;

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

        if (loudnessLevel > 0)
            loudnessLevel -= loudnessDecreaseSpeed * Time.deltaTime;
        if (loudnessLevel <= 0)
            loudnessLevel = 0;

        if (hasCollectedCookies)
            GameWin();

        if (hasLost)
            GameLost();


    }


    public void IncreaseLoudness(float level)
    {
        loudnessLevel += level;
    }

    private void GameWin()
    {
        winPanel.SetActive(true);
        player.SetActive(false);
    }
    private void GameLost()
    {
        Debug.Log("game lost");
    }

    

}
