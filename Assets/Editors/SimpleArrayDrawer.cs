using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SimpleArray))]
public class SimpleArrayDrawer : PropertyDrawer
{
    private const string cross = "X";

    private readonly float singleLine = EditorGUIUtility.singleLineHeight;
    private readonly float space = EditorGUIUtility.standardVerticalSpacing;
    
    private float height;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Debug.Log("Hello world");
        EditorGUI.BeginProperty(position, label, property);

        label.text += ":" + fieldInfo.FieldType.GetGenericArguments()[0].FullName;
        Rect prefixedPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        SerializedProperty array = property.FindPropertyRelative("array");

        if (array.isArray)
        {
            height = 0;

            //SimpleArrayAttribute simpleArray = attribute as SimpleArrayAttribute;
            Rect drawBox = prefixedPosition;
            drawBox.width = 10;// simpleArray.width;
            drawBox.height = singleLine;

            Rect buttonBox = new Rect(0, 0, singleLine, singleLine);

            if (EditorGUIUtility.currentViewWidth < prefixedPosition.x + drawBox.width + space + buttonBox.width)
                return;

            for (int i = 0; i < array.arraySize; i++)
            {
                buttonBox.x = drawBox.x + space + singleLine;
                if(EditorGUIUtility.currentViewWidth < buttonBox.xMax)
                {
                    //create a new row
                    drawBox.x = prefixedPosition.x;
                    drawBox.y += drawBox.height + space;
                    buttonBox.x = drawBox.x + space + singleLine;

                    height += drawBox.height + space;
                    drawBox.height = 0;
                }

                SerializedProperty element = array.GetArrayElementAtIndex(i);
                float elementHeight = EditorGUI.GetPropertyHeight(element);
                                
                drawBox.height = Mathf.Max(drawBox.height, elementHeight);
                buttonBox.y = drawBox.y + (drawBox.height - singleLine) / 2.0f;

                EditorGUI.PropertyField(drawBox, element, true);

                if (GUI.Button(buttonBox, cross))
                {
                    array.DeleteArrayElementAtIndex(i);
                }

                drawBox.x = buttonBox.xMax + space;
            }

            height += drawBox.height;
        }
        else
        {
            height = prefixedPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.HelpBox(prefixedPosition, "This object is not an array! It's ", MessageType.Warning);
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height;
}
