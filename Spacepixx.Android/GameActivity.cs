using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Window;
using Microsoft.Xna.Framework;

namespace Spacepixx.Android
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@mipmap/ic_launcher",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Landscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class GameActivity : AndroidGameActivity
    {
        private Spacepixx _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _game = new Spacepixx();
            _view = _game.Services.GetService(typeof(View)) as View;

            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                // Starting from Android API 33, we can support the geasture
                // BACK buttons.
                // When the phone is running on an older API level, we rely
                // on GamePad BACK key handled by the game.
                OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, new OnBackInvokedCallback(_game));
            }

            SetContentView(_view);
            _game.Run();
        }
    }

    class OnBackInvokedCallback : Java.Lang.Object, IOnBackInvokedCallback
    {
        private readonly IBackButtonPressedCallback callback;

        public OnBackInvokedCallback(IBackButtonPressedCallback callback)
        {
            this.callback = callback;
        }

        public void OnBackInvoked()
        {
            callback.BackButtonPressed();
        }
    }
}

