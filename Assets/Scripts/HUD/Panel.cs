using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class Panel : MonoBehaviour
{
    public abstract bool HideOnClickOutside { get; }
    public abstract Selectable SelectOnFocus { get; }
    public abstract Selectable SelectOnDefocus { get; }
    public bool IsShown => gameObject.activeSelf;    

    [Tooltip("Hides if HideOnClickOutside is true and the cursor clicks outside of this object")]
    public GameObject panelObject;

    private Coroutine hideNextFrameRoutine;

    public virtual void OnEnable()
    {
        DeviceSwitcher.Instance.EnablePanel(this);
    }

    public virtual void OnDisable()
    {
        DeviceSwitcher.Instance.DisablePanel(this);
    }

    public virtual void OnSubmit() { }

    protected virtual void Show(RectTransform targetPosition)
    {
        if (hideNextFrameRoutine != null)
            StopCoroutine(hideNextFrameRoutine);

        if(targetPosition != null)
            transform.position =
                targetPosition.position -
                new Vector3(0, targetPosition.rect.height / 2.0f);

        gameObject.SetActive(true);

        if (DeviceSwitcher.Instance.IsUsingMouse == false)
        {
            var trySelect = SelectOnFocus;
            if(trySelect != null)
                trySelect.Select();
        }
    }

    public virtual void Hide()
    {
        if (IsShown)
        {
            gameObject.SetActive(false);

            if (DeviceSwitcher.Instance.IsUsingMouse == false)
            {
                var trySelect = SelectOnDefocus;
                if (trySelect != null)
                    trySelect.Select();
            }
        }
    }

    public void HideNextFrame()
    {
        if (IsShown)
        {
            if (hideNextFrameRoutine != null)
                StopCoroutine(hideNextFrameRoutine);
            hideNextFrameRoutine = StartCoroutine(HideNextFrameRoutine());
        }
    }

    private IEnumerator HideNextFrameRoutine()
    {
        yield return null;
        Hide();
    }
}