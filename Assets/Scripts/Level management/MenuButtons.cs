using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public const int mainLevelBuildIndex = 1;

    public static void RestartLevel() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);

    public void OnStartButton() =>
        SceneManager.LoadScene(mainLevelBuildIndex, LoadSceneMode.Single);

    public void OnRestartButton() =>
        RestartLevel();

    public void OnQuitButton() =>
        Application.Quit();
}
