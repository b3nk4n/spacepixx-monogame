﻿using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Spacepixx.Inputs;

namespace Spacepixx;

public class Spacepixx : Game, IBackButtonPressedCallback
{
    /*
     * The game's fixed width and heigth of the screen.
     * Do NOT change this number, because other related code uses hard
     * coded values similar to this one and assumes that this is the screen
     * dimension. This value values was fixed in Windows Phone,
     * but there are very different screen out there in Android.
     */
    const int WIDTH = 800;
    const int HEIGHT = 480;
    Matrix screenScaleMatrix;
    Vector2 screenScaleVector;

    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    private const string GameOverText = "GAME OVER!";

    private readonly Random rand = new Random();
    private static readonly string[] GetReadyText = { "Get Ready!", "Rock 'n' Roll!", "Rock on!", "Fight!", "Attack!" };

    private const string ContinueText = "Push to continue...";
    private string VersionText;
    private const string MusicByText = "Music by";
    private const string MusicCreatorText = "Tscho";
    private const string CreatorText = "by Benjamin Kan";

    enum GameStates
    {
        TitleScreen, MainMenu, Instructions, Settings, Playing, BossDuell, Paused, PlayerDead, GameOver,
        Submittion, PhonePosition
    };
    GameStates gameState = GameStates.TitleScreen;
    GameStates stateBeforePaused;
    Texture2D spriteSheet;
    Texture2D menuSheet;

    StarFieldManager starFieldManager1;
    StarFieldManager starFieldManager2;
    StarFieldManager starFieldManager3;

    AsteroidManager asteroidManager;

    PlayerManager playerManager;

    EnemyManager enemyManager;
    BossManager bossManager;

    CollisionManager collisionManager;

    SpriteFont pericles16;
    SpriteFont pericles18;

    ZoomTextManager zoomTextManager;

    private float playerDeathTimer = 0.0f;
    private const float playerDeathDelayTime = 6.0f;
    private const float playerGameOverDelayTime = 5.0f;

    private float titleScreenTimer = 0.0f;
    private const float titleScreenDelayTime = 1.0f;

    private Vector2 playerStartLocation = new Vector2(375, 375);

    Hud hud;

    SubmissionManager submissionManager;

    MainMenuManager mainMenuManager;

    private float backButtonTimer = 0.0f;
    private const float backButtonDelayTime = 0.25f;

    LevelManager levelManager;

    InstructionManager instructionManager;

    PowerUpManager powerUpManager;
    Texture2D powerUpSheet;

    SettingsManager settingsManager;

    private bool bossDirectKill = true;
    private const long InitialBossBonusScore = 5000;
    private long bossBonusScore = InitialBossBonusScore;

    GameInput gameInput = new GameInput();
    private const string TitleAction = "Title";
    private const string BackToGameAction = "BackToGame";
    private const string BackToMainAction = "BackToMain";

    private readonly Rectangle cancelSource = new Rectangle(0, 900,
                                                            300, 50);
    private readonly Rectangle cancelDestination = new Rectangle(450, 370,
                                                                 300, 50);

    private readonly Rectangle continueSource = new Rectangle(0, 850,
                                                              300, 50);
    private readonly Rectangle continueDestination = new Rectangle(50, 370,
                                                                   300, 50);

    PhonePositionManager phonePositionManager;

    private bool backButtonPressed = false;

    public delegate void ShowLeaderboards();
    private readonly ShowLeaderboards showLeaderboards;

    public delegate void SubmitLeaderboardScore(long score);
    private readonly SubmitLeaderboardScore submitLeaderboardScore;

    public delegate void StartNewGame();
    private readonly StartNewGame startNewGameCallback;

    public delegate void GameOverEnded();
    private readonly GameOverEnded gameOverEndedCallback;

    public delegate bool IsPrivacyConsentRequired();
    private readonly IsPrivacyConsentRequired isPrivacyRequiredSupplier;

    public delegate void ShowPrivacyConsent();
    private readonly ShowPrivacyConsent showPrivacyConsentCallback;

