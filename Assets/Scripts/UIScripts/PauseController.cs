using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [Header("References")]
    public GameObject pausePanel;
    public GameManager gameManager;

    private bool isPaused;
    private InputAction pauseAction;

    private void Awake()
    {
        pauseAction = new InputAction(
            "Pause",
            InputActionType.Button,
            "<Keyboard>/escape"
        );
    }

    private void OnEnable()
    {
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.Disable();
    }

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("PauseController: Pause Panel is not assigned.", this);
        }

        isPaused = false;
        GamePauseState.IsPaused = false;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (gameManager != null && gameManager.isGameOver)
        {
            return;
        }

        if (pauseAction.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;

        isPaused = true;
        GamePauseState.IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        isPaused = false;
        GamePauseState.IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        isPaused = false;
        GamePauseState.IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;

        isPaused = false;
        GamePauseState.IsPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Menu");
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

        isPaused = false;
        GamePauseState.IsPaused = false;

        Application.Quit();
        Debug.Log("Exit game");
    }
}
