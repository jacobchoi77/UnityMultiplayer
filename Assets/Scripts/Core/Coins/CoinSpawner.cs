using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour{

    [SerializeField] private RespawningCoin coinPrefab;
    [SerializeField] private int maxCoins = 50;
    [SerializeField] private int coinValue = 10;
    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;

    [SerializeField] private LayerMask layerMask;

    private float coinRadius;
    private Collider2D[] coinBuffer = new Collider2D[1];

    override public void OnNetworkSpawn(){
        if (!IsServer) return;

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;
        for (int i = 0; i < maxCoins; i++){
            SpawnCoin();
        }
    }

    private void SpawnCoin(){
        var coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
        coinInstance.SetValue(coinValue);
        coinInstance.GetComponent<NetworkObject>().Spawn();
        coinInstance.OnCollected += HandleCoinCollected;
    }

    private void HandleCoinCollected(RespawningCoin coin){
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }

    private Vector2 GetSpawnPoint(){
        while (true){
            var x = Random.Range(xSpawnRange.x, xSpawnRange.y);
            var y = Random.Range(ySpawnRange.x, ySpawnRange.y);
            var spawnPoint = new Vector2(x, y);
            var numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);
            if (numColliders == 0)
                return spawnPoint;
        }
    }
}