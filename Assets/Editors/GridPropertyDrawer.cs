#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public abstract class GridPropertyDrawer : PropertyDrawer
{
    public struct DrawInstruction
    {
        public DrawDisplay display;
        public DrawSize size;
        public DrawInstruction(DrawDisplay display, DrawSize size)
        {
            this.display = display;
            this.size = size;
        }
    }
    public struct DrawDisplay
    {
        public string variableName;
        public string labelText;
        public GUIStyle style;
        public static DrawDisplay Variable(string variableName) =>
            new DrawDisplay()
            {
                variableName = variableName
            };
        public static DrawDisplay Label(string labelText) =>
            new DrawDisplay()
            {
                labelText = labelText
            };
        public static DrawDisplay Label(string labelText, GUIStyle style) =>
            new DrawDisplay()
            {
                labelText = labelText,
                style = style
            };
    }
    public struct DrawSize
    {
        public bool sharedWidth;
        public float constantWidth;
        public static readonly DrawSize Shared = new DrawSize() { sharedWidth = true };
        public static DrawSize ConstantWidth(float constantWidth) =>
            new DrawSize
            {
                sharedWidth = false,
                constantWidth = constantWidth
            };
    }

    protected abstract DrawInstruction[][] GetDrawings { get; }
    protected virtual bool IgnorePrefix { get => false; }
    protected const float unitWidth = 10f;

    protected static readonly GUIStyle leftAlign = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
    protected static readonly GUIStyle middleAlign = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
    protected static readonly GUIStyle rightAlign = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };

    private float height = 0;

    public override void OnGUI(Rect originalPosition, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(originalPosition, label, property);

        if (IgnorePrefix)
            label.text = " ";

        Rect prefixedPosition = EditorGUI.PrefixLabel(originalPosition, GUIUtility.GetControlID(FocusType.Passive), label);
        Rect drawBox = prefixedPosition;
        drawBox.height = EditorGUIUtility.singleLineHeight;
        float originalX = prefixedPosition.x;

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        height = 0;

        DrawInstruction[][] getDrawings = GetDrawings;

        for (int i = 0; i < getDrawings.Length; i++)
        {
            float remainingSplit;
            {
                int sharedCount = 0;
                float totalConstantWidth = 0;
                                
                for (int j = 0; j < getDrawings[i].Length; j++)
                    if (getDrawings[i][j].size.sharedWidth)
                        sharedCount++;
                    else
                        totalConstantWidth += getDrawings[i][j].size.constantWidth;

                if (0 < sharedCount)
                    remainingSplit = (
                        prefixedPosition.width
                        - totalConstantWidth
                        - (getDrawings[i].Length - 1) * EditorGUIUtility.standardVerticalSpacing)
                    / sharedCount;
                else
                    remainingSplit = 0;
            }

            float rowHeight = 0;

            for (int j = 0; j < getDrawings[i].Length; j++)
            {
                if (getDrawings[i][j].size.sharedWidth)
                    drawBox.width = remainingSplit;
                else
                    drawBox.width = getDrawings[i][j].size.constantWidth;

                if (getDrawings[i][j].display.labelText == null)
                {
                    Rect copy = drawBox;

                    DrawPropertyField(
                        property.FindPropertyRelative(getDrawings[i][j].display.variableName),
                        originalPosition,
                        indent,
                        ref copy);

                    rowHeight = Mathf.Max(rowHeight, copy.height);
                }
                else
                {
                    if (getDrawings[i][j].display.style == null)
                        EditorGUI.LabelField(
                            drawBox,
                            getDrawings[i][j].display.labelText);
                    else
                        EditorGUI.LabelField(
                            drawBox,
                            getDrawings[i][j].display.labelText,
                            getDrawings[i][j].display.style);
                }

                drawBox.x += drawBox.width + EditorGUIUtility.standardVerticalSpacing;
            }
            
            drawBox.x = originalX;
            drawBox.y += rowHeight;
            height += rowHeight;
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height;

    private void DrawPropertyField(SerializedProperty property, Rect originalPosition, int indent, ref Rect position)
    {
        GUIContent displayLabel;
        Rect useRect;

        bool isArray = false;
        float extraSpace = 0;

        if (property.isArray && property.propertyType != SerializedPropertyType.String)
        {
            isArray = true;

            if (IgnorePrefix)
                displayLabel = GUIContent.none;
            else
                displayLabel = new GUIContent(property.displayName);

            useRect = originalPosition;
            useRect.y = position.y;

            if (height == 0 && IgnorePrefix == false)
            {
                //top-most row, there is no space on the left, so make one line
                extraSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                useRect.y += extraSpace;
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.indentLevel++;
        }
        else
        {
            displayLabel = GUIContent.none;
            useRect = position;
        }

        useRect.height = position.height = EditorGUI.GetPropertyHeight(property) + extraSpace;
                
        EditorGUI.PropertyField(
            useRect,
            property,
            displayLabel,
            true);

        if (isArray)
            EditorGUI.indentLevel = 0;
    }
}

#endif