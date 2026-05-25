using UnityEngine;

public enum GameDifficulty
{
    Easy,
    Normal,
    Hard
}

public static class GameSettings
{
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
                maxAliveBots = 3;
                botMoveSpeedMultiplier = 0.9f;
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
                itemsToCollect = 4;

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

    public static void Save()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetInt("Difficulty", (int)difficulty);
        PlayerPrefs.Save();
    }

    public static void Load()
    {
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 0.12f);
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        int difficultyIndex = PlayerPrefs.GetInt("Difficulty", (int)GameDifficulty.Normal);
        difficulty = (GameDifficulty)difficultyIndex;

        ApplyDifficulty(difficulty);
    }
}
