using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        StartMenu,
        AI,
        Online
    }

    public GameState CurrentState { get; private set; } = GameState.StartMenu;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"Game state changed to: {newState}");
    }

    public bool IsOnline()
    {
        return CurrentState == GameState.Online;
    }

    public bool IsAI()
    {
        return CurrentState == GameState.AI;
    }
}
