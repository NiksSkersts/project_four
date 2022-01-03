using Xamarin.Essentials;

namespace LLU.Android.Controllers; 

/// <summary>
/// 
/// </summary>
public static class ConnectionController {
    public static bool IsConnectedToInternet = GetConnectivity();
    private static bool GetConnectivity() {
        var current = Connectivity.NetworkAccess;
        return current == NetworkAccess.Internet;
    }
}