using System.Collections.Generic;
using UnityEngine;

public class ArenaGenerator : MonoBehaviour
{
    [Header("Tile")]
    public ArenaTile tilePrefab;
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

    [Header("Safe Center")]
    public int protectedCenterSize = 2;

    [Header("Generated Tiles")]
    public List<ArenaTile> tiles = new List<ArenaTile>();

    [Header("Collider")]
    public float colliderOverlap = 0.05f;

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Regenerate Arena")]
    public void Generate()
    {
        ClearArena();

        if (tilePrefab == null)
        {
            Debug.LogError("ArenaGenerator: tilePrefab is not assigned.", this);
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

                // Делаем не квадратную карту, а островную форму.
                if (distanceFromCenter > radius)
                {
                    continue;
                }

                bool isProtected = IsProtectedCenter(x, z);

                int heightLevel = 0;

                // Центральные 4 клетки всегда плоские.
                if (useHeightVariation && !isProtected)
                {
                    float noise = Mathf.PerlinNoise(
                        (x + 1000) * noiseScale,
                        (z + 1000) * noiseScale
                    );

                    if (noise < 0.35f)
                    {
                        heightLevel = -1;
                    }
                    else if (noise > 0.68f)
                    {
                        heightLevel = 1;
                    }
                }

                float worldX = (x - centerOffset) * tileSize;
                float worldZ = (z - centerOffset) * tileSize;
                float worldY = heightLevel * heightStep;

                Vector3 position = new Vector3(worldX, worldY, worldZ);

                ArenaTile tile = Instantiate(
                    tilePrefab,
                    position,
                    Quaternion.identity,
                    tileParent
                );

                tile.name = $"ArenaTile_{x}_{z}";
                tile.transform.localScale = Vector3.one;
                tile.transform.localScale = new Vector3(tileSize, tileThickness, tileSize);

                // BoxCollider boxCollider = tile.GetComponent<BoxCollider>();

                //if (boxCollider != null)
                //{
                //    boxCollider.size = new Vector3(
                //        tileSize + colliderOverlap,
                //        tileThickness,
                //        tileSize + colliderOverlap
                //    );

                //    boxCollider.center = new Vector3(
                //        0f,
                //        -tileThickness / 2f,
                //        0f
                //    );
                //}

                tile.Setup(isProtected);

                tiles.Add(tile);
            }
        }
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
