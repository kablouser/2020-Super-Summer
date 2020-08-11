using UnityEngine;
using System.Collections;

public class AttackIndictator : MonoBehaviour
{
    public const float lastHitFadeRate = 1.0f;
    public const float blockIndicatorFadeRate = 2.0f;

    public GameObject blockIndicatorRotator;
    public SpriteRenderer blockIndicator;
    public Color normalBlockIndicator;
    public Color fadedBlockIndicator;

    public LineRenderer lastHitIndicator;
    public Gradient blockedGradient;
    public Gradient landedGradient;

    public float blockDistance = .7f;

    private float blockAngle;
    private Coroutine fadeLastHitRoutine;
    private Coroutine fadeBlockIndicatorRoutine;

    public void EnableBlockIndicator(float blockAngle)
    {
        this.blockAngle = blockAngle;

        if (fadeBlockIndicatorRoutine != null)
            StopCoroutine(fadeBlockIndicatorRoutine);

        float angleRads = Mathf.Deg2Rad * blockAngle;
        float radius = blockDistance * Mathf.Tan(angleRads);

        blockIndicator.transform.localScale = 2 * radius * Vector3.one;
        blockIndicator.color = normalBlockIndicator;
        blockIndicatorRotator.SetActive(true);
    }

    public void DisableBlockIndicator() =>
        blockIndicatorRotator.SetActive(false);

    public void FadeOutBlockIndicator()
    {
        if (blockIndicatorRotator.gameObject.activeSelf == false) return;

        blockIndicator.color = normalBlockIndicator;

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

        blockIndicatorRotator.SetActive(false);
    }
}
