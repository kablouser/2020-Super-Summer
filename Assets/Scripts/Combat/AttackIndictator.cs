using UnityEngine;
using System.Collections;

public class AttackIndictator : MonoBehaviour
{
    public const float blockDistance = .8f;
    public const float lastHitFadeRate = 1.0f;
    public const float blockIndicatorFadeRate = 2.0f;

    public SpriteRenderer blockIndicator;
    public Color normalBlockIndicator;
    public Color fadedBlockIndicator;

    public LineRenderer lastHitIndicator;
    public Gradient blockedGradient;
    public Gradient landedGradient;
    
    [Header("Script variable")]
    public float blockAngle;

    private Coroutine fadeLastHitRoutine;
    private Coroutine fadeBlockIndicatorRoutine;

    public void SetBlockIndicator(bool enabled)
    {
        if (enabled)
        {
            blockIndicator.color = normalBlockIndicator;
            blockIndicator.gameObject.SetActive(true);
            if (fadeBlockIndicatorRoutine != null)
                StopCoroutine(fadeBlockIndicatorRoutine);

            float angleRads = Mathf.Deg2Rad * blockAngle;
            float radius = blockDistance * Mathf.Tan(angleRads);

            Transform blockTransform = blockIndicator.transform;
            blockTransform.localScale = 2 * radius * Vector3.one;
        }
        else blockIndicator.gameObject.SetActive(false);
    }

    public void FadeOutBlockIndicator()
    {
        blockIndicator.color = normalBlockIndicator;
        blockIndicator.gameObject.SetActive(true);

        if (fadeBlockIndicatorRoutine != null)
            StopCoroutine(fadeBlockIndicatorRoutine);
        
        if (gameObject.activeInHierarchy)
            fadeBlockIndicatorRoutine = StartCoroutine(FadeBlockIndicator());
    }

    public void SetLastHit(Vector3 direction, bool isBlocked)
    {
        var indicatorTransform = lastHitIndicator.transform;
        indicatorTransform.rotation = Quaternion.LookRotation(direction);

        float angleRads = Mathf.Deg2Rad * blockAngle;
        indicatorTransform.localScale = Vector3.one * blockDistance / Mathf.Cos(angleRads);

        lastHitIndicator.gameObject.SetActive(true);
        if (fadeLastHitRoutine != null)
            StopCoroutine(fadeLastHitRoutine);

        lastHitIndicator.colorGradient = isBlocked ? blockedGradient : landedGradient;

        Material lineMaterial = lastHitIndicator.material;
        Color materialColor = lineMaterial.color;
        materialColor.a = 1;
        lineMaterial.color = materialColor;

        if (gameObject.activeInHierarchy)
            fadeLastHitRoutine = StartCoroutine(FadeLastHitIndicator());
    }

    private IEnumerator FadeLastHitIndicator()
    {
        //range from 0 to 1 (started to finished)
        float progress = 0;
        Material lineMaterial = lastHitIndicator.material;
        Color materialColor = lineMaterial.color;

        do
        {
            yield return CoroutineConstants.waitFixed;
            progress += Time.fixedDeltaTime * lastHitFadeRate;
            materialColor.a = 1 - progress;
            lineMaterial.color = materialColor;
        }
        while (progress < 1);

        //reset
        lastHitIndicator.gameObject.SetActive(false);
    }

    private IEnumerator FadeBlockIndicator()
    {
        //range from 0 to 1 (started to finished)
        float progress = 0;

        do
        {
            yield return CoroutineConstants.waitFixed;
            progress += Time.fixedDeltaTime * blockIndicatorFadeRate;
            blockIndicator.color = Color.Lerp(normalBlockIndicator, fadedBlockIndicator, progress);
        }
        while (progress < 1);

        blockIndicator.gameObject.SetActive(false);
    }
}
