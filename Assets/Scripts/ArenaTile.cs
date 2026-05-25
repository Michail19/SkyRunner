using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArenaTile : MonoBehaviour
{
    [Header("State")]
    public bool isProtected;
    public bool isDestroyed;

    [Header("Visual")]
    public Renderer tileRenderer;
    public Material normalMaterial;
    public Material warningMaterial;

    private Collider tileCollider;

    private void Awake()
    {
        tileCollider = GetComponent<Collider>();

        if (tileRenderer == null)
        {
            tileRenderer = GetComponentInChildren<Renderer>();
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
