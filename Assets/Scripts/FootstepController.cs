using UnityEngine;

public class FootstepController : MonoBehaviour
{
    public AudioSource audioSource;
    public PlayerStats stats;

    public AudioClip[] floorStepClips;
    public AudioClip[] forestGroundStepClips;
    public AudioClip[] groundStepClips;
    public AudioClip[] rockyGroundStepClips;
    public AudioClip[] defaultStepClips;

    public float stepInterval = 0.45f;
    public float rayDistance = 2f;

    private float stepTimer;

    void Update()
    {
        if (GamePauseState.IsPaused) return;
        if (audioSource == null) return;
        if (stats == null) return;

        if (stats.isMoving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayStep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayStep()
    {
        AudioClip[] clips = GetCurrentSurfaceClips();

        if (clips == null || clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);
        AudioClip clip = clips[index];

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip);
    }

    AudioClip[] GetCurrentSurfaceClips()
    {
        RaycastHit hit;

        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance))
        {
            if (hit.collider.CompareTag("Floor") && floorStepClips.Length > 0)
                return floorStepClips;

            if (hit.collider.CompareTag("ForestGround") && forestGroundStepClips.Length > 0)
                return forestGroundStepClips;

            if (hit.collider.CompareTag("Ground") && groundStepClips.Length > 0)
                return groundStepClips;

            if (hit.collider.CompareTag("RockyGround") && rockyGroundStepClips.Length > 0)
                return rockyGroundStepClips;
        }

        return defaultStepClips;
    }
}

