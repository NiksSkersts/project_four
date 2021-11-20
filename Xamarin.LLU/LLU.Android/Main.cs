using Android.App;
using Android.Runtime;
using System;

namespace LLU.Android
{
    [Application]
    public class Main : Application
    {
        // Add Xiconify package by overriding Application.
        // More information:
        // https://github.com/PragmaticIT/xiconify
        public Main () : base() { }
        public Main(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        public override void OnCreate()
        {
            base.OnCreate();
            JoanZapata.XamarinIconify.Iconify
                .With(new JoanZapata.XamarinIconify.Fonts.FontAwesomeModule())
                .With(new JoanZapata.XamarinIconify.Fonts.FontAwesomeModule())
                ;
        }
    }
}