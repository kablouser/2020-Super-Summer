#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using static CharacterSheet;

[CustomEditor(typeof(CharacterSheet))]
public class CharacterSheetEditor : Editor
{
    SerializedProperty attributes;
    SerializedProperty resources;

    void OnEnable()
    {
        attributes = serializedObject.FindProperty("attributes");
        resources = serializedObject.FindProperty("resources");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        
        if (attributes.arraySize != (int)Attribute.MAX)
            attributes.arraySize = (int)Attribute.MAX;

        if (resources.arraySize != (int)Resource.MAX)
            resources.arraySize = (int)Resource.MAX;

        var property = serializedObject.GetIterator();
        property.NextVisible(true);

        bool first = true;

        do
        {
            if (property.propertyPath == attributes.propertyPath)
            {
                EditorGUILayout.LabelField("Attributes");
                EditorGUI.indentLevel++;
                ListEnumLabels(attributes, new Attribute());
                EditorGUI.indentLevel--;
            }
            else if(property.propertyPath == resources.propertyPath)
            {
                EditorGUILayout.LabelField("Resources");
                EditorGUI.indentLevel++;
                ListEnumLabels(resources, new Resource());
                EditorGUI.indentLevel--;
            }
            else
            {
                GUI.enabled = property.editable && first == false;
                EditorGUILayout.PropertyField(property);
                GUI.enabled = true;

                first = false;
            }
        }
        while (property.NextVisible(false));

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }

    private void ListEnumLabels(SerializedProperty arrayProperty, System.Enum enumType)
    {
        var enumValues = System.Enum.GetValues(enumType.GetType());
        int i = 0;

        foreach (var enumValue in enumValues)
        {
            if (arrayProperty.arraySize <= i)
            {
                break;
            }
            else
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, new GUIContent(enumValue.ToString()));                
            }
            i++;
        }
    }
}

#endif