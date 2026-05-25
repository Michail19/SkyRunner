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

    private void Start()
    {
        Time.timeScale = 1f;
        GamePauseState.IsPaused = false;

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

    private void LateUpdate()
    {
        if (!isGameOver)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text = "Time: " + FormatTime(survivalTime);
    }

    private void GameOver()
    {
        ShowEndScreen("Game Over\nTime: " + FormatTime(survivalTime));
    }

    public void WinGame()
    {
        ShowEndScreen("Victory!\nTime: " + FormatTime(survivalTime));
    }

    private void ShowEndScreen(string message)
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        GamePauseState.IsPaused = true;

        if (resultText != null)
        {
            resultText.text = message;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        GamePauseState.IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        GamePauseState.IsPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Menu");
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        GamePauseState.IsPaused = false;

        Application.Quit();
        Debug.Log("Exit game");
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{minutes:00}:{seconds:00}";
    }

    public float GetSurvivalTime()
    {
        return survivalTime;
    }
}
