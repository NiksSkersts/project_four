﻿using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.App;
using Android.Widget;
using LLU.Controllers;
using LLU.Models;

namespace LLU.Android.Controllers;

internal static class MessagingController {
    public static void WarningNoRecipients() => MakeToast("Nav ievadīti ziņas saņēmēji!");
    public static void WarningOffline() => MakeToast("Nav pieejams savienojums ar tīklu!");
    public static void ShowSmtpSendError() => MakeToast("Kļūda nosūtot ziņu.");

    public static void ShowConnectionError() => MakeToast("Kļūda pieslēdzoties, mēģiniet vēlreiz.");

    private static void MakeToast(string error) =>
        Toast.MakeText(Application.Context, error, ToastLength.Short)?.Show();
}