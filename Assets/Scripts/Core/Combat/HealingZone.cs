using System.Collections.Generic;
using System.Linq;
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
    private readonly List<TankPlayer> playersInZone = new List<TankPlayer>();
    private readonly NetworkVariable<int> healPower = new NetworkVariable<int>();

    override public void OnNetworkSpawn(){
        if (IsClient){
            healPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, healPower.Value);
        }
        if (IsServer){
            healPower.Value = maxHealPower;
        }
    }

    override public void OnNetworkDespawn(){
        if (IsClient){
            healPower.OnValueChanged -= HandleHealPowerChanged;
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
                healPower.Value = maxHealPower;
            }
            else{
                return;
            }
        }
        tickTimer += Time.deltaTime;
        if (tickTimer >= 1 / healTickRate){
            foreach (var player in playersInZone.TakeWhile(_ => healPower.Value != 0)
                         .Where(player => player.Health.CurrentHealth.Value != player.Health.maxHealth)
                         .Where(player => player.Wallet.totalCoins.Value >= coinsPerTick)){
                player.Wallet.SpendCoins(coinsPerTick);
                player.Health.RestoreHealth(healthPerTick);
                healPower.Value -= 1;
                if (healPower.Value == 0){
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