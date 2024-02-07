using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Window;
using GooglePlay.Services.Helpers;
using Microsoft.Xna.Framework;
using Spacepixx.Android.Ads;

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
        private const string HIGHSCORES_ID = "CgkInOeWhe4fEAIQAQ";

#if DEBUG
        private const string AD_UNIT_ID = "ca-app-pub-3940256099942544/1033173712";
#else
        private const string AD_UNIT_ID = "ca-app-pub-8102925760359189/7277709816";
#endif

        private Spacepixx _game;
        private View _view;

        private GameHelper gameHelper;
        private AdMobService adMobService;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _game = new Spacepixx(
                ShowLeaderboardsHandler, SubmitLeaderboardsScore,
                StartNewGameHandler, GameOverEndedHandler,
                IsPrivacyOptionsRequiredHanlder, ShowPrivacyConsentFormHandler);
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
            InitializeServices();

            if (gameHelper != null && gameHelper.SignedOut)
            {
                gameHelper.SignIn();
            }
            _game.Run();
        }

        void InitializeServices()
        {
            // Setup Google Play Services Helper
            gameHelper = new GameHelper(this);
            // Set Gravity and View for Popups
            gameHelper.GravityForPopups = (GravityFlags.Top | GravityFlags.Center);
            gameHelper.ViewForPopups = _view;
            // Hook up events
            gameHelper.OnSignedIn += (object sender, EventArgs e) => {
                Log.Info("GameActivity", "Signed in");
            };
            gameHelper.OnSignInFailed += (object sender, EventArgs e) => {
                Log.Info("GameActivity", "Signed in failed!");
            };

            gameHelper.Initialize();

            adMobService = new AdMobService(this);
            adMobService.Initialize();
        }

        private void ShowLeaderboardsHandler()
        {
            if (gameHelper != null && !gameHelper.SignedOut)
            {
                gameHelper.ShowAllLeaderBoardsIntent();
            }
        }

        private void SubmitLeaderboardsScore(long score)
        {
            if (gameHelper != null && !gameHelper.SignedOut)
            {
                gameHelper.SubmitScore(HIGHSCORES_ID, score);
            }
        }

        private void StartNewGameHandler()
        {
            adMobService.LoadInterstitial(AD_UNIT_ID);
        }

        private void GameOverEndedHandler()
        {
            adMobService.ShowInterstitial();
        }

        private bool IsPrivacyOptionsRequiredHanlder()
        {
            return adMobService.IsPrivacyOptionsRequired;
        }

        private void ShowPrivacyConsentFormHandler()
        {
            adMobService.ShowPrivacyConsentForm();
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (gameHelper != null)
                gameHelper.Start();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (gameHelper != null)
                gameHelper.OnActivityResult(requestCode, resultCode, data);
            base.OnActivityResult(requestCode, resultCode, data);
        }

        protected override void OnStop()
        {
            if (gameHelper != null)
                gameHelper.Stop();
            base.OnStop();
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

