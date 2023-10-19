using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour{
    [SerializeField] private TankPlayer playerPrefab;
    [SerializeField] private float keptCoinPercentage;

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
        var keptCoins = (int)(player.Wallet.totalCoins.Value * (keptCoinPercentage / 100));
        Destroy(player.gameObject);
        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins){
        yield return null;
        var playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
        playerInstance.Wallet.totalCoins.Value += keptCoins;
    }
}