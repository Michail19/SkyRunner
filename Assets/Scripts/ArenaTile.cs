using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArenaTile : MonoBehaviour
{
    [Header("State")]
    public bool isProtected;
    public bool isDestroyed;

    [Header("Gameplay")]
    public bool hasObjectiveItem;

    [Header("Visual")]
    public Renderer tileRenderer;
    public Material normalMaterial;
    public Material protectedMaterial;
    public Material warningMaterial;

    private Collider tileCollider;

    private void Awake()
    {
        CacheComponents();
        ApplyDefaultVisual();
    }

    private void OnValidate()
    {
        CacheComponents();
        ApplyDefaultVisual();
    }

    private void CacheComponents()
    {
        if (tileCollider == null)
        {
            tileCollider = GetComponent<Collider>();
        }

        if (tileRenderer == null)
        {
            tileRenderer = GetComponentInChildren<Renderer>();
        }
    }

    public void Setup(bool protectedTile)
    {
        isProtected = protectedTile;
        isDestroyed = false;

        CacheComponents();

        if (tileCollider != null)
        {
            tileCollider.enabled = true;
        }

        gameObject.SetActive(true);
        ApplyDefaultVisual();
    }

    private void ApplyDefaultVisual()
    {
        if (tileRenderer == null)
        {
            return;
        }

        Material targetMaterial = normalMaterial;

        if (isProtected && protectedMaterial != null)
        {
            targetMaterial = protectedMaterial;
        }

        if (targetMaterial == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            tileRenderer.material = targetMaterial;
        }
        else
        {
            tileRenderer.sharedMaterial = targetMaterial;
        }
    }

    public void Collapse(float warningTime)
    {
        if (isDestroyed || isProtected)
        {
            return;
        }

        StartCoroutine(CollapseRoutine(warningTime));
    }

    private IEnumerator CollapseRoutine(float warningTime)
    {
        if (tileRenderer != null && warningMaterial != null)
        {
            tileRenderer.material = warningMaterial;
        }

        yield return new WaitForSeconds(warningTime);

        isDestroyed = true;

        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }

        gameObject.SetActive(false);
    }
}
