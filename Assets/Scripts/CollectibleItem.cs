using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    private ObjectiveManager objectiveManager;
    private ArenaTile ownerTile;

    [Header("Audio")]
    public AudioClip collectClip;

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
            if (AudioManager.Instance != null)
            {
                AudioClip clip = collectClip != null ? collectClip : AudioManager.Instance.itemCollectClip;
                AudioManager.Instance.PlaySfxAtPosition(clip, transform.position, 1f);
            }

            objectiveManager.CollectItem(this);
        }
    }
}
