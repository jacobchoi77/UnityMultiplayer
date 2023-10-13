using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class CoinWallet : NetworkBehaviour{
    public NetworkVariable<int> totalCoins = new NetworkVariable<int>();

    private void OnTriggerEnter2D(Collider2D other){
        if (!other.TryGetComponent(out Coin coin)) return;
        var coinValue = coin.Collect();
        if (!IsServer) return;
        totalCoins.Value += coinValue;
    }

    public void SpendCoins(int costToFire){
        totalCoins.Value -= costToFire;
    }
}