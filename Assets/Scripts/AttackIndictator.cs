using UnityEngine;
using System.Collections;

public class AttackIndictator : MonoBehaviour
{
    public const float blockDistance = .8f;
    public const float lastHitFadeRate = 1.0f;

    public GameObject blockIndicator;
    public LineRenderer lastHitIndicator;
    public float blockAngle;

    private Coroutine fadeLastHitRoutine;
    private GradientAlphaKey[] lastHitAlphas;
    private GradientAlphaKey[] fadeAlphas;

    public void SetBlockIndicator(bool enabled)
    {
        if (enabled)
        {
            blockIndicator.SetActive(true);

            float angleRads = Mathf.Deg2Rad * blockAngle;
            float radius = blockDistance * Mathf.Tan(angleRads);

            Transform blockTransform = blockIndicator.transform;
            blockTransform.localScale = 2 * radius * Vector3.one;
        }
        else blockIndicator.SetActive(false);
    }

    public void SetLastHit(Vector3 direction)
    {
        var indicatorTransform = lastHitIndicator.transform;
        indicatorTransform.rotation = Quaternion.LookRotation(direction);

        float angleRads = Mathf.Deg2Rad * blockAngle;
        indicatorTransform.localScale = Vector3.one * blockDistance / Mathf.Cos(angleRads);

        gameObject.SetActive(true);
        if (fadeLastHitRoutine != null)
            StopCoroutine(fadeLastHitRoutine);
        fadeLastHitRoutine = StartCoroutine(FadeLastHitIndicator());
    }

    private void Awake()
    {
        Gradient gradient = lastHitIndicator.colorGradient;
        lastHitAlphas = gradient.alphaKeys;
        fadeAlphas = new GradientAlphaKey[lastHitAlphas.Length];
        for (int i = 0; i < fadeAlphas.Length; i++)
            fadeAlphas[i] = lastHitAlphas[i];
    }

    private IEnumerator FadeLastHitIndicator()
    {
        LineRenderer line = lastHitIndicator.GetComponent<LineRenderer>();
        Gradient gradient = line.colorGradient;

        float alphaReduction = 0;
        while (alphaReduction < 1)
        {
            alphaReduction += Time.fixedDeltaTime * lastHitFadeRate;
            for (int i = 0; i < lastHitAlphas.Length; i++)
                fadeAlphas[i].alpha = lastHitAlphas[i].alpha * (1.0f - alphaReduction);

            gradient.alphaKeys = fadeAlphas;
            line.colorGradient = gradient;
            yield return CoroutineConstants.waitFixed;
        }

        gradient.alphaKeys = lastHitAlphas;
        line.colorGradient = gradient;
        gameObject.SetActive(false);
    }
}
