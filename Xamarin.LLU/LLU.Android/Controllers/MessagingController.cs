using Android.App;
using Android.Widget;

namespace LLU.Android.Controllers;

internal static class MessagingController {
    public static void ShowSmtpSendError() => MakeToast("Kļūda nosūtot ziņu.");

    public static void ShowConnectionError() => MakeToast("Kļūda pieslēdzoties, mēģiniet vēlreiz.");

    private static void MakeToast(string error) =>
        Toast.MakeText(Application.Context, error, ToastLength.Short)?.Show();
}