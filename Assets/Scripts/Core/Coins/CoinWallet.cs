using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour{


    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private BountyCoin coinPrefab;
    [Header("Settings")]
    [SerializeField] private float coinSpread = 3f;
    [SerializeField] private float bountyPercentage = 50f;
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 5;
    [SerializeField] private LayerMask layerMask;

    private float coinRadius;
    private readonly Collider2D[] coinBuffer = new Collider2D[1];


    public NetworkVariable<int> totalCoins = new NetworkVariable<int>();

    override public void OnNetworkSpawn(){
        if (!IsServer) return;
        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;
        health.OnDie += HandleDie;
    }

    override public void OnNetworkDespawn(){
        if (!IsServer) return;
        health.OnDie -= HandleDie;
    }

    private void HandleDie(Health health){
        var bountyValue = (int)(totalCoins.Value * (bountyPercentage / 100));
        var bountyCoinValue = bountyValue / bountyCoinCount;
        if (bountyCoinValue < minBountyCoinValue) return;
        for (int i = 0; i < bountyCoinValue; i++){
            var coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
            coinInstance.SetValue(bountyCoinValue);
            coinInstance.NetworkObject.Spawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (!other.TryGetComponent(out Coin coin)) return;
        var coinValue = coin.Collect();
        if (!IsServer) return;
        totalCoins.Value += coinValue;
    }

    public void SpendCoins(int costToFire){
        totalCoins.Value -= costToFire;
    }

    private Vector2 GetSpawnPoint(){
        while (true){
            var spawnPoint = (Vector2)transform.position + Random.insideUnitCircle * coinSpread;
            var numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);
            if (numColliders == 0)
                return spawnPoint;
        }
    }
}