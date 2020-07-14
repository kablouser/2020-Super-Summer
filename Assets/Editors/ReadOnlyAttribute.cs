#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class ReadOnlyProperty : PropertyAttribute {}

[CustomPropertyDrawer(typeof(ReadOnlyProperty))]
public class ReadOnlyDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;

        float height = GetPropertyHeight(property, label);
        position.height = height;

        EditorGUI.PropertyField(position, property);

        GUI.enabled = true;
    }
}

#endif