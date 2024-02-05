using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using Spacepixx.Inputs;
using Spacepixx.Extensions;

namespace Spacepixx
{
    class SubmissionManager
    {
        #region Members

        private readonly Rectangle cancelSource = new Rectangle(0, 750,
                                                                300, 50);
        private readonly Rectangle cancelDestination = new Rectangle(450, 370,
                                                                     300, 50);

        private readonly Rectangle retrySource = new Rectangle(0, 1100,
                                                                  300, 50);
        private readonly Rectangle retryDestination = new Rectangle(50, 370,
                                                                    300, 50);

        private static SubmissionManager submissionManager;

        public const int MaxScores = 10;

        public static Texture2D Texture;
        public static SpriteFont Font;
        private readonly Rectangle TitleSource = new Rectangle(0, 600,
                                                               500, 100);
        private readonly Vector2 TitlePosition = new Vector2(150.0f, 80.0f);

        private float opacity = 0.0f;
        private const float OpacityMax = 1.0f;
        private const float OpacityMin = 0.0f;
        private const float OpacityChangeRate = 0.05f;

        private bool isActive = false;
        private long score;
        private int level;

        private bool cancelClicked = false;
        private bool retryClicked = false;


        private const string TEXT_SCORE = "Score:";
        private const string TEXT_LEVEL = "Level:";

        public static GameInput GameInput;

        private const string CancelAction = "Cancel";
        private const string RetryAction = "Retry";

        #endregion

        #region Constructors

        private SubmissionManager()
        {
        }

        #endregion

        #region Methods

        public void SetupInputs()
        {
            GameInput.AddTouchGestureInput(CancelAction,
                                           GestureType.Tap,
                                           cancelDestination);
            GameInput.AddTouchGestureInput(RetryAction,
                                           GestureType.Tap,
                                           retryDestination);
        }

        public static SubmissionManager GetInstance()
        {
            if (submissionManager == null)
            {
                submissionManager = new SubmissionManager();
            }

            return submissionManager;
        }

        private void handleTouchInputs()
        {
            if (GameInput.IsPressed(RetryAction))
            {
                retryClicked = true;
            }
            else if (GameInput.IsPressed(CancelAction))
            {
                cancelClicked = true;
            }
        }

        public void SetUp(long score, int level)
        {
            this.score = score;
            this.level = level;
        }

        public void Update(GameTime gameTime)
        {
            if (isActive)
            {
                if (this.opacity < OpacityMax)
                    this.opacity += OpacityChangeRate;
            }

            handleTouchInputs();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture,
                                 cancelDestination,
                                 cancelSource,
                                 Color.Red * opacity);

            spriteBatch.DrawString(Font,
                                   TEXT_SCORE,
                                   new Vector2(300,
                                               270),
                                   Color.Red * opacity);

            spriteBatch.DrawString(Font,
                                   TEXT_LEVEL,
                                   new Vector2(300,
                                               310),
                                   Color.Red * opacity);

            spriteBatch.DrawInt64(Font,
                                  score,
                                  new Vector2(450,
                                              270),
                                  Color.Red * opacity);

            spriteBatch.DrawInt64(Font,
                                  level,
                                  new Vector2(450,
                                              310),
                                  Color.Red * opacity);

            spriteBatch.Draw(Texture,
                             TitlePosition,
                             TitleSource,
                             Color.White * opacity);
        }

        #endregion

        #region Activate/Deactivate

        public void Activated(StreamReader reader)
        {
            this.opacity = Single.Parse(reader.ReadLine());
            this.isActive = Boolean.Parse(reader.ReadLine());
            this.score = Int64.Parse(reader.ReadLine());
            this.level = Int32.Parse(reader.ReadLine());
        }

        public void Deactivated(StreamWriter writer)
        {
            writer.WriteLine(opacity);
            writer.WriteLine(isActive);
            writer.WriteLine(score);
            writer.WriteLine(level);
        }

        #endregion

        #region Properties

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
                    this.retryClicked = false;
                    this.cancelClicked = false;
                }
            }
        }

        public bool CancelClicked
        {
            get
            {
                return this.cancelClicked;
            }
        }

        public bool RetryClicked
        {
            get
            {
                return this.retryClicked;
            }
        }

        #endregion
    }
}
