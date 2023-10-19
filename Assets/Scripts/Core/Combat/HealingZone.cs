using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour{
    [Header("References")]
    [SerializeField] private Image healPowerBar;
    [Header("Settings")]
    [SerializeField] private int maxHealPower = 30;
    [SerializeField] private float healCooldown = 60f;
    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private int coinsPerTick = 10;
    [SerializeField] private int healthPerTick = 10;

    private float remainingCooldown;
    private float tickTimer;
    private List<TankPlayer> playersInZone = new List<TankPlayer>();
    private NetworkVariable<int> HealPower = new NetworkVariable<int>();

    override public void OnNetworkSpawn(){
        if (IsClient){
            HealPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, HealPower.Value);
        }
        if (IsServer){
            HealPower.Value = maxHealPower;
        }
    }

    override public void OnNetworkDespawn(){
        if (IsClient){
            HealPower.OnValueChanged -= HandleHealPowerChanged;
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (!IsServer) return;
        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out var player)) return;
        playersInZone.Add(player);
    }

    private void OnTriggerExit2D(Collider2D other){
        if (!IsServer) return;
        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out var player)) return;
        playersInZone.Remove(player);
    }

    private void Update(){
        if (!IsServer) return;
        if (remainingCooldown > 0f){
            remainingCooldown -= Time.deltaTime;
            if (remainingCooldown <= 0f){
                HealPower.Value = maxHealPower;
            }
            else{
                return;
            }
        }
        tickTimer += Time.deltaTime;
        if (tickTimer >= 1 / healTickRate){
            foreach (var player in playersInZone){
                if (HealPower.Value == 0) break;
                if (player.Health.CurrentHealth.Value == player.Health.maxHealth){ continue; }
                if (player.Wallet.totalCoins.Value < coinsPerTick) continue;
                player.Wallet.SpendCoins(coinsPerTick);
                player.Health.RestoreHealth(healthPerTick);
                HealPower.Value -= 1;
                if (HealPower.Value == 0){
                    remainingCooldown = healCooldown;
                }
            }
            tickTimer %= 1 / healTickRate;
        }
    }

    private void HandleHealPowerChanged(int oldHealPower, int newHealPower){
        healPowerBar.fillAmount = (float)newHealPower / maxHealPower;
    }
}