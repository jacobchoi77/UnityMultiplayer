using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class ApplicationController : MonoBehaviour{

    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;

    private async void Start(){
        DontDestroyOnLoad(gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer){
        if (isDedicatedServer){
        }
        else{
            var clientSingleton = Instantiate(clientPrefab);
            var authenticated = await clientSingleton.CreateClient();

            var hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();

            if (authenticated){
                clientSingleton.GameManager.GoToMenu();
            }
        }
    }
}