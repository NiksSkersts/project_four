using Android.App;
using Android.Widget;
using System;

namespace LLU.Android.Controllers
{
    internal static class MessagingController
    {
        public static void ShowSMTPSendError() => MakeToast("Kļūda nosūtot ziņu.");
        public static void ShowConnnectionError() => MakeToast("Kļūda pieslēdzoties, mēģiniet vēlreiz.");
        public static void ShowConnnectionErrorInternalFailure() => MakeToast("Kļūda pieslēdzoties. Iekšēja datu kļūda.");
        public static void ShowConnnectionErrorServerFailure() => MakeToast("Serveris nav pieejams! \n Lūdzu uzgaidiet.");
        internal static void AuthentificationError() => MakeToast("Autentifikācijas kļūda! Uzrādīts nepareizs lietotājvārds vai parole.");
        internal static void MakeToast(string error) => Toast.MakeText(Application.Context, error, ToastLength.Short).Show();
    }
}