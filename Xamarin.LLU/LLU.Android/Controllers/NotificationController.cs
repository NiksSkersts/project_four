using System;
using System.Runtime.CompilerServices;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using LLU.Android.Models;
using LLU.Android.Views;
using AndroidApp = Android.App.Application;
using String = Java.Lang.String;

[assembly: Dependency("NotificationController", LoadHint.Always)]

namespace LLU.Android.Controllers; 

/// <summary>
///     <a href="https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/local-notifications">
///         More info at
///         Microsoft docs.
///     </a>
/// </summary>
public class NotificationController : INotificationController {
    private const string channelId = "default";
    private const string channelName = "Default";
    private const string channelDescription = "The default channel for notifications.";

    public const string TitleKey = "title";
    public const string MessageKey = "message";

    private bool channelInitialized;

    private NotificationManager manager;
    private int messageId;
    private int pendingIntentId;

    public NotificationController() => Initialize();

    public static NotificationController Instance { get; private set; }

    public event EventHandler NotificationReceived;

    public void Initialize() {
        if (Instance == null) {
            CreateNotificationChannel();
            Instance = this;
        }
    }

    public void SendNotification(string title, string message, DateTime? notifyTime = null) {
        if (!channelInitialized) CreateNotificationChannel();

        if (notifyTime != null) {
            var intent = new Intent(AndroidApp.Context, typeof(AlarmHandler));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);

            var pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, pendingIntentId++, intent,
                PendingIntentFlags.CancelCurrent);
            var triggerTime = GetNotifyTime(notifyTime.Value);
            var alarmManager = AndroidApp.Context.GetSystemService(Context.AlarmService) as AlarmManager;
            alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
        }
        else {
            Show(title, message);
        }
    }

    public void ReceiveNotification(string title, string message) {
        var args = new NotificationEventArgs {
            Title = title,
            Message = message
        };
        NotificationReceived?.Invoke(null, args);
    }

    public void Show(string title, string message) {
        var intent = new Intent(AndroidApp.Context, typeof(EmailActivity));
        intent.PutExtra(TitleKey, title);
        intent.PutExtra(MessageKey, message);

        var pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId++, intent,
            PendingIntentFlags.UpdateCurrent);

        var builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
            .SetContentIntent(pendingIntent)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources,
                Resource.Drawable.ic_calendar_black_24dp))
            .SetSmallIcon(Resource.Drawable.ic_calendar_black_24dp)
            .SetDefaults((int) NotificationDefaults.Sound | (int) NotificationDefaults.Vibrate);

        var notification = builder.Build();
        manager.Notify(messageId++, notification);
    }

    private void CreateNotificationChannel() {
        manager = (NotificationManager) AndroidApp.Context.GetSystemService(Context.NotificationService);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {
            var channelNameJava = new String(channelName);
            var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default) {
                Description = channelDescription
            };
            manager.CreateNotificationChannel(channel);
        }

        channelInitialized = true;
    }

    private long GetNotifyTime(DateTime notifyTime) {
        var utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
        var epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
        var utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;
        return utcAlarmTime; // milliseconds
    }
}