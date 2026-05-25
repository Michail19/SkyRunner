using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public ObjectiveManager objectiveManager;

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
            objectiveManager.TryWin();
        }
    }
}
