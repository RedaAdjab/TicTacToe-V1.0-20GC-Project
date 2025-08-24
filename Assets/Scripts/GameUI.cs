using System;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject crossYouText;
    [SerializeField] private GameObject circleYouText;
    [SerializeField] private GameObject crossArrowTurn;
    [SerializeField] private GameObject circleArrowTurn;
    [SerializeField] private TextMeshProUGUI crossScore;
    [SerializeField] private TextMeshProUGUI circleScore;

    private void Awake()
    {
        crossYouText.SetActive(false);
        circleYouText.SetActive(false);
        crossArrowTurn.SetActive(false);
        circleArrowTurn.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnTurnsChanged += GameManager_OnTurnsChanged;
        GameManager.Instance.OnScoresChanged += GameManager_OnScoresChanged;
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossYouText.SetActive(true);
        }
        else if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Circle)
        {
            circleYouText.SetActive(true);
        }
        else
        {
            crossYouText.SetActive(false);
            circleYouText.SetActive(false);
        }

        UpdateArrows();
        Show();
    }

    private void GameManager_OnTurnsChanged(object sender, EventArgs e)
    {
        UpdateArrows();
    }

    private void GameManager_OnScoresChanged(object sender, EventArgs e)
    {
        crossScore.text = GameManager.Instance.GetCrossScore().ToString();
        circleScore.text = GameManager.Instance.GetCircleScore().ToString();
    }

    private void UpdateArrows()
    {
        if (GameManager.Instance.GetPlayerTypeTurn() == GameManager.PlayerType.Cross)
        {
            crossArrowTurn.SetActive(true);
            circleArrowTurn.SetActive(false);
        }
        else if (GameManager.Instance.GetPlayerTypeTurn() == GameManager.PlayerType.Circle)
        {
            crossArrowTurn.SetActive(false);
            circleArrowTurn.SetActive(true);
        }
        else
        {
            crossArrowTurn.SetActive(false);
            circleArrowTurn.SetActive(false);
        }
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
}
