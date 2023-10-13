using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class ApplicationController : MonoBehaviour{

    [SerializeField] private ClientSingleton clientPrefab;

    private async void Start(){
        DontDestroyOnLoad(gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer){
        if (isDedicatedServer){
        }
        else{
            var clientSingleton = Instantiate(clientPrefab);
            await clientSingleton.CreateClient();
            //Go to main menu
        }
    }
}