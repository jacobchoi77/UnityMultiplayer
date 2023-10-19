using System;
using Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [field: SerializeField] public Health Health{ get; private set; }

    [field: SerializeField] public CoinWallet Wallet{ get; private set; }

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;

    public static event Action<TankPlayer> OnPlayerSpawned;

    public static event Action<TankPlayer> OnPlayerDespawned;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    override public void OnNetworkSpawn(){
        if (IsServer){
            var userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.userName;
            OnPlayerSpawned?.Invoke(this);
        }
        if (IsOwner){
            virtualCamera.Priority = ownerPriority;
        }
    }

    override public void OnNetworkDespawn(){
        if (IsServer){
            OnPlayerDespawned?.Invoke(this);
        }
    }
}