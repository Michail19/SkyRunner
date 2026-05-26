using UnityEngine;

public enum GameDifficulty
{
    Easy,
    Normal,
    Hard
}

public static class GameSettings
{
    private const string MouseSensitivityKey = "MouseSensitivity";
    private const string MasterVolumeKey = "MasterVolume";
    private const string DifficultyKey = "Difficulty";

    public static float mouseSensitivity = 0.12f;
    public static float masterVolume = 1f;

    public static GameDifficulty difficulty = GameDifficulty.Normal;

    public static int itemsToCollect = 3;

    public static float collapseStartDelay = 5f;
    public static float warningTime = 1.5f;
    public static float pauseAfterCollapse = 1.5f;
    public static int tilesPerWave = 1;

    public static float botSpawnInterval = 8f;
    public static int maxAliveBots = 4;
    public static float botMoveSpeedMultiplier = 1f;

    public static void ApplyDifficulty(GameDifficulty selectedDifficulty)
    {
        difficulty = selectedDifficulty;

        switch (difficulty)
        {
            case GameDifficulty.Easy:
                itemsToCollect = 2;

                collapseStartDelay = 8f;
                warningTime = 2f;
                pauseAfterCollapse = 2f;
                tilesPerWave = 1;

                botSpawnInterval = 10f;
                maxAliveBots = 2;
                botMoveSpeedMultiplier = 0.85f;
                break;

            case GameDifficulty.Normal:
                itemsToCollect = 3;

                collapseStartDelay = 5f;
                warningTime = 1.5f;
                pauseAfterCollapse = 1.5f;
                tilesPerWave = 1;

                botSpawnInterval = 8f;
                maxAliveBots = 4;
                botMoveSpeedMultiplier = 1f;
                break;

            case GameDifficulty.Hard:
                itemsToCollect = 5;

                collapseStartDelay = 3f;
                warningTime = 1.1f;
                pauseAfterCollapse = 0.8f;
                tilesPerWave = 2;

                botSpawnInterval = 5f;
                maxAliveBots = 7;
                botMoveSpeedMultiplier = 1.15f;
                break;
        }
    }

    public static void ApplyDifficultyIndex(int difficultyIndex)
    {
        int normalizedIndex = Mathf.Clamp(
            difficultyIndex,
            (int)GameDifficulty.Easy,
            (int)GameDifficulty.Hard
        );

        ApplyDifficulty((GameDifficulty)normalizedIndex);
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat(MouseSensitivityKey, mouseSensitivity);
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.SetInt(DifficultyKey, (int)difficulty);
        PlayerPrefs.Save();
    }

    public static void Load()
    {
        mouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, 0.12f);
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);

        int difficultyIndex = PlayerPrefs.GetInt(DifficultyKey, (int)GameDifficulty.Normal);
        ApplyDifficultyIndex(difficultyIndex);
    }
}
