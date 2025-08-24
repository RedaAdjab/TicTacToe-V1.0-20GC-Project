using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private Button onlineButton;
    [SerializeField] private Button aiButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        onlineButton.onClick.AddListener(OnOnlineButtonClicked);
        aiButton.onClick.AddListener(OnAIButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    private void OnOnlineButtonClicked()
    {
        GameStateManager.Instance.SetState(GameStateManager.GameState.Online);
        SceneManager.LoadScene(1);
    }

    private void OnAIButtonClicked()
    {
        GameStateManager.Instance.SetState(GameStateManager.GameState.AI);
        SceneManager.LoadScene(2);
    }

    private void OnQuitButtonClicked()
    {
#if UNITY_STANDALONE_WIN
        Application.Quit();
#endif
    }

}
