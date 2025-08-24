using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartBeatTimer;
    private const float heartbeatInterval = 15f; // seconds
    private float lobbyUpdateTimer;
    private const float lobbyUpdateInterval = 4f; // seconds
    private string playerName;

    private async void Start()
    {
        await UnityServices.InitializeAsync(new InitializationOptions()
            .SetEnvironmentName("production")); // ensure same env as dashboard

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in : " + AuthenticationService.Instance.PlayerId);
        };

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        playerName = "Player" + Random.Range(1000, 9999);
        Debug.Log("Player name set to: " + playerName);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby == null) return;

        heartBeatTimer += Time.deltaTime;
        if (heartBeatTimer >= heartbeatInterval)
        {
            heartBeatTimer = 0f;
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                Debug.Log("Lobby heartbeat sent");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby == null) return;

        lobbyUpdateTimer += Time.deltaTime;
        if (lobbyUpdateTimer >= lobbyUpdateInterval)
        {
            lobbyUpdateTimer = 0f;
            try
            {
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
                Debug.Log("Lobby updated");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }

    [ContextMenu("Create Lobby")]
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "TestLobby";
            int maxPlayers = 2;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayerData()
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = lobby;

            Debug.Log("Lobby name: " + lobby.Name + ", ID:" + lobby.Id + " , Code: " + lobby.LobbyCode);
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [ContextMenu("List Lobbies")]
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Lobby Name: " + lobby.Name + ", ID: " + lobby.Id + ", Players: " + lobby.Players.Count);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayerData()
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyOptions);
            joinedLobby = lobby;
            Debug.Log("Joined lobby: " + joinedLobby.Name + ", ID: " + joinedLobby.Id + ", with Code: " + joinedLobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [ContextMenu("Quick Join Lobby")]
    private async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayerData()
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinOptions);
            joinedLobby = lobby;
            Debug.Log("Quick joined lobby: " + lobby.Name + ", ID: " + lobby.Id + ", Code: " + lobby.LobbyCode);
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void LeaveLobby()
    {
        if (joinedLobby == null) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            Debug.Log("Left lobby: " + joinedLobby.Name + ", ID: " + joinedLobby.Id);
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void KickPlayersByID(string playerId)
    {
        if (hostLobby == null) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, playerId);
            joinedLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
            Debug.Log("Kicked player with ID: " + playerId + " from lobby: " + hostLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void MigrateLobbyHostByID(string newHostId)
    {
        if (hostLobby == null) return;

        try
        {
            await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = newHostId
            });
            joinedLobby = hostLobby;
            hostLobby = null;
            Debug.Log("Migrated lobby host to player with ID: " + newHostId + " in lobby: " + joinedLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void DeleteLobby()
    {
        if (hostLobby == null) return;

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
            Debug.Log("Deleted lobby: " + hostLobby.Name);
            hostLobby = null;
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby: ");
        foreach (Player player in lobby.Players)
        {
            Debug.Log("\tPlayer ID: " + player.Id + ", Name: " + player.Data["PlayerName"].Value);
        }
    }
    private Player GetPlayerData()
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
        return player;
    }
    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId,
            new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

}