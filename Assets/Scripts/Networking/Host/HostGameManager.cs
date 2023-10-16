using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager{

    private Allocation allocation;
    private string joinCode;
    private const int MaxConnections = 20;
    private const string GameSceneName = "Game";
    private string lobbyId;
    private NetworkServer networkServer;

    public async Task StartHostAsync(){
        try{
            allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
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
            var playerName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Unknown");
            var lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, lobbyOptions);
            lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e){
            Debug.Log(e);
            return;
        }
        networkServer = new NetworkServer(NetworkManager.Singleton);
        var userData = new UserData{
            userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name")
        };
        var payload = JsonUtility.ToJson(userData);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds){
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true){
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}