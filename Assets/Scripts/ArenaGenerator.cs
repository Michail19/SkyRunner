using System.Collections.Generic;
using UnityEngine;

public class ArenaGenerator : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public ArenaTile lowTilePrefab;      // -1: вода / земля / низина
    public ArenaTile grassTilePrefab;    //  0: трава / обычная земля
    public ArenaTile stoneTilePrefab;    //  1: камень / возвышенность

    [Header("Parent")]
    public Transform tileParent;

    [Header("Grid")]
    public int gridSize = 18;
    public float tileSize = 2f;
    public float tileThickness = 0.25f;

    [Header("Shape")]
    public float islandRadiusOffset = 1f;

    [Header("Heights")]
    public bool useHeightVariation = true;
    public float heightStep = 0.35f;
    public float noiseScale = 0.18f;

    [Header("Noise Thresholds")]
    [Range(0f, 1f)] public float lowThreshold = 0.35f;
    [Range(0f, 1f)] public float highThreshold = 0.68f;

    [Header("Safe Center")]
    public int protectedCenterSize = 2;

    [Header("Collider")]
    public float colliderOverlap = 0.02f;

    [Header("Generated Tiles")]
    public List<ArenaTile> tiles = new List<ArenaTile>();

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Regenerate Arena")]
    public void Generate()
    {
        ClearArena();

        if (grassTilePrefab == null)
        {
            Debug.LogError("ArenaGenerator: grassTilePrefab is not assigned.", this);
            return;
        }

        if (tileParent == null)
        {
            tileParent = transform;
        }

        float centerOffset = (gridSize - 1) / 2f;
        float radius = gridSize / 2f - islandRadiusOffset;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                float distanceFromCenter = Mathf.Sqrt(
                    Mathf.Pow(x - centerOffset, 2) +
                    Mathf.Pow(z - centerOffset, 2)
                );

                if (distanceFromCenter > radius)
                {
                    continue;
                }

                bool isProtected = IsProtectedCenter(x, z);

                int heightLevel = GetHeightLevel(x, z, isProtected);

                float worldX = (x - centerOffset) * tileSize;
                float worldZ = (z - centerOffset) * tileSize;
                float worldY = heightLevel * heightStep;

                Vector3 position = new Vector3(worldX, worldY, worldZ);

                ArenaTile selectedPrefab = GetPrefabForHeightLevel(heightLevel);

                ArenaTile tile = Instantiate(
                    selectedPrefab,
                    position,
                    Quaternion.identity,
                    tileParent
                );

                ConfigureSurface(tile, heightLevel);

                tile.name = $"ArenaTile_{x}_{z}_{heightLevel}";

                // Важно: масштабируем prefab, но collider задаём в локальных координатах.
                //tile.transform.localScale = new Vector3(tileSize, tileThickness, tileSize);

                tile.transform.localScale = Vector3.one;

                BoxCollider boxCollider = tile.GetComponent<BoxCollider>();

                if (boxCollider != null)
                {
                    boxCollider.size = new Vector3(
                        tileSize + colliderOverlap,
                        tileThickness,
                        tileSize + colliderOverlap
                    );

                    boxCollider.center = new Vector3(
                        0f,
                        -tileThickness / 2f,
                        0f
                    );
                }

                ConfigureCollider(tile);

                tile.Setup(isProtected);

                tiles.Add(tile);
            }
        }
    }

    public float GetWorldRadius()
    {
        return (gridSize * tileSize) * 0.5f;
    }

    public float GetWorldDiameter()
    {
        return gridSize * tileSize;
    }

    public int GetApproxTileCount()
    {
        return tiles != null ? tiles.Count : gridSize * gridSize;
    }

    private void ConfigureSurface(ArenaTile tile, int heightLevel)
    {
        if (tile == null)
        {
            return;
        }

        if (heightLevel < 0)
        {
            tile.surfaceType = TileSurfaceType.Water;
            tile.movementSpeedMultiplier = 0.88f;
        }
        else if (heightLevel > 0)
        {
            tile.surfaceType = TileSurfaceType.Stone;
            tile.movementSpeedMultiplier = 1.06f;
        }
        else
        {
            tile.surfaceType = TileSurfaceType.Grass;
            tile.movementSpeedMultiplier = 1f;
        }
    }

    private int GetHeightLevel(int x, int z, bool isProtected)
    {
        // Центральные 4 клетки всегда плоские и травяные.
        if (isProtected)
        {
            return 0;
        }

        if (!useHeightVariation)
        {
            return 0;
        }

        float noise = Mathf.PerlinNoise(
            (x + 1000) * noiseScale,
            (z + 1000) * noiseScale
        );

        if (noise < lowThreshold)
        {
            return -1;
        }

        if (noise > highThreshold)
        {
            return 1;
        }

        return 0;
    }

    private ArenaTile GetPrefabForHeightLevel(int heightLevel)
    {
        if (heightLevel < 0 && lowTilePrefab != null)
        {
            return lowTilePrefab;
        }

        if (heightLevel > 0 && stoneTilePrefab != null)
        {
            return stoneTilePrefab;
        }

        return grassTilePrefab;
    }

    private void ConfigureCollider(ArenaTile tile)
    {
        BoxCollider boxCollider = tile.GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            return;
        }

        // Так collider не станет в 2 раза больше из-за transform.localScale.
        float localOverlap = colliderOverlap / Mathf.Max(0.01f, tileSize);

        boxCollider.size = new Vector3(
            1f + localOverlap,
            1f,
            1f + localOverlap
        );

        boxCollider.center = Vector3.zero;
    }

    [ContextMenu("Clear Arena")]
    public void ClearArena()
    {
        tiles.Clear();

        Transform parent = tileParent != null ? tileParent : transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private bool IsProtectedCenter(int x, int z)
    {
        int start = gridSize / 2 - protectedCenterSize / 2;
        int end = start + protectedCenterSize - 1;

        return x >= start && x <= end && z >= start && z <= end;
    }
}
