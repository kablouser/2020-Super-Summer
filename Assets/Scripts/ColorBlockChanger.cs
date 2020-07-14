using UnityEngine;
using UnityEngine.UI;

public class ColorBlockChanger : MonoBehaviour
{
    public ColorBlock colorBlock = ColorBlock.defaultColorBlock;

    [ContextMenu("Go")]
    void Go()
    {
        var array = FindObjectsOfType<Selectable>(true);
        foreach (var item in array)
            item.colors = colorBlock;
    }
}
