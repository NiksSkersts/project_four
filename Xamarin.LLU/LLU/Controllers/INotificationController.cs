using System;

namespace LLU.Android.Controllers;

public interface INotificationController {
    event EventHandler NotificationReceived;
    void Initialize();

    //todo create an app-wide notification system!
    /// <summary>
    ///     <para>
    ///         Apps that are running on Android 8.0 must create a notification channel for their notifications. A notification
    ///         channel requires the following three pieces of information:
    ///         An ID string that is unique to the package that will identify the channel.
    ///         The name of the channel that will be displayed to the user. The name must be between one and 40 characters.
    ///         The importance of the channel.
    ///         Apps will need to check the version of Android that they are running. Devices running versions older than
    ///         Android 8.0 should not create a notification channel.
    ///     </para>
    ///     <para>
    ///         Notification channels are new in API 26 (and not a part of the support library). There is no need to create a
    ///         notification channel on older versions of Android.
    ///     </para>
    ///     <summary>
    ///         <a href="https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/notifications/local-notifications">
    ///             Source:
    ///             Xamarin local notifications
    ///         </a>
    ///         <a
    ///             href="https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/notifications/local-notifications-walkthrough">
    ///             Source:
    ///             Walkthrough Xam. loc. notif.
    ///         </a>
    ///     </summary>
    /// </summary>
    void SendNotification(string title, string message, DateTime? notifyTime = null);

    void ReceiveNotification(string title, string message);
}