using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    [Header("References")]
    public ArenaGenerator arenaGenerator;
    public SimpleBotPusher botPrefab;
    public Transform player;

    [Header("Spawn Settings")]
    public float startDelay = 3f;
    public float spawnInterval = 8f;
    public int botsPerWave = 1;
    public int maxAliveBots = 5;
    public float spawnHeightOffset = 2f;

    [Header("Spawn Safety")]
    public float minDistanceFromPlayer = 8f;
    public float edgeSpawnPercent = 0.55f;

    [Header("Difficulty")]
    public bool increaseDifficulty = true;
    public float difficultyStepTime = 20f;
    public int maxBotsPerWave = 3;
    public int maxAliveBotsLimit = 10;
    public float minSpawnInterval = 3f;

    private readonly List<SimpleBotPusher> aliveBots = new List<SimpleBotPusher>();
    private float difficultyTimer;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        // Ждём один кадр, чтобы ArenaGenerator точно успел создать клетки.
        yield return null;

        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            CleanupDeadBots();

            SpawnWave();

            UpdateDifficulty();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnWave()
    {
        if (arenaGenerator == null || botPrefab == null || player == null)
        {
            return;
        }

        CleanupDeadBots();

        int availableSlots = maxAliveBots - aliveBots.Count;

        if (availableSlots <= 0)
        {
            return;
        }

        int spawnCount = Mathf.Min(botsPerWave, availableSlots);

        for (int i = 0; i < spawnCount; i++)
        {
            ArenaTile spawnTile = GetRandomSpawnTile();

            if (spawnTile == null)
            {
                return;
            }

            Vector3 spawnPosition = spawnTile.transform.position + Vector3.up * spawnHeightOffset;

            SimpleBotPusher bot = Instantiate(
                botPrefab,
                spawnPosition,
                Quaternion.identity
            );

            bot.player = player;
            bot.arenaGenerator = arenaGenerator;

            aliveBots.Add(bot);
        }
    }

    private ArenaTile GetRandomSpawnTile()
    {
        List<ArenaTile> candidates = new List<ArenaTile>();

        float maxDistanceFromCenter = GetMaxTileDistanceFromCenter();

        foreach (ArenaTile tile in arenaGenerator.tiles)
        {
            if (tile == null || tile.isDestroyed || tile.isProtected)
            {
                continue;
            }

            float distanceFromCenter = new Vector2(
                tile.transform.position.x,
                tile.transform.position.z
            ).magnitude;

            // Берём плитки ближе к краю карты.
            if (distanceFromCenter < maxDistanceFromCenter * edgeSpawnPercent)
            {
                continue;
            }

            float distanceToPlayer = Vector3.Distance(
                tile.transform.position,
                player.position
            );

            if (distanceToPlayer < minDistanceFromPlayer)
            {
                continue;
            }

            candidates.Add(tile);
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }

    private float GetMaxTileDistanceFromCenter()
    {
        float maxDistance = 0f;

        foreach (ArenaTile tile in arenaGenerator.tiles)
        {
            if (tile == null)
            {
                continue;
            }

            float distance = new Vector2(
                tile.transform.position.x,
                tile.transform.position.z
            ).magnitude;

            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }

        return maxDistance;
    }

    private void CleanupDeadBots()
    {
        for (int i = aliveBots.Count - 1; i >= 0; i--)
        {
            if (aliveBots[i] == null)
            {
                aliveBots.RemoveAt(i);
            }
        }
    }

    private void UpdateDifficulty()
    {
        if (!increaseDifficulty)
        {
            return;
        }

        difficultyTimer += spawnInterval;

        if (difficultyTimer < difficultyStepTime)
        {
            return;
        }

        difficultyTimer = 0f;

        if (botsPerWave < maxBotsPerWave)
        {
            botsPerWave++;
        }

        if (maxAliveBots < maxAliveBotsLimit)
        {
            maxAliveBots++;
        }

        spawnInterval = Mathf.Max(
            minSpawnInterval,
            spawnInterval - 1f
        );
    }
}
