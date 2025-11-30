using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
   
    [Header("Game State")]
    public int currentScore = 0;
    public bool isGamePaused = false;

    [Header("UI Game")]
    public GameObject pauseMenuUI;
    public TMP_Text scoreText;
    public Slider HPBar;

    private void Awake()
    {
       if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ------------------- Singleton Functionality -------------------

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
       if (HPBar != null)
        {
            HPBar.value = currentHealth;
            HPBar.maxValue = maxHealth;
        }
    }
    public void AddScore(int amount)
    {
        currentScore += amount;
        scoreText.text = currentScore.ToString();
        Debug.Log("score updated :: " + currentScore);
    }

    public void TogglePause()
    {
       
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
}