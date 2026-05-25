using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public ObjectiveManager objectiveManager;

    private void OnTriggerEnter(Collider other)
    {
        TryHandlePlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHandlePlayer(other);
    }

    private void TryHandlePlayer(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
        {
            player = other.GetComponentInParent<PlayerController>();
        }

        if (player == null)
        {
            player = other.GetComponentInChildren<PlayerController>();
        }

        if (player == null)
        {
            return;
        }

        Debug.Log("Player is inside ExitZone.", this);

        if (objectiveManager != null)
        {
            objectiveManager.TryWin();
        }
        else
        {
            Debug.LogError("ExitZone: ObjectiveManager is not assigned.", this);
        }
    }
}
