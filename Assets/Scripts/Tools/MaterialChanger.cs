using UnityEngine;
using System.Collections;

public class MaterialChanger : MonoBehaviour
{
    public readonly WaitForSeconds wait = new WaitForSeconds(0.2f);

    public Renderer[] renderers;
    public Material flickeringMaterial;

    private bool usingOriginal = true;
    private Material[] originalMaterials;
    private Coroutine flickerRoutine;

    private void Awake()
    {
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalMaterials[i] = renderers[i].sharedMaterial;
    }

    public void ChangeToFlickerMaterial()
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterial = flickeringMaterial;

        usingOriginal = false;
    }

    public void Revert()
    {
        if (usingOriginal)
            return;            

        for(int i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterial = originalMaterials[i];

        usingOriginal = true;
    }

    public void Flicker()
    {
        StopFlicker();
        flickerRoutine = StartCoroutine(FlickerRoutine());
    }

    public void StopFlicker()
    {
        if (flickerRoutine != null)
            StopCoroutine(flickerRoutine);
        Revert();
    }

    private IEnumerator FlickerRoutine()
    {
        do
        {
            ChangeToFlickerMaterial();
            yield return wait;
            Revert();
            yield return wait;
        } while (true);
    }
}
