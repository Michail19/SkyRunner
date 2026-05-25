using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject settingsPanel;

    public Slider sensitivitySlider;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;

    public TMP_Dropdown difficultyDropdown;
    public Slider volumeSlider;

    public GameObject instructionsPanel;

    private List<Vector2Int> availableResolutions = new List<Vector2Int>();

    void Start()
    {
        GameSettings.Load();

        Time.timeScale = 1f;
        GamePauseState.IsPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        SetupSensitivity();
        SetupVolume();
        SetupDifficulty();
        SetupResolutionDropdown();
        SetupQualityDropdown();
        SetupFullscreenToggle();
    }

    void SetupSensitivity()
    {
        if (sensitivitySlider == null) return;

        sensitivitySlider.value = GameSettings.mouseSensitivity;
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        availableResolutions.Clear();
        resolutionDropdown.ClearOptions();

        Resolution[] unityResolutions = Screen.resolutions;

        foreach (Resolution resolution in unityResolutions)
        {
            AddResolutionIfUnique(resolution.width, resolution.height);
        }

        // Fallback, если Unity в билде дала только 640x480 или пустой список.
        if (availableResolutions.Count <= 1)
        {
            AddResolutionIfUnique(1280, 720);
            AddResolutionIfUnique(1366, 768);
            AddResolutionIfUnique(1600, 900);
            AddResolutionIfUnique(1920, 1080);
            AddResolutionIfUnique(2560, 1440);
        }

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Vector2Int resolution = availableResolutions[i];
            options.Add(resolution.x + "x" + resolution.y);

            if (resolution.x == Screen.width && resolution.y == Screen.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void AddResolutionIfUnique(int width, int height)
    {
        if (width < 800 || height < 600)
        {
            return;
        }

        Vector2Int newResolution = new Vector2Int(width, height);

        if (!availableResolutions.Contains(newResolution))
        {
            availableResolutions.Add(newResolution);
        }
    }

    public void SetResolution(int index)
    {
        if (availableResolutions == null || availableResolutions.Count == 0) return;
        if (index < 0 || index >= availableResolutions.Count) return;

        Vector2Int selectedResolution = availableResolutions[index];

        Screen.SetResolution(
            selectedResolution.x,
            selectedResolution.y,
            Screen.fullScreen
        );
    }

    void SetupQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        qualityDropdown.RefreshShownValue();

        qualityDropdown.onValueChanged.RemoveListener(SetQuality);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    void SetupFullscreenToggle()
    {
        if (fullscreenToggle == null) return;

        fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);

        fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetSensitivity(float value)
    {
        GameSettings.mouseSensitivity = value;
        GameSettings.Save();
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level");
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Выход из игры");
    }

    void SetupVolume()
    {
        if (volumeSlider == null) return;

        volumeSlider.value = GameSettings.masterVolume;
        volumeSlider.onValueChanged.AddListener(SetVolume);

        AudioListener.volume = GameSettings.masterVolume;
    }

    void SetupDifficulty()
    {
        if (difficultyDropdown == null) return;

        difficultyDropdown.ClearOptions();

        difficultyDropdown.AddOptions(new List<string>
    {
        "Лёгкая",
        "Обычная",
        "Сложная"
    });

        difficultyDropdown.value = (int)GameSettings.difficulty;
        difficultyDropdown.RefreshShownValue();

        difficultyDropdown.onValueChanged.AddListener(SetDifficulty);
    }

    public void SetVolume(float value)
    {
        GameSettings.masterVolume = value;
        AudioListener.volume = value;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyVolume();
        }

        GameSettings.Save();
    }

    public void SetDifficulty(int index)
    {
        GameDifficulty selectedDifficulty = (GameDifficulty)index;
        GameSettings.ApplyDifficulty(selectedDifficulty);
        GameSettings.Save();
    }

    public void OpenInstructions()
    {
        mainPanel.SetActive(false);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }
    }

    public void CloseInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        mainPanel.SetActive(true);
    }
}
