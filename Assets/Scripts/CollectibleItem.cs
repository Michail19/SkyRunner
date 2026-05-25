using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    private ObjectiveManager objectiveManager;

    public void Init(ObjectiveManager manager)
    {
        objectiveManager = manager;
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
