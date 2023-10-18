using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;
    private readonly List<LeaderboardEntityDisplay> entityDisplays = new List<LeaderboardEntityDisplay>();

    private void Awake(){
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    override public void OnNetworkSpawn(){
        if (IsClient){
            leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
            foreach (var entity in leaderboardEntities){
                HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>{
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }
        if (IsServer){
            var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (var player in players){
                HandlePlayerSpawned(player);
            }
            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }
    }

    private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent){
        switch (changeEvent.Type){
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                if (entityDisplays.All(x => x.ClientId != changeEvent.Value.ClientId)){
                    var leaderboardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                    leaderboardEntity.Initialise(changeEvent.Value.ClientId,
                        changeEvent.Value.PlayerName,
                        changeEvent.Value.Coins);
                    entityDisplays.Add(leaderboardEntity);
                }
                break;

            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                var displayToRemove = entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToRemove != null){
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    entityDisplays.Remove(displayToRemove);
                }
                break;

            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                var displayToUpdate = entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToUpdate != null){
                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;
        }
    }

    override public void OnNetworkDespawn(){
        if (IsClient){
            leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
        }
        if (IsServer){
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
    }

    private void HandlePlayerSpawned(TankPlayer player){
        leaderboardEntities.Add(new LeaderboardEntityState{
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });
    }

    private void HandlePlayerDespawned(TankPlayer player){
        if (leaderboardEntities == null) return;
        foreach (var entity in leaderboardEntities){
            if (entity.ClientId != player.OwnerClientId){ continue; }
            leaderboardEntities.Remove(entity);
            break;
        }
    }
}