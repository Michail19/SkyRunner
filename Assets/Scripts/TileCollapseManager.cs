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

    [Header("Difficulty")]
    public bool increaseDifficulty = true;
    public float difficultyStepTime = 15f;
    public int maxTilesPerWave = 4;
    public float minPauseAfterCollapse = 0.5f;

    [Header("Safety")]
    public float minDistanceFromPlayer = 3f;

    private bool isRunning;
    private float difficultyTimer;

    private void Start()
    {
        ApplyGameSettings();
        StartCollapse();
    }

    private void ApplyGameSettings()
    {
        startDelay = GameSettings.collapseStartDelay;
        warningTime = GameSettings.warningTime;
        pauseAfterCollapse = GameSettings.pauseAfterCollapse;
        tilesPerWave = GameSettings.tilesPerWave;
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
