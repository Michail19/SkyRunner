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
    public float collapseInterval = 2f;
    public float warningTime = 1f;
    public int tilesPerWave = 1;

    [Header("Safety")]
    public float minDistanceFromPlayer = 3f;

    private bool isRunning;

    private void Start()
    {
        StartCollapse();
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
            yield return new WaitForSeconds(collapseInterval);
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

            if (tile.isDestroyed || tile.isProtected)
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
}
