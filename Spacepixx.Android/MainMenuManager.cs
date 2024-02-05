using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using Spacepixx.Inputs;
using Android.Content;
using Android.App;
using AndroidNet = Android.Net;

namespace Spacepixx
{
    class MainMenuManager
    {
        #region Members

        public enum MenuItems { None, Start, Highscores, Instructions, Settings };

        private MenuItems lastPressedMenuItem = MenuItems.None;

        private Texture2D texture;

        private Rectangle spacepixxSource = new Rectangle(0, 0,
                                                          500, 100);
        private Rectangle spacepixxDestination = new Rectangle(150, 80,
                                                               500, 100);

        private Rectangle startSource = new Rectangle(0, 200,
                                                      300, 50);
        private Rectangle startDestination = new Rectangle(250, 200,
                                                           300, 50);

        private Rectangle instructionsSource = new Rectangle(0, 300,
                                                             300, 50);
        private Rectangle instructionsDestination = new Rectangle(250, 270,
                                                                  300, 50);
        
        private Rectangle highscoresSource = new Rectangle(0, 250,
                                                           300, 50);
        private Rectangle highscoresDestination = new Rectangle(250, 340,
                                                                300, 50);

        private Rectangle settingsSource = new Rectangle(0, 400,
                                                     300, 50);
        private Rectangle settingsDestination = new Rectangle(250, 410,
                                                          300, 50);

        private Rectangle reviewSource = new Rectangle(400, 800,
                                                       100, 100);
        private Rectangle reviewDestination = new Rectangle(690, 380,
                                                            100, 100);
        private Rectangle moreGamesSource = new Rectangle(400, 900,
                                                       100, 100);
        private Rectangle moreGamesDestination = new Rectangle(10, 380,
                                                               100, 100);

        private float opacity = 0.0f;
        private const float OpacityMax = 1.0f;
        private const float OpacityMin = 0.0f;
        private const float OpacityChangeRate = 0.05f;

        private bool isActive = false;

        private float time = 0.0f;

        private GameInput gameInput;
        private const string StartAction = "Start";
        private const string InstructionsAction = "Instructions";
        private const string HighscoresAction = "Highscores";
        private const string SettingsAction = "Settings";
        private const string MoreGamesAction = "MoreGames";
        private const string ReviewAction = "Review";

        #endregion

        #region Constructors

        public MainMenuManager(Texture2D spriteSheet, GameInput input)
        {
            this.texture = spriteSheet;
            this.gameInput = input;
        }

        #endregion

        #region Methods

        public void SetupInputs()
        {
            gameInput.AddTouchGestureInput(StartAction,
                                           GestureType.Tap, 
                                           startDestination);
            gameInput.AddTouchGestureInput(InstructionsAction,
                                           GestureType.Tap,
                                           instructionsDestination);
            gameInput.AddTouchGestureInput(HighscoresAction,
                                           GestureType.Tap,
                                           highscoresDestination);
            gameInput.AddTouchGestureInput(SettingsAction,
                                           GestureType.Tap,
                                           settingsDestination);
            gameInput.AddTouchGestureInput(MoreGamesAction,
                                           GestureType.Tap,
                                           moreGamesDestination);
            gameInput.AddTouchGestureInput(ReviewAction,
                                           GestureType.Tap,
                                           reviewDestination);
        }

        public void Update(GameTime gameTime)
        {
            if (isActive)
            {
                if (this.opacity < OpacityMax)
                    this.opacity += OpacityChangeRate;
            }

            time = (float)gameTime.TotalGameTime.TotalSeconds;

            this.handleTouchInputs();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                             spacepixxDestination,
                             spacepixxSource,
                             Color.Red * opacity);

            spriteBatch.Draw(texture,
                             startDestination,
                             startSource,
                             Color.Red * opacity);

            spriteBatch.Draw(texture,
                             highscoresDestination,
                             highscoresSource,
                             Color.Red * opacity);

            spriteBatch.Draw(texture,
                                instructionsDestination,
                                instructionsSource,
                                Color.Red * opacity);

            spriteBatch.Draw(texture,
                             settingsDestination,
                             settingsSource,
                             Color.Red * opacity);

            spriteBatch.Draw(texture,
                            reviewDestination,
                            reviewSource,
                            Color.Red * opacity);
            spriteBatch.Draw(texture,
                            moreGamesDestination,
                            moreGamesSource,
                            Color.Red * opacity);
        }

        private void handleTouchInputs()
        {
            if (gameInput.IsPressed(StartAction))
            {
                this.lastPressedMenuItem = MenuItems.Start;
            }
            else if (gameInput.IsPressed(HighscoresAction))
            {
                this.lastPressedMenuItem = MenuItems.Highscores;
            }
            else if (gameInput.IsPressed(InstructionsAction))
            {
                this.lastPressedMenuItem = MenuItems.Instructions;
            }
            else if (gameInput.IsPressed(SettingsAction))
            {
                this.lastPressedMenuItem = MenuItems.Settings;
            }
            else if (gameInput.IsPressed(MoreGamesAction))
            {
                var devStoreUri = "https://play.google.com/store/apps/dev?id=4634207615548190812";
                launchInBrowser(devStoreUri);
            }
            else if (gameInput.IsPressed(ReviewAction))
            {
                var packageName = Application.Context.PackageName;
                var appInStoreUri = "https://play.google.com/store/apps/details?id=" + packageName;
                launchInBrowser(appInStoreUri);
            }
            else
            {
                this.lastPressedMenuItem = MenuItems.None;
            }
        }

        private void launchInBrowser(string uri)
        {
            var intent = new Intent(Intent.ActionView, AndroidNet.Uri.Parse(uri))
                .AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(intent);
        }

        #endregion

        #region Activate/Deactivate

        public void Activated(StreamReader reader)
        {
            this.lastPressedMenuItem = (MenuItems)Enum.Parse(lastPressedMenuItem.GetType(), reader.ReadLine(), false);
            this.opacity = Single.Parse(reader.ReadLine());
            this.isActive = Boolean.Parse(reader.ReadLine());
            this.time = Single.Parse(reader.ReadLine());
        }

        public void Deactivated(StreamWriter writer)
        {
            writer.WriteLine(lastPressedMenuItem);
            writer.WriteLine(opacity);
            writer.WriteLine(isActive);
            writer.WriteLine(time);
        }

        #endregion

        #region Properties

        public MenuItems LastPressedMenuItem
        {
            get
            {
                return this.lastPressedMenuItem;
            }
        }

        public bool IsActive
        {
            get
            {
                return this.isActive;
            }
            set
            {
                this.isActive = value;

                if (isActive == false)
                {
                    this.opacity = OpacityMin;
                }
            }
        }

        #endregion
    }
}
