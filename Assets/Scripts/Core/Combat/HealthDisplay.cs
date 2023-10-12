using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour{
    [Header("References")] [SerializeField] private Health health;
    [SerializeField] private Image healthBarImage;

    override public void OnNetworkSpawn(){
        if (!IsClient) return;
        health.CurrentHealth.OnValueChanged += HandleHealthChanged;
        HandleHealthChanged(0, health.CurrentHealth.Value);
    }

    override public void OnNetworkDespawn(){
        if (!IsClient) return;
        health.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int oldHealth, int newHealth){
        healthBarImage.fillAmount = (float)newHealth / health.maxHealth;
    }
}