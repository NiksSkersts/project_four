using Android.App;
using Android.Widget;

namespace LLU.Android.Controllers
{
    internal static class MessagingManager
    {
        public static void ShowConnnectionError() => Toast.MakeText(Application.Context, "Kļūda pieslēdzoties, mēģiniet vēlreiz.", ToastLength.Short);
        public static void ShowConnnectionErrorInternalFailure() => Toast.MakeText(Application.Context, "Kļūda pieslēdzoties. Iekšēja datu kļūda.", ToastLength.Short);
        public static void ShowConnnectionErrorServerFailure() => Toast.MakeText(Application.Context, "Serveris nav pieejams! \n Lūdzu uzgaidiet.", ToastLength.Short).Show();
    }
}