    public Spacepixx(
        ShowLeaderboards showLeaderboards, SubmitLeaderboardScore submitLeaderboardScore,
        StartNewGame startNewGame, GameOverEnded gameOverEnded,
        IsPrivacyConsentRequired isPrivacyRequired, ShowPrivacyConsent showPrivacyConsent)
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);

        Content.RootDirectory = "Content";

        // Frame rate is 60 fps
        TargetElapsedTime = TimeSpan.FromTicks(166667);

        this.showLeaderboards = showLeaderboards;
        this.submitLeaderboardScore = submitLeaderboardScore;
        this.startNewGameCallback = startNewGame;
        this.gameOverEndedCallback = gameOverEnded;
        this.isPrivacyRequiredSupplier = isPrivacyRequired;
        this.showPrivacyConsentCallback = showPrivacyConsent;
    }

    void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
    {
        e.GraphicsDeviceInformation.PresentationParameters.PresentationInterval = PresentInterval.One;
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
        // The fullscreen mode on Android in MonoGame is unfortunately a bit broken,
        // and does not correctly incorporate the hidden status bar,
        // or gesture button controls. And therefore, the reported back-buffer
        // resolution is incorrect, and we are scaling the screen incorrectly.
        // See for example:
        // https://community.monogame.net/t/graphicsdevice-presentationparameters-has-wrong-backbuffer-size-on-vanilla-android/16412
        // https://community.monogame.net/t/full-screen-on-android/19136/5
        // So we need to live with non-fullscreen mode for now...
        // graphics.IsFullScreen = true;

        graphics.PreferredBackBufferHeight = HEIGHT;
        graphics.PreferredBackBufferWidth = WIDTH;
        graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;

        // Aplly the gfx changes
        graphics.ApplyChanges();

        // calculate scaling matrix/vector to fit everything to the assumed screen bounds
        var bw = GraphicsDevice.PresentationParameters.BackBufferWidth;
        var bh = GraphicsDevice.PresentationParameters.BackBufferHeight;
        screenScaleVector = new Vector2((float)bw / WIDTH, (float)bh / HEIGHT);
        screenScaleMatrix = Matrix.Identity * Matrix.CreateScale(screenScaleVector.X, screenScaleVector.Y, 0f);

        // Because we are using a different virtual scale compared to the
        // physical resolution of the screen, using a transformation matrix
        // of the SpriteBatch, we need to change the display for the touch
        // panel the same way
        TouchPanel.DisplayOrientation = DisplayOrientation.LandscapeLeft;
        TouchPanel.DisplayHeight = HEIGHT;
        TouchPanel.DisplayWidth = WIDTH;
        TouchPanel.EnabledGestures = GestureType.Tap;

        loadVersion();

        base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        // Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = new SpriteBatch(GraphicsDevice);

        spriteSheet = Content.Load<Texture2D>(@"Textures\SpriteSheet");
        menuSheet = Content.Load<Texture2D>(@"Textures\MenuSheet");
        powerUpSheet = Content.Load<Texture2D>(@"Textures\PowerUpSheet");

        starFieldManager1 = new StarFieldManager(WIDTH,
                                                HEIGHT,
                                                100,
                                                new Vector2(0, 20.0f),
                                                spriteSheet,
                                                new Rectangle(0, 350, 1, 1));
        starFieldManager2 = new StarFieldManager(WIDTH,
                                                HEIGHT,
                                                70,
                                                new Vector2(0, 40.0f),
                                                spriteSheet,
                                                new Rectangle(0, 350, 2, 2));
        starFieldManager3 = new StarFieldManager(WIDTH,
                                                HEIGHT,
                                                30,
                                                new Vector2(0, 60.0f),
                                                spriteSheet,
                                                new Rectangle(0, 350, 3, 3));

        asteroidManager = new AsteroidManager(3,
                                              spriteSheet,
                                              new Rectangle(0, 0, 50, 50),
                                              20,
                                              WIDTH,
                                              HEIGHT);

        playerManager = new PlayerManager(spriteSheet,
                                          new Rectangle(0, 150, 50, 50),
                                          6,
                                          new Rectangle(0, 0,
                                                        WIDTH,
                                                        HEIGHT),
                                          playerStartLocation,
                                          gameInput);

        enemyManager = new EnemyManager(spriteSheet,
                                        playerManager,
                                        new Rectangle(0, 0,
                                                      WIDTH,
                                                      HEIGHT));

        bossManager = new BossManager(spriteSheet,
                                      playerManager,
                                      new Rectangle(0, 0,
                                                    WIDTH,
                                                    HEIGHT));
        Boss.Player = playerManager;

        EffectManager.Initialize(spriteSheet,
                                 new Rectangle(0, 350, 2, 2),
                                 new Rectangle(0, 100, 50, 50),
                                 5);

        powerUpManager = new PowerUpManager(powerUpSheet);

        collisionManager = new CollisionManager(asteroidManager,
                                                playerManager,
                                                enemyManager,
                                                bossManager,
                                                powerUpManager);

        SoundManager.Initialize(Content);

        pericles16 = Content.Load<SpriteFont>(@"Fonts\Pericles16");
        pericles18 = Content.Load<SpriteFont>(@"Fonts\Pericles18");

        zoomTextManager = new ZoomTextManager(new Vector2(WIDTH / 2,
                                                          HEIGHT / 2),
                                                          pericles16);

        hud = Hud.GetInstance(new Rectangle(0, 0, WIDTH, HEIGHT),
                              spriteSheet,
                              pericles16,
                              0,
                              3,
                              0.0f,
                              100.0f,
                              0.0f,
                              3,
                              10,
                              PlayerManager.MIN_SCORE_MULTI,
                              1,
                              bossManager);

        submissionManager = SubmissionManager.GetInstance();
        SubmissionManager.Font = pericles18;
        SubmissionManager.Texture = menuSheet;
        SubmissionManager.GameInput = gameInput;

        mainMenuManager = new MainMenuManager(menuSheet, gameInput);

        levelManager = new LevelManager();
        levelManager.Register(asteroidManager);
        levelManager.Register(enemyManager);
        levelManager.Register(bossManager);
        levelManager.Register(playerManager);

        instructionManager = new InstructionManager(spriteSheet,
                                                    pericles18,
                                                    new Rectangle(0, 0,
                                                                  WIDTH, HEIGHT),
                                                    asteroidManager,
                                                    playerManager,
                                                    enemyManager,
                                                    bossManager,
                                                    powerUpManager);

        SoundManager.PlayBackgroundSound();


        settingsManager = SettingsManager.GetInstance();
        settingsManager.Initialize(menuSheet, pericles18);
        SettingsManager.GameInput = gameInput;

        phonePositionManager = PhonePositionManager.GetInstance();
        PhonePositionManager.Font = pericles18;
        PhonePositionManager.Texture = menuSheet;
        PhonePositionManager.GameInput = gameInput;

        setupInputs();
    }

    private void setupInputs()
    {
        gameInput.AddTouchGestureInput(TitleAction, GestureType.Tap, new Rectangle(0, 0, WIDTH, HEIGHT));
        gameInput.AddTouchGestureInput(BackToGameAction, GestureType.Tap, continueDestination);
        gameInput.AddTouchGestureInput(BackToMainAction, GestureType.Tap, cancelDestination);
        mainMenuManager.SetupInputs();
        playerManager.SetupInputs();
        submissionManager.SetupInputs();
        settingsManager.SetupInputs();
        phonePositionManager.SetupInputs();
    }

    /// <summary>
    /// Pauses the game when a call is incoming.
    /// Attention: Also called for GUID !!!
    /// </summary>
    protected override void OnDeactivated(object sender, EventArgs args)
    {
        base.OnDeactivated(sender, args);

        if (gameState == GameStates.Playing
            || gameState == GameStates.PlayerDead
            || gameState == GameStates.Instructions
            || gameState == GameStates.BossDuell)
        {
            stateBeforePaused = gameState;
            gameState = GameStates.Paused;
        }
    }

    protected override void OnActivated(object sender, EventArgs args)
    {
        base.OnActivated(sender, args);

        // Ensure we stay fullscreen, even after app resume
        // this.graphics.IsFullScreen = false;

        // Somehow fullscreen mode seems to work fine on Android when we set it here in OnActivated?!
        // And does not skrew up the screen aspect ratio / resolution
        this.graphics.IsFullScreen = true;
    }

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        TouchPanel.DisplayHeight = HEIGHT;
        TouchPanel.DisplayWidth = WIDTH;

        SoundManager.Update(gameTime);

        gameInput.BeginUpdate();

        backButtonTimer += elapsed;

        if (backButtonTimer >= backButtonDelayTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                backButtonPressed = true;
                backButtonTimer = 0.0f;
            }
        }

        switch (gameState)
        {
            case GameStates.TitleScreen:

                titleScreenTimer += elapsed;

                updateBackground(gameTime);

                if (titleScreenTimer >= titleScreenDelayTime)
                {
                    if (gameInput.IsPressed(TitleAction))
                    {
                        gameState = GameStates.MainMenu;
                    }
                }

                if (backButtonPressed)
                    this.Exit();

                break;

            case GameStates.MainMenu:

                updateBackground(gameTime);

                mainMenuManager.IsActive = true;
                mainMenuManager.Update(gameTime);

                switch (mainMenuManager.LastPressedMenuItem)
                {
                    case MainMenuManager.MenuItems.Start:
                        gameState = GameStates.PhonePosition;
                        break;

                    case MainMenuManager.MenuItems.Highscores:
                        showLeaderboards();
                        break;

                    case MainMenuManager.MenuItems.Instructions:
                        resetGame();
                        instructionManager.Reset();
                        updateHud();
                        gameState = GameStates.Instructions;
                        break;

                    case MainMenuManager.MenuItems.Settings:
                        gameState = GameStates.Settings;
                        break;

                    case MainMenuManager.MenuItems.None:
                        // do nothing
                        break;
                }

                if (gameState != GameStates.MainMenu)
                    mainMenuManager.IsActive = false;

                if (backButtonPressed)
                    this.Exit();

                break;

            case GameStates.PhonePosition:

                updateBackground(gameTime);

                EffectManager.Update(gameTime);

                phonePositionManager.IsActive = true;
                phonePositionManager.Update(gameTime);

                if (phonePositionManager.CancelClicked || backButtonPressed)
                {
                    phonePositionManager.IsActive = false;
                    gameState = GameStates.MainMenu;
                }
                else if (phonePositionManager.StartClicked)
                {
                    phonePositionManager.IsActive = false;
                    resetGame();
                    updateHud();
                    if (instructionManager.HasDoneInstructions)
                    {
                        gameState = GameStates.Playing;
                    }
                    else
                    {
                        instructionManager.Reset();
                        instructionManager.IsAutostarted = true;
                        gameState = GameStates.Instructions;
                    }
                }

                break;

            case GameStates.Submittion:

                updateBackground(gameTime);

                if (!submissionManager.IsActive)
                {
                    // we just got into the submission state
                    submitLeaderboardScore(playerManager.PlayerScore);
                }

                submissionManager.IsActive = true;
                submissionManager.Update(gameTime);

                if (submissionManager.CancelClicked || backButtonPressed)
                {
                    submissionManager.IsActive = false;
                    gameState = GameStates.MainMenu;
                }
                else if (submissionManager.RetryClicked)
                {
                    submissionManager.IsActive = false;
                    resetGame();
                    updateHud();
                    gameState = GameStates.Playing;
                }

                break;

            case GameStates.Instructions:

                starFieldManager1.Update(gameTime);
                starFieldManager2.Update(gameTime);
                starFieldManager3.Update(gameTime);

                instructionManager.Update(gameTime);
                collisionManager.Update();
                EffectManager.Update(gameTime);
                updateHud();

                if (backButtonPressed)
                {
                    if (!instructionManager.HasDoneInstructions && instructionManager.EnougthInstructionsDone)
                    {
                        instructionManager.InstructionsDone();
                        instructionManager.SaveHasDoneInstructions();
                    }

                    EffectManager.Reset();
                    if (instructionManager.IsAutostarted)
                    {
                        resetGame();
                        updateHud();
                        gameState = GameStates.Playing;
                    }
                    else
                    {
                        gameState = GameStates.MainMenu;
                    }
                }

                break;

            case GameStates.Settings:

                updateBackground(gameTime);

                settingsManager.IsActive = true;
                settingsManager.Update(gameTime, isPrivacyRequiredSupplier(), showPrivacyConsentCallback);

                if (settingsManager.CancelClicked || backButtonPressed)
                {
                    settingsManager.IsActive = false;
                    gameState = GameStates.MainMenu;
                }

                break;

            case GameStates.Playing:

                updateBackground(gameTime);

                levelManager.Update(gameTime);

                playerManager.Update(gameTime);

                enemyManager.Update(gameTime);
                enemyManager.IsActive = true;

                bossManager.Update(gameTime);

                EffectManager.Update(gameTime);

                collisionManager.Update();

                powerUpManager.Update(gameTime);

                zoomTextManager.Update();

                updateHud();

                if (levelManager.HasChanged)
                {
                    enemyManager.IsActive = false;

                    if (enemyManager.Enemies.Count == 0)
                    {
                        bossManager.SpawnRandomBoss();
                        gameState = GameStates.BossDuell;
                    }
                }

                if (playerManager.IsDestroyed)
                {
                    playerDeathTimer = 0.0f;
                    enemyManager.IsActive = false;
                    bossManager.IsActive = false;
                    playerManager.LivesRemaining--;

                    if (playerManager.LivesRemaining < 0)
                    {
                        levelManager.ResetLevelTimer();
                        gameState = GameStates.GameOver;
                        zoomTextManager.ShowText(GameOverText);
                    }
                    else
                    {
                        levelManager.ResetLevelTimer();
                        gameState = GameStates.PlayerDead;
                    }
                }

                if (backButtonPressed)
                {
                    stateBeforePaused = GameStates.Playing;
                    gameState = GameStates.Paused;
                }

                break;

            case GameStates.BossDuell:

                updateBackground(gameTime);

                levelManager.Update(gameTime);

                playerManager.Update(gameTime);

                enemyManager.Update(gameTime);

                bossManager.Update(gameTime);

                EffectManager.Update(gameTime);

                collisionManager.Update();

                powerUpManager.Update(gameTime);

                zoomTextManager.Update();

                updateHud();

                if (bossManager.Bosses.Count == 0 && levelManager.HasChanged)
                {
                    if (bossManager.BossWasKilled)
                    {
                        bossManager.BossWasKilled = false;

                        if (bossDirectKill)
                        {
                            zoomTextManager.ShowText("+" + bossBonusScore + " Bonus");
                            playerManager.IncreasePlayerScore(bossBonusScore, false);
                            bossBonusScore += InitialBossBonusScore;
                        }
                        else
                        {
                            bossBonusScore = InitialBossBonusScore;
                        }
                        levelManager.GoToNextLevel();
                        zoomTextManager.ShowText("Level " + levelManager.CurrentLevel);

                        bossDirectKill = true;
                    }
                    else
                    {
                        bossDirectKill = false;

                        levelManager.ResetLevelTimer();
                    }

                    gameState = GameStates.Playing;
                }

                if (playerManager.IsDestroyed)
                {
                    playerDeathTimer = 0.0f;
                    enemyManager.IsActive = false;
                    bossManager.IsActive = false;
                    playerManager.LivesRemaining--;

                    if (playerManager.LivesRemaining < 0)
                    {
                        gameState = GameStates.GameOver;
                        zoomTextManager.ShowText(GameOverText);
                    }
                    else
                    {
                        gameState = GameStates.PlayerDead;
                        bossDirectKill = false;
                    }

                    levelManager.ResetLevelTimer();
                }

                if (backButtonPressed)
                {
                    stateBeforePaused = GameStates.BossDuell;
                    gameState = GameStates.Paused;
                }

                break;

            case GameStates.Paused:

                if (gameInput.IsPressed(BackToGameAction) || backButtonPressed)
                {
                    gameState = stateBeforePaused;
                }

                if (gameInput.IsPressed(BackToMainAction))
                {
                    if (playerManager.PlayerScore > 0)
                    {
                        gameState = GameStates.Submittion;
                        submissionManager.SetUp(playerManager.PlayerScore, levelManager.CurrentLevel);
                    }
                    else
                    {
                        gameState = GameStates.MainMenu;
                    }
                }

                break;

            case GameStates.PlayerDead:

                playerDeathTimer += elapsed;

                updateBackground(gameTime);
                asteroidManager.Update(gameTime);
                asteroidManager.IsActive = false;
                starFieldManager1.SpeedFactor = 3.0f;
                starFieldManager2.SpeedFactor = 3.0f;
                starFieldManager3.SpeedFactor = 3.0f;

                playerManager.PlayerShotManager.Update(gameTime);
                playerManager.PlayerShotManager.Update(gameTime);

                powerUpManager.Update(gameTime);

                enemyManager.Update(gameTime);
                enemyManager.Update(gameTime);
                bossManager.Update(gameTime);
                bossManager.Update(gameTime);
                EffectManager.Update(gameTime);
                EffectManager.Update(gameTime);
                collisionManager.Update();
                zoomTextManager.Update();
                updateHud();

                if (playerDeathTimer >= playerDeathDelayTime)
                {
                    starFieldManager1.SpeedFactor = 1.0f;
                    starFieldManager2.SpeedFactor = 1.0f;
                    starFieldManager3.SpeedFactor = 1.0f;
                    asteroidManager.IsActive = true;
                    resetRound();
                    gameState = GameStates.Playing;
                }

                if (backButtonPressed)
                {
                    starFieldManager1.SpeedFactor = 1.0f;
                    starFieldManager2.SpeedFactor = 1.0f;
                    starFieldManager3.SpeedFactor = 1.0f;
                    asteroidManager.IsActive = true;
                    stateBeforePaused = GameStates.PlayerDead;
                    gameState = GameStates.Paused;
                }

                break;

            case GameStates.GameOver:

                playerDeathTimer += elapsed;

                updateBackground(gameTime);

                playerManager.PlayerShotManager.Update(gameTime);
                powerUpManager.Update(gameTime);
                enemyManager.Update(gameTime);
                bossManager.Update(gameTime);
                EffectManager.Update(gameTime);
                collisionManager.Update();
                zoomTextManager.Update();
                updateHud();

                if (playerDeathTimer >= playerGameOverDelayTime)
                {
                    playerDeathTimer = 0.0f;
                    titleScreenTimer = 0.0f;

                    if (playerManager.PlayerScore > 0)
                    {
                        gameState = GameStates.Submittion;
                        submissionManager.SetUp(playerManager.PlayerScore, levelManager.CurrentLevel);
                    }
                    else
                    {
                        gameState = GameStates.MainMenu;
                    }

                    gameOverEndedCallback();
                }

                if (backButtonPressed)
                {
                    stateBeforePaused = GameStates.GameOver;
                    gameState = GameStates.Paused;
                }

                break;
        }

        // Reset Back-Button flag
        backButtonPressed = false;

        gameInput.EndUpdate();

        base.Update(gameTime);
    }

    private readonly Color VERY_DARK_GRAY = new Color(10, 10, 10);

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin(transformMatrix: screenScaleMatrix);

        spriteBatch.Draw(spriteSheet,
            new Rectangle(0, 0, WIDTH, HEIGHT),
            new Rectangle(0, 360, 16, 16),
            VERY_DARK_GRAY);

        if (gameState == GameStates.TitleScreen)
        {
            drawBackground(spriteBatch);

            spriteBatch.Draw(menuSheet,
                             new Vector2(150.0f, 150.0f),
                             new Rectangle(0, 0,
                                           500,
                                           100),
                             Color.White);


            spriteBatch.DrawString(pericles18,
                                   ContinueText,
                                   new Vector2(WIDTH / 2 - pericles18.MeasureString(ContinueText).X / 2,
                                               275),
                                   Color.Red * (0.25f + (float)(Math.Pow(Math.Sin(gameTime.TotalGameTime.TotalSeconds), 2.0f)) * 0.75f));

            spriteBatch.DrawString(pericles16,
                                   MusicByText,
                                   new Vector2(WIDTH / 2 - pericles18.MeasureString(MusicByText).X / 2,
                                               410),
                                   Color.Red);
            spriteBatch.DrawString(pericles16,
                                   MusicCreatorText,
                                   new Vector2(WIDTH / 2 - pericles18.MeasureString(MusicCreatorText).X / 2,
                                               435),
                                   Color.Red);

            spriteBatch.DrawString(pericles16,
                                   VersionText,
                                   new Vector2(WIDTH - (pericles16.MeasureString(VersionText).X + 15),
                                               HEIGHT - (pericles16.MeasureString(VersionText).Y + 10)),
                                   Color.Red);

            spriteBatch.DrawString(pericles16,
                                   CreatorText,
                                   new Vector2(15,
                                               HEIGHT - (pericles16.MeasureString(CreatorText).Y + 10)),
                                   Color.Red);
        }

        if (gameState == GameStates.MainMenu)
        {
            drawBackground(spriteBatch);

            mainMenuManager.Draw(spriteBatch);
        }

        if (gameState == GameStates.PhonePosition)
        {
            drawBackground(spriteBatch);

            phonePositionManager.Draw(spriteBatch);
        }

        if (gameState == GameStates.Submittion)
        {
            drawBackground(spriteBatch);

            submissionManager.Draw(spriteBatch);
        }

        if (gameState == GameStates.Instructions)
        {
            starFieldManager1.Draw(spriteBatch);
            starFieldManager2.Draw(spriteBatch);
            starFieldManager3.Draw(spriteBatch);

            instructionManager.Draw(spriteBatch);

            EffectManager.Draw(spriteBatch);

            hud.Draw(spriteBatch);
        }

        if (gameState == GameStates.Settings)
        {
            drawBackground(spriteBatch);

            settingsManager.Draw(spriteBatch);
        }

        if (gameState == GameStates.Paused)
        {
            drawBackground(spriteBatch);

            powerUpManager.Draw(spriteBatch);

            playerManager.Draw(spriteBatch);

            enemyManager.Draw(spriteBatch);

            bossManager.Draw(spriteBatch);

            EffectManager.Draw(spriteBatch);

            // Pause title

            spriteBatch.Draw(spriteSheet,
                             new Rectangle(0, 0, WIDTH, HEIGHT),
                             new Rectangle(0, 350, 1, 1),
                             new Color(0, 0, 0, 150));

            spriteBatch.Draw(menuSheet,
                             new Vector2(150.0f, 150.0f),
                             new Rectangle(0, 100,
                                           500,
                                           100),
                             Color.White * (0.25f + (float)(Math.Pow(Math.Sin(gameTime.TotalGameTime.TotalSeconds), 2.0f)) * 0.75f));

            spriteBatch.Draw(menuSheet,
                             cancelDestination,
                             cancelSource,
                             Color.Red);

            spriteBatch.Draw(menuSheet,
                             continueDestination,
                             continueSource,
                             Color.Red);
        }

        if (gameState == GameStates.Playing ||
            gameState == GameStates.PlayerDead ||
            gameState == GameStates.GameOver ||
            gameState == GameStates.BossDuell)
        {
            drawBackground(spriteBatch);

            powerUpManager.Draw(spriteBatch);

            playerManager.Draw(spriteBatch);

            enemyManager.Draw(spriteBatch);

            bossManager.Draw(spriteBatch);

            EffectManager.Draw(spriteBatch);

            zoomTextManager.Draw(spriteBatch);

            hud.Draw(spriteBatch);
        }

        spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>
    /// Helper method to reduce update redundace.
    /// </summary>
    private void updateBackground(GameTime gameTime)
    {
        starFieldManager1.Update(gameTime);
        starFieldManager2.Update(gameTime);
        starFieldManager3.Update(gameTime);

        asteroidManager.Update(gameTime);
    }

    /// <summary>
    /// Helper method to reduce draw redundace.
    /// </summary>
    private void drawBackground(SpriteBatch spriteBatch)
    {
        starFieldManager1.Draw(spriteBatch);
        starFieldManager2.Draw(spriteBatch);
        starFieldManager3.Draw(spriteBatch);

        asteroidManager.Draw(spriteBatch);
    }

    private void resetRound()
    {
        asteroidManager.Reset();
        enemyManager.Reset();
        bossManager.Reset();
        playerManager.Reset();
        EffectManager.Reset();
        powerUpManager.Reset();

        zoomTextManager.Reset();
        zoomTextManager.ShowText(GetReadyText[rand.Next(GetReadyText.Length)]);
    }

    private void resetGame()
    {
        resetRound();

        levelManager.Reset();

        playerManager.ResetPlayerScore();
        playerManager.ResetRemainingLives();
        playerManager.ResetSpecialWeapons();

        bossDirectKill = true;
        bossBonusScore = InitialBossBonusScore;

        startNewGameCallback();

        GC.Collect();
    }

    /// <summary>
    /// Loads the current version from assembly.
    /// </summary>
    private void loadVersion()
    {
        System.Reflection.AssemblyName an = new System.Reflection.AssemblyName(System.Reflection.Assembly
                                                                               .GetExecutingAssembly()
                                                                               .FullName);
        this.VersionText = new StringBuilder().Append("v ")
                                              .Append(an.Version.Major)
                                              .Append('.')
                                              .Append(an.Version.Minor)
                                              .ToString();
    }

    private void updateHud()
    {
        hud.Update(playerManager.PlayerScore,
                           playerManager.LivesRemaining,
                           playerManager.Overheat,
                           playerManager.HitPoints,
                           playerManager.ShieldPoints,
                           playerManager.SpecialShotsRemaining,
                           playerManager.CarliRocketsRemaining,
                           playerManager.ScoreMulti,
                           levelManager.CurrentLevel);
    }

    public void BackButtonPressed()
    {
        backButtonPressed = true;
    }
}

