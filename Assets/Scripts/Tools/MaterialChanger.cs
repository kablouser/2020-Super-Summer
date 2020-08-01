using UnityEngine;
using System.Collections;

public class MaterialChanger : MonoBehaviour
{
    public readonly WaitForSeconds wait = new WaitForSeconds(0.2f);

    public Renderer[] renderers;

    private bool usingOriginal = true;
    private Material[][] originalMaterials;
    private Coroutine flickerRoutine;

    private void Awake()
    {
        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = (Material[])renderers[i].sharedMaterials.Clone();
        }
    }

    public void ChangeTo(Material newMaterial)
    {
        foreach(Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
                materials[i] = newMaterial;

            renderer.sharedMaterials = materials;
        }

        usingOriginal = false;
    }

    public void Revert()
    {
        if (usingOriginal)
            return;            

        for(int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            for (int j = 0; j < materials.Length; j++)
                materials[j] = originalMaterials[i][j];

            renderers[i].sharedMaterials = materials;
        }

        usingOriginal = true;
    }

    public void Flicker(Material flickerMaterial)
    {
        StopFlicker();
        flickerRoutine = StartCoroutine(FlickerRoutine(flickerMaterial));
    }

    public void StopFlicker()
    {
        if (flickerRoutine != null)
            StopCoroutine(flickerRoutine);
        Revert();
    }

    private IEnumerator FlickerRoutine(Material flickerMaterial)
    {
        do
        {
            ChangeTo(flickerMaterial);
            yield return wait;
            Revert();
            yield return wait;
        } while (true);
    }
}
