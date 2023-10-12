using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour{
    [SerializeField] private Transform turretTransform;
    [SerializeField] private InputReader inputReader;

    private void LateUpdate(){
        if (!IsOwner) return;
        var aimScreenPosition = inputReader.AimPosition;
        var aimWorldPosition = Camera.main.ScreenToWorldPoint(aimScreenPosition);
        turretTransform.up = new Vector2(aimWorldPosition.x - turretTransform.position.x,
            aimWorldPosition.y - turretTransform.position.y);
    }

}