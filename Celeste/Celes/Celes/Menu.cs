using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Celeste
{
    internal class Menu
    {
        protected static GraphicsDevice graphicsDevice;
        public static SpriteFont TextFont;
        private static Vector2 Position;
        private readonly Texture2D Mask;

        public Button TopButton;
        private Button MiddleButton;
        private Button ExitButton;

        public Menu(GraphicsDevice graphicsDevice, SpriteFont textFont)
        {
            Menu.graphicsDevice = graphicsDevice;
            TextFont = textFont;
            Position = new Vector2(graphicsDevice.Viewport.X, graphicsDevice.Viewport.Y);
            Mask = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Mask.SetData(new[] { Color.Black });
            CreateButtons();
        }

        private void CreateButtons()
        {
            float buttonScale = 1f;
            Vector2 shiftedOrigin = Position + new Vector2(100, 100);
            Vector2 buttonShift = new Vector2(0, 150) * buttonScale;

            TopButton = new Button("Play", shiftedOrigin, buttonScale);
            MiddleButton = new Button("Options", shiftedOrigin + buttonShift, buttonScale);
            ExitButton = new Button("Exit Game", shiftedOrigin + 5f * buttonShift, buttonScale);
        }

        public void CheckIfButtonsPressed()
        {
            if (TopButton.IsHovering)
            {
                Engine.GameState = Engine.GameStates.Playing;
            }
            else if (MiddleButton.IsHovering)
            {
                Engine.OptionsMenu.prevGameState = Engine.GameState;
                Engine.GameState = Engine.GameStates.Options;
            }
            else if (ExitButton.IsHovering)
            {
                Environment.Exit(1);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Mask, new Rectangle(graphicsDevice.Viewport.X, graphicsDevice.Viewport.Y, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White * 0.85f);
            TopButton.Draw(spriteBatch);
            MiddleButton.Draw(spriteBatch);
            ExitButton.Draw(spriteBatch);
        }
    }
}
