using UnityEngine;
using UnityEditor;

//needed when building (even if it serves no purpose)
public class ReadOnlyProperty : PropertyAttribute { }

#if UNITY_EDITOR

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