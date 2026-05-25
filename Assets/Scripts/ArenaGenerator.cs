using System.Collections.Generic;
using UnityEngine;

public class ArenaGenerator : MonoBehaviour
{
    [Header("Tile")]
    public GameObject tilePrefab;
    public Transform tileParent;

    [Header("Grid")]
    public int gridSize = 17;
    public float tileSize = 2f;

    [Header("Shape")]
    public float islandRadiusOffset = 1f;

    [Header("Heights")]
    public bool useHeightVariation = true;
    public float heightStep = 0.35f;
    public float noiseScale = 0.18f;

    [Header("Safe Zone")]
    public int protectedRadius = 2;

    public List<ArenaTile> tiles = new List<ArenaTile>();

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        tiles.Clear();

        int half = gridSize / 2;
        float radius = half - islandRadiusOffset;

        for (int x = -half; x <= half; x++)
        {
            for (int z = -half; z <= half; z++)
            {
                float distanceFromCenter = Mathf.Sqrt(x * x + z * z);

                // Делаем не квадрат, а остров.
                if (distanceFromCenter > radius)
                {
                    continue;
                }

                bool isProtected = Mathf.Abs(x) <= protectedRadius && Mathf.Abs(z) <= protectedRadius;

                int heightLevel = 0;

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

                Vector3 position = new Vector3(
                    x * tileSize,
                    heightLevel * heightStep,
                    z * tileSize
                );

                GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity, tileParent);

                ArenaTile tile = tileObject.GetComponent<ArenaTile>();

                if (tile != null)
                {
                    tile.isProtected = isProtected;
                    tiles.Add(tile);
                }
            }
        }
    }
}
