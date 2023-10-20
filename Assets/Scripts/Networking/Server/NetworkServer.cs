using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable{

    private readonly NetworkManager networkManager;
    private readonly Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    private readonly Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
    public Action<string> OnClientLeft;

    public NetworkServer(NetworkManager networkManager){
        this.networkManager = networkManager;
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }

    private void OnNetworkReady(){
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId){
        if (clientIdToAuth.TryGetValue(clientId, out var authId)){
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
            OnClientLeft?.Invoke(authId);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response){
        var payload = Encoding.UTF8.GetString(request.Payload);
        var userData = JsonUtility.FromJson<UserData>(payload);
        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdToUserData[userData.userAuthId] = userData;
        response.Approved = true;
        response.Position = SpawnPoint.GetRandomSpawnPos();
        response.Rotation = Quaternion.identity;
        response.CreatePlayerObject = true;
    }

    public UserData GetUserDataByClientId(ulong clientId){
        if (clientIdToAuth.TryGetValue(clientId, out var authId))
            if (authIdToUserData.TryGetValue(authId, out var data))
                return data;
        return null;
    }

    public void Dispose(){
        if (networkManager == null) return;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnServerStarted -= OnNetworkReady;

        if (networkManager.IsListening){
            networkManager.Shutdown();
        }
    }
}