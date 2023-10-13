using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour{

    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private CoinWallet coinWallet;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;

    private bool shouldFire;
    private float timer;
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
        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void Update(){
        if (muzzleFlashTimer > 0f){
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer <= 0f){
                muzzleFlash.SetActive(false);
            }
        }
        if (!IsOwner) return;
        if (timer > 0)
            timer -= Time.deltaTime;
        if (coinWallet.totalCoins.Value < costToFire) return;
        if (!shouldFire) return;
        if (timer > 0) return;
        var spawnPos = projectileSpawnPoint.position;
        var direction = projectileSpawnPoint.up;
        PrimaryFireServerRpc(spawnPos, direction);
        SpawnDummyProjectile(spawnPos, direction);
        timer = 1 / fireRate;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction){
        if (coinWallet.totalCoins.Value < costToFire) return;
        coinWallet.SpendCoins(costToFire);
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