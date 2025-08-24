using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum PlayerType { None, Cross, Circle }
    public enum LineType { None, Horizontal, Vertical, DiagonalMain, DiagonalAnti }
    public enum AIDifficulty { None, Easy, Medium, Hard }
    public struct WinResult
    {
        public PlayerType Winner;
        public Vector2 Center;
        public LineType Line;
    }

    public event EventHandler<OnGridBoxClickedEventArgs> OnGridBoxClicked;
    public class OnGridBoxClickedEventArgs : EventArgs
    {
        public Vector2 GridPosition;
        public PlayerType PlayerType;
    }
    public event EventHandler OnGameStarted;
    public event EventHandler OnTurnsChanged;
    public event EventHandler<WinResult> OnGameEnded;
    public event EventHandler<PlayerType> OnGameWinner;
    public event EventHandler OnGameRestarted;
    public event EventHandler OnScoresChanged;
    public event EventHandler OnTurnPlayed;

    private PlayerType localPlayerType = PlayerType.Cross;
    private AIDifficulty aiDifficulty = AIDifficulty.None;
    private NetworkVariable<PlayerType> playerTypeTurn = new NetworkVariable<PlayerType>(PlayerType.None);
    private PlayerType remotePlayerTypeTurn = PlayerType.None;
    private PlayerType[,] playerTypeArray;
    private int playsCount = 0;
    private WinResult winResult;
    private NetworkVariable<int> crossScore = new NetworkVariable<int>(0);
    private int remoteCrossScore = 0;
    private NetworkVariable<int> circleScore = new NetworkVariable<int>(0);
    private int remoteCircleScore = 0;
    private bool isFirstFrame = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of GameManager detected!");
        }
        Instance = this;

        playerTypeArray = new PlayerType[3, 3];
        playsCount = 0;
    }

    private void Start()
    {
        if (GameStateManager.Instance.IsAI())
        {
            AIDifficultyUI.OnDifficultySelected += AIDifficultyUI_OnDifficultySelected;
            localPlayerType = PlayerType.Cross;
            remotePlayerTypeTurn = PlayerType.None;
        }
    }

    private void AIDifficultyUI_OnDifficultySelected(object sender, EventArgs e)
    {
        remotePlayerTypeTurn = PlayerType.Cross;
    }

    private void Update()
    {
        if (GameStateManager.Instance.IsAI())
        {
            if (isFirstFrame)
            {
                OnGameStarted?.Invoke(this, EventArgs.Empty);
                OnTurnsChanged?.Invoke(this, EventArgs.Empty);
                OnScoresChanged?.Invoke(this, EventArgs.Empty);
                isFirstFrame = false;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Called with ID " + NetworkManager.Singleton.LocalClientId);

        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else if (NetworkManager.Singleton.LocalClientId == 1)
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnected;
        }

        playerTypeTurn.OnValueChanged += (oldValue, newValue) =>
        {
            OnTurnsChanged?.Invoke(this, EventArgs.Empty);
        };

        crossScore.OnValueChanged += (oldValue, newValue) =>
        {
            OnScoresChanged?.Invoke(this, EventArgs.Empty);
        };
        circleScore.OnValueChanged += (oldValue, newValue) =>
        {
            OnScoresChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            playerTypeTurn.Value = localPlayerType;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridBoxRpc(Vector2Int gridCoordinate, Vector2 gridPosition, PlayerType playerType)
    {
        Debug.Log("Grid position clicked on coordinate " + gridCoordinate + " of world position " + gridPosition);

        if (playerTypeTurn.Value != playerType)
        {
            Debug.Log("It's not your turn!");
            return;
        }

        if (playerTypeArray[gridCoordinate.x, gridCoordinate.y] != PlayerType.None)
        {
            Debug.Log("Grid box already occupied!");
            return;
        }

        playerTypeArray[gridCoordinate.x, gridCoordinate.y] = playerType;
        playsCount++;
        OnGridBoxClicked?.Invoke(this, new OnGridBoxClickedEventArgs
        {
            GridPosition = gridPosition,
            PlayerType = playerType
        });
        TriggerOnTurnPlayedRpc();

        switch (playerType)
        {
            case PlayerType.Cross:
                playerTypeTurn.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                playerTypeTurn.Value = PlayerType.Cross;
                break;
            default:
                Debug.LogError("Invalid player type");
                return;
        }

        // here we check for a win
        winResult = CheckWin();
        if (winResult.Winner != PlayerType.None)
        {
            // win
            Debug.Log($"Winner: {winResult.Winner}, Line: {winResult.Line}, Center: {winResult.Center}");
            winResult.Center = GridBoxList.Instance.GetGridWorldPosition(winResult.Center);
            playerTypeTurn.Value = PlayerType.None;
            if (winResult.Winner == PlayerType.Cross)
            {
                crossScore.Value++;
            }
            else if (winResult.Winner == PlayerType.Circle)
            {
                circleScore.Value++;
            }
            OnGameEnded?.Invoke(this, winResult);
            TriggerOnGameWinnerRpc(winResult.Winner);
        }
        else if (playsCount >= 9)
        {
            // draw
            Debug.Log("It's a draw!");
            playerTypeTurn.Value = PlayerType.None;
            OnGameEnded?.Invoke(this, winResult);
            TriggerOnGameWinnerRpc(winResult.Winner);
        }
    }

    public void ClickedOnGridBoxRemote(Vector2Int gridCoordinate, Vector2 gridPosition, PlayerType playerType)
    {
        Debug.Log("Grid position clicked on coordinate " + gridCoordinate + " of world position " + gridPosition);

        if (remotePlayerTypeTurn != playerType)
        {
            Debug.Log("It's not your turn!");
            return;
        }

        if (playerTypeArray[gridCoordinate.x, gridCoordinate.y] != PlayerType.None)
        {
            Debug.Log("Grid box already occupied!");
            return;
        }

        playerTypeArray[gridCoordinate.x, gridCoordinate.y] = playerType;
        playsCount++;
        OnGridBoxClicked?.Invoke(this, new OnGridBoxClickedEventArgs
        {
            GridPosition = gridPosition,
            PlayerType = playerType
        });

        OnTurnPlayed?.Invoke(this, EventArgs.Empty);

        switch (playerType)
        {
            case PlayerType.Cross:
                remotePlayerTypeTurn = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                remotePlayerTypeTurn = PlayerType.Cross;
                break;
            default:
                Debug.LogError("Invalid player type");
                return;
        }

        OnTurnsChanged?.Invoke(this, EventArgs.Empty);

        // here we check for a win
        winResult = CheckWin();
        if (winResult.Winner != PlayerType.None)
        {
            // win
            Debug.Log($"Winner: {winResult.Winner}, Line: {winResult.Line}, Center: {winResult.Center}");
            winResult.Center = GridBoxList.Instance.GetGridWorldPosition(winResult.Center);
            remotePlayerTypeTurn = PlayerType.None;
            if (winResult.Winner == PlayerType.Cross)
            {
                remoteCrossScore++;
                OnScoresChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (winResult.Winner == PlayerType.Circle)
            {
                remoteCircleScore++;
                OnScoresChanged?.Invoke(this, EventArgs.Empty);
            }
            OnGameEnded?.Invoke(this, winResult);
            OnGameWinner?.Invoke(this, winResult.Winner);
        }
        else if (playsCount >= 9)
        {
            // draw
            Debug.Log("It's a draw!");
            playerTypeTurn.Value = PlayerType.None;
            OnGameEnded?.Invoke(this, winResult);
            OnGameWinner?.Invoke(this, winResult.Winner);
        }
        // After human move, if it's now AI's turn, make AI play
        if (remotePlayerTypeTurn == PlayerType.Circle && winResult.Winner == PlayerType.None && playsCount < 9)
        {
            if (aiDifficulty == AIDifficulty.Easy)
                StartCoroutine(AiPlayMoveEasy());
            else if (aiDifficulty == AIDifficulty.Medium)
                StartCoroutine(AiPlayMoveMedium());
            else if (aiDifficulty == AIDifficulty.Hard)
                StartCoroutine(AiPlayMoveHard());
        }
    }

    [Rpc(SendTo.Server)]
    public void RestartGameRpc()
    {
        playerTypeArray = new PlayerType[3, 3];
        playsCount = 0;
        if (winResult.Winner == PlayerType.Cross)
        {
            playerTypeTurn.Value = PlayerType.Circle;
        }
        else if (winResult.Winner == PlayerType.Circle)
        {
            playerTypeTurn.Value = PlayerType.Cross;
        }
        else
        {
            if (Random.value > 0.5f)
            {
                playerTypeTurn.Value = PlayerType.Cross;
            }
            else
            {
                playerTypeTurn.Value = PlayerType.Circle;
            }
        }

        TriggerOnGameRestartedRpc();
    }

    public void RestartGameRemote()
    {
        playerTypeArray = new PlayerType[3, 3];
        playsCount = 0;
        if (winResult.Winner == PlayerType.Cross)
        {
            remotePlayerTypeTurn = PlayerType.Circle;
        }
        else if (winResult.Winner == PlayerType.Circle)
        {
            remotePlayerTypeTurn = PlayerType.Cross;
        }
        else
        {
            if (Random.value > 0.5f)
            {
                remotePlayerTypeTurn = PlayerType.Cross;
            }
            else
            {
                remotePlayerTypeTurn = PlayerType.Circle;
            }
        }
        winResult = new WinResult() { Winner = PlayerType.None, Center = Vector2.zero, Line = LineType.None };

        OnTurnsChanged?.Invoke(this, EventArgs.Empty);

        OnGameRestarted?.Invoke(this, EventArgs.Empty);

        // After restart, if it's AI's turn, make AI play
        if (remotePlayerTypeTurn == PlayerType.Circle)
        {
            if (aiDifficulty == AIDifficulty.Easy)
                StartCoroutine(AiPlayMoveEasy());
            else if (aiDifficulty == AIDifficulty.Medium)
                StartCoroutine(AiPlayMoveMedium());
            else if (aiDifficulty == AIDifficulty.Hard)
                StartCoroutine(AiPlayMoveHard());
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinnerRpc(PlayerType winner)
    {
        OnGameWinner?.Invoke(this, winner);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameRestartedRpc()
    {
        OnGameRestarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnTurnPlayedRpc()
    {
        OnTurnPlayed?.Invoke(this, EventArgs.Empty);
    }


    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }
    public PlayerType GetPlayerTypeTurn()
    {
        if (GameStateManager.Instance.IsOnline())
            return playerTypeTurn.Value;
        else
            return remotePlayerTypeTurn;
    }
    public int GetCrossScore()
    {
        if (GameStateManager.Instance.IsOnline())
            return crossScore.Value;
        else
            return remoteCrossScore;
    }
    public int GetCircleScore()
    {
        if (GameStateManager.Instance.IsOnline())
            return circleScore.Value;
        else
            return remoteCircleScore;
    }

    public void SetAIDifficulty(AIDifficulty difficulty)
    {
        aiDifficulty = difficulty;
    }

    private WinResult CheckWin()
    {
        WinResult result = new() { Winner = PlayerType.None, Center = Vector2.zero, Line = LineType.None };

        // horizontal
        for (int x = 0; x < 3; x++)
        {
            if (playerTypeArray[x, 0] != PlayerType.None &&
                playerTypeArray[x, 0] == playerTypeArray[x, 1] &&
                playerTypeArray[x, 1] == playerTypeArray[x, 2])
            {
                result.Winner = playerTypeArray[x, 0];
                result.Line = LineType.Vertical;
                result.Center = new Vector2(x, 1);
                return result;
            }
        }

        // vertical
        for (int y = 0; y < 3; y++)
        {
            if (playerTypeArray[0, y] != PlayerType.None &&
                playerTypeArray[0, y] == playerTypeArray[1, y] &&
                playerTypeArray[1, y] == playerTypeArray[2, y])
            {
                result.Winner = playerTypeArray[0, y];
                result.Line = LineType.Horizontal;
                result.Center = new Vector2(1, y);
                return result;
            }
        }

        // main diagonal
        if (playerTypeArray[0, 2] != PlayerType.None &&
            playerTypeArray[0, 2] == playerTypeArray[1, 1] &&
            playerTypeArray[1, 1] == playerTypeArray[2, 0])
        {
            result.Winner = playerTypeArray[0, 2];
            result.Line = LineType.DiagonalMain;
            result.Center = new Vector2(1, 1);
            return result;
        }

        // anti diagonal
        if (playerTypeArray[0, 0] != PlayerType.None &&
            playerTypeArray[0, 0] == playerTypeArray[1, 1] &&
            playerTypeArray[1, 1] == playerTypeArray[2, 2])
        {
            result.Winner = playerTypeArray[0, 0];
            result.Line = LineType.DiagonalAnti;
            result.Center = new Vector2(1, 1);
            return result;
        }

        return result;
    }

    private IEnumerator AiPlayMoveEasy()
    {
        yield return new WaitForSeconds(1f);

        // Pick a random empty slot
        List<Vector2Int> empty = new List<Vector2Int>();
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (playerTypeArray[x, y] == PlayerType.None)
                    empty.Add(new Vector2Int(x, y));
            }
        }

        if (empty.Count > 0)
        {
            Vector2Int choice = empty[Random.Range(0, empty.Count)];
            Vector2 worldPos = GridBoxList.Instance.GetGridWorldPosition(choice);
            ClickedOnGridBoxRemote(choice, worldPos, PlayerType.Circle);
        }
    }

    private IEnumerator AiPlayMoveMedium()
    {
        yield return new WaitForSeconds(1f);

        Vector2Int choice = SearchBlockWinAI.GetMove(playerTypeArray, PlayerType.Circle);
        Vector2 worldPos = GridBoxList.Instance.GetGridWorldPosition(choice);
        ClickedOnGridBoxRemote(choice, worldPos, PlayerType.Circle);
    }

    private IEnumerator AiPlayMoveHard()
    {
        yield return new WaitForSeconds(1f);

        Vector2Int choice = MinMaxAI.GetBestMove(playerTypeArray, PlayerType.Circle);
        Vector2 worldPos = GridBoxList.Instance.GetGridWorldPosition(choice);
        ClickedOnGridBoxRemote(choice, worldPos, PlayerType.Circle);
    }
}
