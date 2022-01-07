using Android.Content;
using LLU.Android.Controllers;

namespace LLU.Android.Models;

[BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
public class AlarmHandler : BroadcastReceiver {
    public override void OnReceive(Context context, Intent intent) {
        if (intent?.Extras != null) {
            var title = intent.GetStringExtra(NotificationController.TitleKey);
            var message = intent.GetStringExtra(NotificationController.MessageKey);

            var manager = NotificationController.Instance ?? new NotificationController();
            manager.Show(title, message);
        }
    }
}