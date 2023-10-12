using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour{
    [SerializeField] private int damage = 5;
    private ulong ownerClientId;

    public void SetOwner(ulong ownerClientId){
        this.ownerClientId = ownerClientId;
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.TryGetComponent(out NetworkObject netObj)){
            if (netObj.OwnerClientId == ownerClientId) return;
        }
        if (other.attachedRigidbody.TryGetComponent(out Health health)){
            health.TakeDamage(damage);
        }
    }
}