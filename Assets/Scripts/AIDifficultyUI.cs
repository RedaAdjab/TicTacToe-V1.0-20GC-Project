using System;
using UnityEngine;
using UnityEngine.UI;

public class AIDifficultyUI : MonoBehaviour
{
    public static event EventHandler OnDifficultySelected;

    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;

    private void Start()
    {
        easyButton.onClick.AddListener(OnEasyButtonClicked);
        mediumButton.onClick.AddListener(OnMediumButtonClicked);
        hardButton.onClick.AddListener(OnHardButtonClicked);

        if (GameStateManager.Instance.IsOnline())
        {
            Hide();
        }
    }

    private void OnEasyButtonClicked()
    {
        Debug.Log("Easy AI selected");
        GameManager.Instance.SetAIDifficulty(GameManager.AIDifficulty.Easy);
        OnDifficultySelected?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    private void OnMediumButtonClicked()
    {
        Debug.Log("Medium AI selected");
        GameManager.Instance.SetAIDifficulty(GameManager.AIDifficulty.Medium);
        OnDifficultySelected?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    private void OnHardButtonClicked()
    {
        Debug.Log("Hard AI selected");
        GameManager.Instance.SetAIDifficulty(GameManager.AIDifficulty.Hard);
        OnDifficultySelected?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
