using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public virtual void OnSubmit() { }

    protected virtual void Show(RectTransform targetPosition)
    {
        EventSystemModifier.Current.EnablePanel(this);
        if (hideNextFrameRoutine != null)
            StopCoroutine(hideNextFrameRoutine);

        if(targetPosition != null)
            transform.position =
                targetPosition.position -
                new Vector3(0, targetPosition.rect.height / 2.0f);

        gameObject.SetActive(true);

        if (EventSystemModifier.Current.IsUsingMouse == false)
        {
            var trySelect = SelectOnFocus;
            if(trySelect != null)
                trySelect.Select();
        }
    }

    public virtual void Hide()
    {
        EventSystemModifier.Current.DisablePanel(this);
        gameObject.SetActive(false);

        if (EventSystem.current == null) return;

        if (EventSystemModifier.Current.IsUsingMouse)
            EventSystem.current.SetSelectedGameObject(null);
        else
        {
            var trySelect = SelectOnDefocus;
            if (trySelect == null)
                EventSystem.current.SetSelectedGameObject(null);
            else
                trySelect.Select();
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