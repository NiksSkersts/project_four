using System;
using System.Threading;
using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.OS;
using AndroidX.RecyclerView.Widget;

namespace LLU.Android
{
    [Activity(Label = "LLU", MainLauncher = true)]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            //Button button = FindViewById<Button>(Resource.Id.myButton);
            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.mainLayout);
            RecyclerView recyclerView = new RecyclerView(Application.Context);
            recyclerView.SetMinimumHeight(500);
            recyclerView.SetBackgroundColor(color:new Color(0,0,0));
            recyclerView.SetMinimumWidth(500);
            layout.AddView(recyclerView);
            var a =Toast.MakeText(Application.Context, "HELP", ToastLength.Long);
            a.Show();
            //button.Click += delegate { button.Text = $"{count++} clicks!"; };
        }
            
        }
    }