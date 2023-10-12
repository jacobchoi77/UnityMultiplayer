using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour{

    [Header("References")] [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;

    [Header("Settings")] [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    private bool shouldFire;
    private float previousFireTime;
    private float muzzleFlashTimer;

    override public void OnNetworkSpawn(){
        if (!IsOwner) return;
        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    private void HandlePrimaryFire(bool shouldFire){
        this.shouldFire = shouldFire;
    }

    override public void OnNetworkDespawn(){
        if (!IsOwner) return;
    }

    private void Update(){
        if (muzzleFlashTimer > 0f){
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer <= 0f){
                muzzleFlash.SetActive(false);
            }
        }
        if (!IsOwner) return;
        if (!shouldFire) return;
        if (Time.time < 1 / fireRate + previousFireTime) return;
        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
        previousFireTime = Time.time;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction){
        var projectileInstance = Instantiate(serverProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

        if (projectileInstance.TryGetComponent(out DealDamageOnContact dealDamage)){
            dealDamage.SetOwner(OwnerClientId);
        }
        if (projectileInstance.TryGetComponent(out Rigidbody2D rb)){
            rb.velocity = rb.transform.up * projectileSpeed;
        }
        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction){
        if (IsOwner) return;
        SpawnDummyProjectile(spawnPos, direction);
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction){
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;
        var projectileInstance = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        if (projectileInstance.TryGetComponent(out Rigidbody2D rb)){
            rb.velocity = rb.transform.up * projectileSpeed;
        }
    }
}