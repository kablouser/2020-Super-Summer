using UnityEditor;

[CustomPropertyDrawer(typeof(CharacterSheet.ResourceRecord))]
public class ResourceRecordDrawer : GridPropertyDrawer
{    
    private const float regenLabelWidth = 50f;
    
    protected override DrawInstruction[][] GetDrawings => myDrawings;

    private static readonly DrawInstruction[][] myDrawings = new DrawInstruction[][]
    {
        new DrawInstruction[]
        {
            new DrawInstruction(DrawDisplay.Variable("current"), DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Label("/", middleAlign), DrawSize.ConstantWidth(unitWidth)),
            new DrawInstruction(DrawDisplay.Variable("max"), DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Label("+", middleAlign), DrawSize.ConstantWidth(unitWidth)),
            new DrawInstruction(DrawDisplay.Variable("additionalMax"), DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Label("regen", rightAlign), DrawSize.ConstantWidth(regenLabelWidth)),
            new DrawInstruction(DrawDisplay.Variable("regen"), DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Label("+", middleAlign), DrawSize.ConstantWidth(unitWidth)),
            new DrawInstruction(DrawDisplay.Variable("additionalRegen"), DrawSize.Shared)
        }
    };
}
