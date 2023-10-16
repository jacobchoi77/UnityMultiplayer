using System.Text;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer{

    private NetworkManager networkManager;

    public NetworkServer(NetworkManager networkManager){
        this.networkManager = networkManager;
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
    }

    private static void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response){
        var payload = Encoding.UTF8.GetString(request.Payload);
        var userData = JsonUtility.FromJson<UserData>(payload);
        Debug.Log(userData.userName);
        response.Approved = true;
        response.CreatePlayerObject = true;
    }
}