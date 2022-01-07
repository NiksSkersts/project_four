using Android.App;
using Android.Widget;

namespace LLU.Android.Controllers;

/// <summary>
///     Controller that deals with toast notifications!
/// </summary>
internal static class MessagingController {
    public static void WarningNoRecipients() => MakeToast("Nav ievadīti ziņas saņēmēji!");
    public static void WarningOffline() => MakeToast("Nav pieejams savienojums ar tīklu!");
    public static void ShowSmtpSendError() => MakeToast("Kļūda nosūtot ziņu.");

    public static void ShowConnectionError() => MakeToast("Kļūda pieslēdzoties, mēģiniet vēlreiz.");

    private static void MakeToast(string error) =>
        Toast.MakeText(Application.Context, error, ToastLength.Short)?.Show();
}