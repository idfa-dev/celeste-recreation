using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Celeste
{
    public class Engine : Game
    {
        public static float scale = 6;
        private readonly GraphicsDeviceManager _graphics;
        public static SpriteBatch spriteBatch;
        public static SpriteBatch spriteBatch2;
        public bool IsBeingPressed;

        public static Player Madeline;
        internal static Map Map;
        private Menu Menu;
        internal static OptionsMenu OptionsMenu;

        public static GameStates GameState = GameStates.Unbegun;

        public enum GameStates
        {
            Playing = 0,
            Unbegun = 1,
            Paused = 2,
            Options = 3
        }

        public Engine()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Map = new Map(Content, scale);

            var startPoint = new Vector2(300, 300) + Map.currentLevel.Position;
            Madeline = new Player(Content, startPoint, Vector2.One, scale);

            var font = Content.Load<SpriteFont>("Renogare");
            Menu = new Menu(GraphicsDevice, font);
            OptionsMenu = new OptionsMenu(GraphicsDevice, font, _graphics);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch2 = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                if (Menu != null)
                {
                    Menu.TopButton.Text = "Resume";
                }
                GameState = GameStates.Paused;
            }

            if (GameState != GameStates.Playing)
            {
                var mouseState = Mouse.GetState();
                var keyboardState = Keyboard.GetState();
                if ((mouseState.LeftButton == ButtonState.Pressed && !IsBeingPressed) || keyboardState.GetPressedKeyCount() > 0)
                {
                    if (GameState == GameStates.Options)
                    {
                        OptionsMenu.CheckIfButtonIsPressed();
                    }
                    else if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        Menu.CheckIfButtonsPressed();
                    }

                    IsBeingPressed = true;
                }
                else if (mouseState.LeftButton != ButtonState.Pressed)
                {
                    IsBeingPressed = false;
                }
            }

            if (GameState == GameStates.Playing)
            {
                Madeline.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var transformationMatrix = Madeline.ReturnTransformationMatrix(
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                Map.currentLevel.Width,
                Map.currentLevel.Height);

            spriteBatch.Begin(
                SpriteSortMode.BackToFront,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp,
                null,
                null,
                null,
                transformMatrix: transformationMatrix);

            if (GameState == GameStates.Playing || GameState == GameStates.Paused)
            {
                Madeline.Draw(spriteBatch);
                Map.currentLevel.Draw(spriteBatch);
            }

            spriteBatch.End();

            if (GameState != GameStates.Playing)
            {
                spriteBatch2.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null,
                    null,
                    null,
                    null);

                if (GameState == GameStates.Options)
                {
                    OptionsMenu.Draw(spriteBatch2);
                }
                else
                {
                    Menu.Draw(spriteBatch2);
                }

                spriteBatch2.End();
            }

            base.Draw(gameTime);
        }
    }
}
