using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField] private Transform crossTransform;
    [SerializeField] private Transform circleTransform;
    [SerializeField] private Transform vertLineTransform;
    [SerializeField] private Transform horzLineTransform;
    [SerializeField] private Transform diagLineTransform;
    [SerializeField] private Transform antiDiagLineTransform;

    private List<Transform> spawnedObjects;

    private void Start()
    {
        GameManager.Instance.OnGridBoxClicked += GameManager_OnGridBoxClicked;
        GameManager.Instance.OnGameEnded += GameManager_OnGameEnded;
        GameManager.Instance.OnGameRestarted += GameManager_OnGameRestarted;

        spawnedObjects = new List<Transform>();
    }

    private void GameManager_OnGridBoxClicked(object sender, GameManager.OnGridBoxClickedEventArgs e)
    {
        Debug.Log("Grid position clicked: " + e.GridPosition);
        if (GameStateManager.Instance.IsOnline())
        {
            SpawnObjectRpc(e.GridPosition, e.PlayerType);
        }
        else
        {
            SpawnObjectRemote(e.GridPosition, e.PlayerType);
        }
    }

    private void GameManager_OnGameEnded(object sender, GameManager.WinResult e)
    {
        Transform objectToSpawn;
        switch (e.Line)
        {
            case GameManager.LineType.Horizontal:
                objectToSpawn = horzLineTransform;
                break;
            case GameManager.LineType.Vertical:
                objectToSpawn = vertLineTransform;
                break;
            case GameManager.LineType.DiagonalMain:
                objectToSpawn = diagLineTransform;
                break;
            case GameManager.LineType.DiagonalAnti:
                objectToSpawn = antiDiagLineTransform;
                break;
            case GameManager.LineType.None:
                return;
            default:
                Debug.LogError("Invalid winning line!");
                return;
        }

        Transform spawnedObject = Instantiate(objectToSpawn, e.Center, Quaternion.identity);
        spawnedObjects.Add(spawnedObject);
        if (GameStateManager.Instance.IsOnline())
            spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }

    private void GameManager_OnGameRestarted(object sender, EventArgs e)
    {
        foreach (Transform spawnedObject in spawnedObjects)
        {
            Destroy(spawnedObject.gameObject);
        }
        spawnedObjects.Clear();
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(Vector2 position, GameManager.PlayerType playerType)
    {
        Debug.Log("Spawning " + playerType + " at position: " + position);

        Transform objectToSpawn;
        switch (playerType)
        {
            case GameManager.PlayerType.Cross:
                objectToSpawn = crossTransform;
                break;
            case GameManager.PlayerType.Circle:
                objectToSpawn = circleTransform;
                break;
            default:
                Debug.LogError("Invalid player type");
                return;
        }

        Transform spawnedObject = Instantiate(objectToSpawn, position, Quaternion.identity);
        spawnedObjects.Add(spawnedObject);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }

    private void SpawnObjectRemote(Vector2 position, GameManager.PlayerType playerType)
    {
        Debug.Log("Spawning " + playerType + " at position: " + position);

        Transform objectToSpawn;
        switch (playerType)
        {
            case GameManager.PlayerType.Cross:
                objectToSpawn = crossTransform;
                break;
            case GameManager.PlayerType.Circle:
                objectToSpawn = circleTransform;
                break;
            default:
                Debug.LogError("Invalid player type");
                return;
        }

        Transform spawnedObject = Instantiate(objectToSpawn, position, Quaternion.identity);
        spawnedObjects.Add(spawnedObject);
    }
}
