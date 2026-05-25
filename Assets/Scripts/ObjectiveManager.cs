using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [Header("References")]
    public ArenaGenerator arenaGenerator;
    public GameManager gameManager;
    public CollectibleItem itemPrefab;
    public GameObject exitZoneObject;

    [Header("UI")]
    public TextMeshProUGUI objectiveText;

    [Header("Items")]
    public int itemsToSpawn = 3;
    public float itemHeightOffset = 1f;
    public float minDistanceFromCenter = 6f;

    private int collectedItems;
    private readonly List<CollectibleItem> spawnedItems = new List<CollectibleItem>();

    private void Start()
    {
        if (exitZoneObject != null)
        {
            exitZoneObject.SetActive(false);
        }

        Invoke(nameof(SpawnItems), 0.2f);
        UpdateUI();
    }

    private void SpawnItems()
    {
        if (arenaGenerator == null || itemPrefab == null)
        {
            Debug.LogError("ObjectiveManager: missing references.", this);
            return;
        }

        List<ArenaTile> candidates = new List<ArenaTile>();

        foreach (ArenaTile tile in arenaGenerator.tiles)
        {
            if (tile == null || tile.isProtected || tile.isDestroyed)
            {
                continue;
            }

            float distanceFromCenter = new Vector2(
                tile.transform.position.x,
                tile.transform.position.z
            ).magnitude;

            if (distanceFromCenter < minDistanceFromCenter)
            {
                continue;
            }

            candidates.Add(tile);
        }

        for (int i = 0; i < itemsToSpawn; i++)
        {
            if (candidates.Count == 0)
            {
                return;
            }

            int index = Random.Range(0, candidates.Count);
            ArenaTile tile = candidates[index];
            candidates.RemoveAt(index);

            Vector3 position = tile.transform.position + Vector3.up * itemHeightOffset;

            CollectibleItem item = Instantiate(
                itemPrefab,
                position,
                Quaternion.identity
            );

            item.Init(this);
            spawnedItems.Add(item);
        }

        UpdateUI();
    }

    public void CollectItem(CollectibleItem item)
    {
        if (item == null)
        {
            return;
        }

        collectedItems++;

        spawnedItems.Remove(item);
        Destroy(item.gameObject);

        if (collectedItems >= itemsToSpawn)
        {
            ActivateExit();
        }

        UpdateUI();
    }

    private void ActivateExit()
    {
        if (exitZoneObject != null)
        {
            exitZoneObject.SetActive(true);
        }

        Debug.Log("Exit activated.");
    }

    public void TryWin()
    {
        if (collectedItems < itemsToSpawn)
        {
            return;
        }

        if (gameManager != null)
        {
            gameManager.WinGame();
        }
    }

    private void UpdateUI()
    {
        if (objectiveText == null)
        {
            return;
        }

        if (collectedItems < itemsToSpawn)
        {
            objectiveText.text = $"Items: {collectedItems}/{itemsToSpawn}";
        }
        else
        {
            objectiveText.text = "Exit is open!";
        }
    }
}
