using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IDisposable{

    private Allocation allocation;
    private string joinCode;
    private const int MAX_CONNECTIONS = 20;
    private const string GAME_SCENE_NAME = "Game";
    private string lobbyId;

    public NetworkServer NetworkServer{ get; private set; }

    public async Task StartHostAsync(){
        try{
            allocation = await Relay.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
        }
        catch (Exception e){
            Debug.Log(e);
        }
        try{
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
        }
        catch (Exception e){
            Debug.Log(e);
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        var relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);
        try{
            var lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>(){
                {
                    "JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: joinCode)
                }
            };
            var playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            var lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MAX_CONNECTIONS, lobbyOptions);
            lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e){
            Debug.Log(e);
            return;
        }
        NetworkServer = new NetworkServer(NetworkManager.Singleton);
        var userData = new UserData{
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        var payload = JsonUtility.ToJson(userData);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartHost();
        NetworkServer.OnClientLeft += HandleClientLeft;
        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_NAME, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds){
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true){
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public void Dispose(){
        Shutdown();
    }

    public async void Shutdown(){
        if (HostSingleton.Instance != null)
            HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));
        if (!string.IsNullOrEmpty(lobbyId)){
            try{
                await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e){
                Debug.Log(e);
            }
            lobbyId = string.Empty;
        }
        NetworkServer.OnClientLeft -= HandleClientLeft;
        NetworkServer?.Dispose();
    }

    private async void HandleClientLeft(string authId){
        try{
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        }
        catch (LobbyServiceException e){
            Debug.Log(e);
        }
    }
}