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

        timerText.text = "Время: " + FormatTime(survivalTime);
    }

    private void GameOver()
    {
        ShowEndScreen("Поражение\nВремя: " + FormatTime(survivalTime), false);
    }

    public void WinGame()
    {
        ShowEndScreen("Победа!\nВремя: " + FormatTime(survivalTime), true);
    }

    private void ShowEndScreen(string message, bool isVictory)
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

        if (AudioManager.Instance != null)
        {
            AudioClip clip = isVictory
                ? AudioManager.Instance.victoryClip
                : AudioManager.Instance.gameOverClip;

            AudioManager.Instance.PlaySfx(clip, 1f);
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
        Debug.Log("Выход из игры");
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
