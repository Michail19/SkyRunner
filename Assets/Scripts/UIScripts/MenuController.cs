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

    private Resolution[] resolutions;

    void Start()
    {
        Time.timeScale = 1f;
        GamePauseState.IsPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetupSensitivity();
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
}
