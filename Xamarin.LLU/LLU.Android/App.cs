using System;
using Android.App;
using Android.Runtime;
using LLU.Android.Controllers;
using Unity;

namespace LLU.Android; 

[Application]
public class App : Application
{
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