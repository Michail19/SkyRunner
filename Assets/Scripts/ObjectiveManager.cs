using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [Header("References")]
    public ArenaGenerator arenaGenerator;
    public GameManager gameManager;
    public CollectibleItem[] itemPrefabs;
    public GameObject exitZoneObject;

    [Header("UI")]
    public TextMeshProUGUI objectiveText;

    [Header("Items")]
    public int itemsToSpawn = 3;
    public float itemHeightOffset = 1f;
    public float minDistanceFromCenter = 6f;

    [Header("Auto Scale")]
    public bool autoScaleWithArena = true;
    public int minItems = 1;
    public int maxItems = 8;

    private int collectedItems;
    private readonly List<CollectibleItem> spawnedItems = new List<CollectibleItem>();

    private void Start()
    {
        GameSettings.Load();
        ApplyGameSettings();

        if (exitZoneObject != null)
        {
            exitZoneObject.SetActive(false);
        }

        Invoke(nameof(SpawnItems), 0.2f);
        UpdateUI();
    }

    private void ApplyGameSettings()
    {
        itemsToSpawn = GameSettings.itemsToCollect;

        if (arenaGenerator == null)
        {
            return;
        }

        minDistanceFromCenter = arenaGenerator.GetWorldRadius() * 0.35f;

        if (!autoScaleWithArena)
        {
            return;
        }

        float arenaScale = Mathf.Max(0.5f, arenaGenerator.gridSize / 18f);
        int scaledItems = Mathf.RoundToInt(GameSettings.itemsToCollect * arenaScale);

        itemsToSpawn = Mathf.Clamp(
            scaledItems,
            Mathf.Min(minItems, GameSettings.itemsToCollect),
            maxItems
        );

        itemsToSpawn = Mathf.Max(1, itemsToSpawn);
    }

    private void SpawnItems()
    {
        if (arenaGenerator == null || itemPrefabs == null)
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
            CollectibleItem selectedPrefab = GetRandomItemPrefab();

            if (selectedPrefab == null)
            {
                Debug.LogError("ObjectiveManager: no valid item prefab found.", this);
                return;
            }

            CollectibleItem item = Instantiate(
                selectedPrefab,
                position,
                Quaternion.identity
            );

            item.Init(this, tile);
            spawnedItems.Add(item);
        }

        UpdateUI();
    }

    private CollectibleItem GetRandomItemPrefab()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            return null;
        }

        List<CollectibleItem> validPrefabs = new List<CollectibleItem>();

        foreach (CollectibleItem prefab in itemPrefabs)
        {
            if (prefab != null)
            {
                validPrefabs.Add(prefab);
            }
        }

        if (validPrefabs.Count == 0)
        {
            return null;
        }

        int index = Random.Range(0, validPrefabs.Count);
        return validPrefabs[index];
    }

    public void CollectItem(CollectibleItem item)
    {
        if (item == null)
        {
            return;
        }

        collectedItems++;

        spawnedItems.Remove(item);
        item.ClearOwnerTile();
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
            Debug.Log("Выход активирован: " + exitZoneObject.name, exitZoneObject);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx(AudioManager.Instance.portalOpenClip, 1f);
            }
        }
        else
        {
            Debug.LogError("ObjectiveManager: объект выхода не назначен.", this);
        }
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
            objectiveText.text = $"Предметы: {collectedItems}/{itemsToSpawn}";
        }
        else
        {
            objectiveText.text = "Выход открыт!";
        }
    }
}
