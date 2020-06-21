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
    protected const float unitWidth = 10f;

    protected static readonly GUIStyle middleAlign = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
    protected static readonly GUIStyle rightAlign = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };

    private int extraLines = 0;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.BeginProperty(pos, label, prop);
        pos = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        extraLines = 0;

        DrawInstruction[][] getDrawings = GetDrawings;

        float heightPerLine = base.GetPropertyHeight(prop, label);

        float originalX;
        float xPosition = originalX = pos.x;
        float yPosition = pos.y;
        for (int i = 0; i < getDrawings.Length; i++, xPosition = originalX, yPosition += heightPerLine)
        {
            int sharedCount = 0;
            float totalConstantWidth = 0;
            int currentExtraLines = 0;
            for (int j = 0; j < getDrawings[i].Length; j++)
                if (getDrawings[i][j].size.sharedWidth)
                    sharedCount++;
                else
                    totalConstantWidth += getDrawings[i][j].size.constantWidth;

            float remainingSplit;
            if (0 < sharedCount)
                remainingSplit = (pos.width - totalConstantWidth) / sharedCount;
            else
                remainingSplit = 0;

            for (int j = 0; j < getDrawings[i].Length; j++)
            {
                float width;
                if (getDrawings[i][j].size.sharedWidth)
                    width = remainingSplit;
                else
                    width = getDrawings[i][j].size.constantWidth;

                Rect rect = new Rect(xPosition, yPosition, width, heightPerLine);
                if (getDrawings[i][j].display.labelText == null)
                {
                    SerializedProperty property = prop.FindPropertyRelative(getDrawings[i][j].display.variableName);

                    if (property.isExpanded)
                    {
                        rect.height = heightPerLine * (property.arraySize + 2);
                        currentExtraLines = Mathf.Max(currentExtraLines, property.arraySize + 1);
                    }

                    GUIContent displayLabel;
                    if (property.isArray && property.propertyType != SerializedPropertyType.String)
                        displayLabel = new GUIContent(property.displayName);
                    else
                        displayLabel = GUIContent.none;
                    
                    EditorGUI.PropertyField(
                        rect,
                        property,
                        displayLabel,
                        true);
                }
                else
                {
                    if (getDrawings[i][j].display.style == null)
                        EditorGUI.LabelField(
                            rect,
                            getDrawings[i][j].display.labelText);
                    else
                        EditorGUI.LabelField(
                            rect,
                            getDrawings[i][j].display.labelText,
                            getDrawings[i][j].display.style);
                }

                xPosition += width;
            }

            extraLines += currentExtraLines;
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * (GetDrawings.Length + extraLines);
    }

    private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {

    }
}
