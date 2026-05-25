using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public float loseY = -5f;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;

    [Header("State")]
    public bool isGameOver;

    private float survivalTime;

    [Header("Disable On Game Over")]
    public MonoBehaviour[] scriptsToDisableOnGameOver;

    private void Start()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        survivalTime += Time.deltaTime;
        UpdateTimerUI();

        if (player != null && player.position.y < loseY)
        {
            GameOver();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text = "Время: " + FormatTime(survivalTime);
    }

    private void GameOver()
    {
        isGameOver = true;

        GamePauseState.IsPaused = true;

        if (scriptsToDisableOnGameOver != null)
        {
            foreach (MonoBehaviour script in scriptsToDisableOnGameOver)
            {
                if (script != null)
                {
                    script.enabled = false;
                }
            }
        }

        if (resultText != null)
        {
            resultText.text = "Вы продержались: " + FormatTime(survivalTime);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Over. Survival time: " + FormatTime(survivalTime));
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{minutes:00}:{seconds:00}";
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        GamePauseState.IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public float GetSurvivalTime()
    {
        return survivalTime;
    }
}
