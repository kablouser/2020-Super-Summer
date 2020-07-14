using UnityEngine;
using UnityEngine.UI;

public class ItemControlPanel : Panel
{
    public override bool HideOnClickOutside => true;
    public override Selectable SelectOnFocus => dropButton;
    public override Selectable SelectOnDefocus => controlling.selectable;

    public struct ShowOptions
    {
        public bool showMove;
    }

    public EquipmentDisplayer master;
    public Transform hiddenParent;
    public Transform shownParent;

    //control buttons
    public Button dropButton;
    public Button moveButton;

    private ItemDisplayer controlling;

    public void Show(ItemDisplayer target, ShowOptions showOptions)
    {
        controlling = target;

        SetOption(dropButton, true);
        SetOption(moveButton, showOptions.showMove);
        LinkNavigation();

        Show(target.rectTransform);
    }

    public override void Hide()
    {        
        base.Hide();
        controlling = null;
    }

    public void MoveItem()
    {
        master.BeginDrag(controlling.inventoryIndex, false);
        Hide();
    }

    public void DropItem()
    {
        var copy = controlling;
        Hide();

        if (master.playerEquipment.inventory[copy.inventoryIndex].count == 1)
            master.dropControlPanel.DropItem(1, copy.inventoryIndex);
        else
            master.dropControlPanel.Show(copy);
    }

    private void SetOption(Button target, bool enabled)
    {        
        if (enabled)
        {
            target.transform.SetParent(shownParent);
            target.transform.SetAsLastSibling();
            target.enabled = true;
        }
        else
        {
            target.enabled = false;
            target.transform.SetParent(hiddenParent);
        }
    }

    private void LinkNavigation()
    {
        int length = shownParent.childCount;
        for (int i = 0; i < length; i++)
        {
            Selectable selectable = shownParent.GetChild(i).GetComponent<Selectable>();
            var navigation = selectable.navigation;

            int previous = i - 1;
            if (previous < 0) previous = length - 1;

            int next = i + 1;
            if (length <= next) next = 0;

            navigation.selectOnUp = shownParent.GetChild(previous)
                .GetComponent<Selectable>();
            navigation.selectOnDown = shownParent.GetChild(next)
                .GetComponent<Selectable>();

            selectable.navigation = navigation;
        }
    }
}
