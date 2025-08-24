using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    private void Start()
    {
        GameManager.Instance.OnGameWinner += GameManager_OnGameWinner;
        restartButton.onClick.AddListener(() =>
        {
            Restart();
        });
        quitButton.onClick.AddListener(() =>
        {
#if UNITY_STANDALONE_WIN
            Application.Quit();
#endif
        });
        GameManager.Instance.OnGameRestarted += GameManager_OnGameRestarted;
        Hide();
    }

    private void GameManager_OnGameWinner(object sender, GameManager.PlayerType e)
    {
        if (e == GameManager.Instance.GetLocalPlayerType())
        {
            winnerText.text = "you win";
            winnerText.color = Color.green;
        }
        else if (e == GameManager.PlayerType.None)
        {
            winnerText.text = "draw";
            winnerText.color = Color.blue;
        }
        else
        {
            winnerText.text = "you lose";
            winnerText.color = Color.red;
        }
        gameObject.SetActive(true);
    }

    private void Restart()
    {
        if (GameStateManager.Instance.IsOnline())
        {
            GameManager.Instance.RestartGameRpc();
        }
        else
        {
            GameManager.Instance.RestartGameRemote();
        }
    }

    private void GameManager_OnGameRestarted(object sender, EventArgs e)
    {
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
