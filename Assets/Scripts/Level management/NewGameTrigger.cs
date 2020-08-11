using UnityEngine;

public class NewGameTrigger : MonoBehaviour
{
    public void EnterNewGame() =>
        NewGameManager.Current.EnterNewGame();
}
