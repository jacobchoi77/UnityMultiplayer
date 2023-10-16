using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager{

    private const string MenuSceneName = "Menu";
    private JoinAllocation allocation;

    public async Task<bool> InitAsync(){
        await UnityServices.InitializeAsync();
        var authState = await AuthenticationWrapper.DoAuth();
        if (authState == AuthState.Authenticated){
            return true;
        }
        return false;
    }

    public void GoToMenu(){
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode){
        try{
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e){
            Debug.Log(e);
            return;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        var relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);
        var userData = new UserData{
            userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name")
        };
        var payload = JsonUtility.ToJson(userData);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartClient();
    }
}