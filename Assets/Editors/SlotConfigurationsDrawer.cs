using UnityEditor;

//[CustomPropertyDrawer(typeof(Armament.SlotConfigurations))]
public class SlotConfigurationsDrawer : GridPropertyDrawer
{
    protected override DrawInstruction[][] GetDrawings => myDrawings;

    private DrawInstruction[][] myDrawings = new DrawInstruction[][]
    {
        new DrawInstruction[]
        {
            new DrawInstruction(DrawDisplay.Variable("configs"), DrawSize.Shared)
        }
    };
}
