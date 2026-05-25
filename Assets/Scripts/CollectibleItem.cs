using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    private ObjectiveManager objectiveManager;
    private ArenaTile ownerTile;

    public void Init(ObjectiveManager manager, ArenaTile tile)
    {
        objectiveManager = manager;
        ownerTile = tile;

        if (ownerTile != null)
        {
            ownerTile.hasObjectiveItem = true;
        }
    }

    public void ClearOwnerTile()
    {
        if (ownerTile != null)
        {
            ownerTile.hasObjectiveItem = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
        {
            player = other.GetComponentInParent<PlayerController>();
        }

        if (player == null)
        {
            return;
        }

        if (objectiveManager != null)
        {
            objectiveManager.CollectItem(this);
        }
    }
}
