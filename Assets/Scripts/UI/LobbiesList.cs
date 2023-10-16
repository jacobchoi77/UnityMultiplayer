using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour{
    [SerializeField] private LobbyItem lobbyItemPrefab;
    [SerializeField] private Transform lobbyItemParent;
    private bool isJoining;
    private bool isRefreshing;

    private void OnEnable(){
        RefreshList();
    }

    public async void RefreshList(){
        if (isRefreshing) return;
        isRefreshing = true;
        try{
            var options = new QueryLobbiesOptions{
                Count = 25,
                Filters = new List<QueryFilter>(){
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"),
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.IsLocked,
                        op: QueryFilter.OpOptions.EQ,
                        value: "0")
                }
            };
            var lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            foreach (Transform child in lobbyItemParent){
                Destroy(child.gameObject);
            }
            foreach (var lobby in lobbies.Results){
                var lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Initialise(this, lobby);
            }
        }
        catch (LobbyServiceException e){
            Debug.Log(e);
        }

        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby){
        if (isJoining) return;
        isJoining = true;
        try{
            var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            var joinCode = joiningLobby.Data["JoinCode"].Value;
            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException e){
            Debug.Log(e);
        }
        isJoining = false;
    }
}