using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour{
    [SerializeField] private NetworkObject playerPrefab;

    override public void OnNetworkSpawn(){
        if (!IsServer) return;
        var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach (var player in players){
            HandlePlayerSpawned(player);
        }
        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }

    override public void OnNetworkDespawn(){
        if (!IsServer) return;
        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(TankPlayer player){
        player.Health.OnDie += _ => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawned(TankPlayer player){
        player.Health.OnDie -= _ => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(TankPlayer player){
        Destroy(player.gameObject);
        StartCoroutine(RespawnPlayer(player.OwnerClientId));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId){
        yield return null;
        var playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
        playerInstance.SpawnAsPlayerObject(ownerClientId);
    }
}