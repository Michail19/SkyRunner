using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public PlayerStats stats;
    public TextMeshProUGUI staminaText;

    void Update()
    {
        if (stats == null || staminaText == null)
            return;

        staminaText.text = $"Выносливость: {Mathf.RoundToInt(stats.stamina)}";
    }
}
