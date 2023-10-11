using Unity.Netcode.Components;

public class ClientNetworkTransform : NetworkTransform{
    override public void OnNetworkSpawn(){
        base.OnNetworkSpawn();
        CanCommitToTransform = IsOwner;
    }

    protected override void Update(){
        CanCommitToTransform = IsOwner;
        base.Update();
        if (NetworkManager != null){
            if (NetworkManager.IsConnectedClient || NetworkManager.IsListening){
                if (CanCommitToTransform){
                    TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
                }
            }
        }
    }

    protected override bool OnIsServerAuthoritative(){
        return false;
    }
}