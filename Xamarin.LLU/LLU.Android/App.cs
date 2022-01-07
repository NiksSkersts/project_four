using System;
using Android.App;
using Android.Runtime;
using Java.Lang;
using LLU.Android.Controllers;
using Unity;

namespace LLU.Android;

/// <summary>
/// Provides dependency injection for app-wide notification system.
/// <br></br>
/// todo: Unity Container is no longer in active development. Find a replacement.
/// </summary>
[Application]
public class App : Application
{
    [Deprecated]
    public static UnityContainer Container { get; set; }

    public App( IntPtr javaRef, JniHandleOwnership transfer ) : base( javaRef, transfer )
    {

    }

    public override void OnCreate( )
    {
        base.OnCreate( );

        Container = new UnityContainer( );

        Container.RegisterType<INotificationController, NotificationController>( );
    }
}