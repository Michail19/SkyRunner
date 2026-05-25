using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stamina")]
    public float staminaMax = 100f;
    public float stamina = 100f;

    [Header("Stamina Drain")]
    public float runDrainPerSecond = 8f;

    [Header("Stamina Regen")]
    public float regenPerSecond = 18f;
    public float regenDelayAfterRun = 0.6f;

    [Header("Run Limits")]
    public float minStaminaToStartRun = 8f;
    public float minStaminaToKeepRun = 0.5f;

    [Header("State")]
    public bool isMoving;
    public bool isRunning;

    private float regenDelayTimer;

    private void Awake()
    {
        stamina = Mathf.Clamp(stamina, 0f, staminaMax);
    }

    public void TickStamina(float dt, bool moving, bool running)
    {
        isMoving = moving;
        isRunning = running;

        if (moving && running && stamina > 0f)
        {
            stamina -= runDrainPerSecond * dt;
            stamina = Mathf.Clamp(stamina, 0f, staminaMax);

            regenDelayTimer = regenDelayAfterRun;
            return;
        }

        if (regenDelayTimer > 0f)
        {
            regenDelayTimer -= dt;
            return;
        }

        stamina += regenPerSecond * dt;
        stamina = Mathf.Clamp(stamina, 0f, staminaMax);
    }

    public bool CanStartRun()
    {
        return stamina >= minStaminaToStartRun;
    }

    public bool CanKeepRunning()
    {
        return stamina > minStaminaToKeepRun;
    }

    public float GetStamina01()
    {
        if (staminaMax <= 0f)
        {
            return 0f;
        }

        return stamina / staminaMax;
    }
}
