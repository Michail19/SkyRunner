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

    private Resolution[] resolutions;

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

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
            resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void SetupQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    void SetupFullscreenToggle()
    {
        if (fullscreenToggle == null) return;

        fullscreenToggle.isOn = Screen.fullScreen;
    }

    public void SetSensitivity(float value)
    {
        GameSettings.mouseSensitivity = value;
        GameSettings.Save();
    }

    public void SetResolution(int index)
    {
        if (resolutions == null || resolutions.Length == 0) return;
        if (index < 0 || index >= resolutions.Length) return;

        Resolution selectedResolution = resolutions[index];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
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
