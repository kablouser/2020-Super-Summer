using UnityEngine;
using UnityEditor;

using static Armament;

[System.Serializable]
public class SlotArray<T>
{
    public T[] array = new T[(int)Slot.MAX];

    public T this[int index]
    {
        get => array[index];
        set => array[index] = value;
    }

    public static implicit operator T[](SlotArray<T> s) =>
        s.array;
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(SlotArray<>))]
public class SlotArrayDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property = property.FindPropertyRelative("array");

        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PrefixLabel(position, label);

        int saveIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 1;

        if (property.arraySize != (int)Slot.MAX)
            property.arraySize = (int)Slot.MAX;

        for (int i = 0; i < (int)Slot.MAX; i++)
        {
            position.y += EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing;

            string enumName = ObjectNames.NicifyVariableName(((Slot)i).ToString());
            EditorGUI.PropertyField(position, property.GetArrayElementAtIndex(i), new GUIContent(enumName));
        }
        
        EditorGUI.indentLevel = saveIndent;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return ((int)Slot.MAX + 1) *
            (
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing
            ) - EditorGUIUtility.standardVerticalSpacing;
    }
}

#endif