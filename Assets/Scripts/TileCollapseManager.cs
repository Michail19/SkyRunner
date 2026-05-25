using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCollapseManager : MonoBehaviour
{
    [Header("References")]
    public ArenaGenerator arenaGenerator;
    public Transform player;

    [Header("Collapse Settings")]
    public float startDelay = 10f;
    public float collapseInterval = 2.5f;
    public float warningTime = 1f;
    public int tilesPerWave = 1;

    [Header("Safety")]
    public float minDistanceFromPlayer = 3f;

    private void Start()
    {
        StartCoroutine(CollapseLoop());
    }

    private IEnumerator CollapseLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
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
            if (tile == null || tile.isDestroyed || tile.isProtected)
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

            int index = Random.Range(0, candidates.Count);
            ArenaTile selectedTile = candidates[index];

            selectedTile.Collapse(warningTime);
            candidates.RemoveAt(index);
        }
    }
}
