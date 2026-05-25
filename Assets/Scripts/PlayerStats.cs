using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float staminaMax = 100f;
    public float stamina = 100f;

    public float drainPerSecond = 15f;
    public float regenPerSecond = 10f;

    public bool isMoving;

    void Awake()
    {
        stamina = Mathf.Clamp(stamina, 0f, staminaMax);
    }

    public void TickStamina(float dt)
    {
        if (isMoving)
            stamina -= drainPerSecond * dt;
        else
            stamina += regenPerSecond * dt;

        stamina = Mathf.Clamp(stamina, 0f, staminaMax);
    }

    public bool HasStamina()
    {
        return stamina > 0.1f;
    }
}
