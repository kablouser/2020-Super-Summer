using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "MultiEffect", menuName = "Effects/MultiEffect", order = 0)]
public class MultiEffect : Effect
{
    [ContextMenuItem("Gather effects into children", "GatherEffectsIntoChildren")]
    [ContextMenuItem("Clear children effects", "ClearChildrenEffects")]
    public List<Effect> effects;

    public override void Apply(CharacterSheet target)
    {
        foreach (Effect effect in effects)
            if (effect != null)
                effect.Apply(target);
    }

    public override void Remove(CharacterSheet target)
    {
        foreach (Effect effect in effects)
            if (effect != null)
                effect.Remove(target);
    }

    public T FindSubEffect<T>() where T : Effect
    {
        foreach (Effect effect in effects)
            if (effect is T t)
                return t;
        return null;
    }

    public T FindSubEffect<T>(System.Func<T, bool> match) where T : Effect
    {
        foreach (Effect effect in effects)
            if (effect is T t && match(t))
                return t;
        return null;
    }

    public override T IsA<T>()
    {
        T result = base.IsA<T>();
        if(result == null)
            result = FindSubEffect<T>();
        return result;
    }

#if UNITY_EDITOR
    [ContextMenu("Gather effects into children")]
    private void GatherEffectsIntoChildren()
    {
        string myPath = AssetDatabase.GetAssetPath(this);

        for(int i = 0; i < effects.Count; i++)
        {
            if (effects[i] == null)
            {
                effects.RemoveAt(i);
                i--;
                continue;
            }

            bool isSub = AssetDatabase.IsSubAsset(effects[i]);
            string path = AssetDatabase.GetAssetPath(effects[i]);

            if (isSub && path == myPath)
                //its already a child
                continue;

            var copy = Instantiate(effects[i]);
            AssetDatabase.AddObjectToAsset(copy, this);

            //don't delete if it's a sub-asset of something else
            if (isSub == false && AssetDatabase.Contains(effects[i]))
                AssetDatabase.DeleteAsset(path);

            effects[i] = copy;
        }

        AssetDatabase.ImportAsset(myPath);
    }

    [ContextMenu("Clear children effects")]
    private void ClearChildrenEffects()
    {
        string myPath = AssetDatabase.GetAssetPath(this);

        foreach (Effect effect in effects)
        {
            if (effect == null) continue;

            bool isSub = AssetDatabase.IsSubAsset(effect);
            string path = AssetDatabase.GetAssetPath(effect);

            if(isSub && path == myPath)
                AssetDatabase.RemoveObjectFromAsset(effect);
        }

        //get all sub-assets that are not in my effects list        
        Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(myPath);
        foreach(Object subAsset in subAssets)
            AssetDatabase.RemoveObjectFromAsset(subAsset);

        effects.Clear();
        AssetDatabase.ImportAsset(myPath);
    }
#endif
}
