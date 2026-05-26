using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCollapseManager : MonoBehaviour
{
    [Header("References")]
    public ArenaGenerator arenaGenerator;
    public Transform player;

    [Header("Collapse Settings")]
    public float startDelay = 5f;
    public float warningTime = 1.5f;
    public float pauseAfterCollapse = 1.5f;
    public int tilesPerWave = 1;

    [Header("Difficulty Growth")]
    public bool increaseDifficulty = true;
    public float difficultyStepTime = 15f;
    public int maxTilesPerWave = 4;
    public float minPauseAfterCollapse = 0.5f;

    [Header("Safety")]
    public float minDistanceFromPlayer = 3f;

    [Header("Auto Scale")]
    public bool autoScaleWithArena = true;
    public int minTilesPerWave = 1;
    public int maxTilesPerWaveLimit = 10;

    private bool isRunning;
    private float difficultyTimer;

    private void Start()
    {
        GameSettings.Load();
        ApplyGameSettings();
        StartCollapse();
    }

    private void ApplyGameSettings()
    {
        float arenaScale = GetArenaScale();

        startDelay = GameSettings.collapseStartDelay;
        warningTime = GameSettings.warningTime;
        pauseAfterCollapse = GameSettings.pauseAfterCollapse;

        tilesPerWave = autoScaleWithArena
            ? Mathf.Clamp(
                Mathf.RoundToInt(GameSettings.tilesPerWave * arenaScale),
                minTilesPerWave,
                maxTilesPerWaveLimit
            )
            : GameSettings.tilesPerWave;

        tilesPerWave = Mathf.Max(1, tilesPerWave);
        maxTilesPerWave = GetDifficultyMaxTilesPerWave(arenaScale);
        minPauseAfterCollapse = GetDifficultyMinPause();
        difficultyStepTime = GetDifficultyStepTime(arenaScale);
        minDistanceFromPlayer = GetMinDistanceFromPlayer();
    }

    private float GetArenaScale()
    {
        if (!autoScaleWithArena || arenaGenerator == null)
        {
            return 1f;
        }

        return Mathf.Max(0.5f, arenaGenerator.gridSize / 18f);
    }

    private int GetDifficultyMaxTilesPerWave(float arenaScale)
    {
        int baseLimit;

        switch (GameSettings.difficulty)
        {
            case GameDifficulty.Easy:
                baseLimit = 2;
                break;

            case GameDifficulty.Hard:
                baseLimit = 6;
                break;

            default:
                baseLimit = 4;
                break;
        }

        return Mathf.Clamp(Mathf.RoundToInt(baseLimit * arenaScale), tilesPerWave, maxTilesPerWaveLimit);
    }

    private float GetDifficultyMinPause()
    {
        switch (GameSettings.difficulty)
        {
            case GameDifficulty.Easy:
                return 1.25f;

            case GameDifficulty.Hard:
                return 0.35f;

            default:
                return 0.5f;
        }
    }

    private float GetDifficultyStepTime(float arenaScale)
    {
        float baseStepTime;

        switch (GameSettings.difficulty)
        {
            case GameDifficulty.Easy:
                baseStepTime = 24f;
                break;

            case GameDifficulty.Hard:
                baseStepTime = 12f;
                break;

            default:
                baseStepTime = 16f;
                break;
        }

        return Mathf.Max(8f, baseStepTime / Mathf.Sqrt(arenaScale));
    }

    private float GetMinDistanceFromPlayer()
    {
        if (arenaGenerator == null)
        {
            return minDistanceFromPlayer;
        }

        return Mathf.Max(
            3f,
            arenaGenerator.tileSize * 2.5f
        );
    }

    public void StartCollapse()
    {
        if (isRunning)
        {
            return;
        }

        isRunning = true;
        StartCoroutine(CollapseLoop());
    }

    private IEnumerator CollapseLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (isRunning)
        {
            CollapseRandomTiles();

            yield return new WaitForSeconds(warningTime);
            yield return new WaitForSeconds(pauseAfterCollapse);

            UpdateDifficulty();
        }
    }

    private void CollapseRandomTiles()
    {
        if (arenaGenerator == null || player == null)
        {
            return;
        }

        List<ArenaTile> candidates = new List<ArenaTile>();

        foreach (ArenaTile tile in arenaGenerator.tiles)
        {
            if (tile == null)
            {
                continue;
            }

            if (tile.isDestroyed || tile.isProtected || tile.hasObjectiveItem)
            {
                continue;
            }

            float distanceToPlayer = Vector3.Distance(tile.transform.position, player.position);

            if (distanceToPlayer < minDistanceFromPlayer)
            {
                continue;
            }

            candidates.Add(tile);
        }

        for (int i = 0; i < tilesPerWave; i++)
        {
            if (candidates.Count == 0)
            {
                return;
            }

            int randomIndex = Random.Range(0, candidates.Count);
            ArenaTile selectedTile = candidates[randomIndex];

            selectedTile.Collapse(warningTime);

            candidates.RemoveAt(randomIndex);
        }
    }

    private void UpdateDifficulty()
    {
        if (!increaseDifficulty)
        {
            return;
        }

        difficultyTimer += warningTime + pauseAfterCollapse;

        if (difficultyTimer < difficultyStepTime)
        {
            return;
        }

        difficultyTimer = 0f;

        if (tilesPerWave < maxTilesPerWave)
        {
            tilesPerWave++;
        }

        pauseAfterCollapse = Mathf.Max(
            minPauseAfterCollapse,
            pauseAfterCollapse - 0.25f
        );
    }
}
