using Xamarin.Essentials;

namespace LLU.Android.Controllers; 

/// <summary>
/// Controller that deals with network connection
/// </summary>
public static class ConnectionController {
    public static bool IsConnectedToInternet = GetConnectivity();
    private static bool GetConnectivity() {
        var current = Connectivity.NetworkAccess;
        return current == NetworkAccess.Internet;
    }
}