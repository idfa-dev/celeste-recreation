using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace Celeste
{
    internal class OptionsMenu
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly GraphicsDeviceManager graphicsManager;
        private static Vector2 Position;
        public Engine.GameStates prevGameState;
        private readonly Texture2D Mask;
        public bool WaitingForInput = false;

        public Keys[] KeyBinds;
        private int TargetKeyIndex;

        private Button FullScreenButton;
        private Button ChangeUpMoveKey;
        private Button ChangeLeftMoveKey;
        private Button ChangeDownMoveKey;
        private Button ChangeRightMoveKey;
        private Button ChangeJumpKey;
        private Button ChangeDashKey;
        private Button ChangeClimbKey;
        private Button ReturnToPrevMenu;

        public OptionsMenu(GraphicsDevice graphicsDevice, SpriteFont textFont, GraphicsDeviceManager graphicsManager)
        {
            this.graphicsDevice = graphicsDevice;
            this.graphicsManager = graphicsManager;
            Position = new Vector2(graphicsDevice.Viewport.X, graphicsDevice.Viewport.Y);
            Mask = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Mask.SetData(new[] { Color.Black });
            KeyBinds = GetDefaultKeybinds();
            CreateButtons();
        }

        private Keys[] GetDefaultKeybinds()
        {
            if (Engine.Madeline != null)
            {
                return new[]
                {
                    Engine.Madeline.UpMoveKey,
                    Engine.Madeline.LeftMoveKey,
                    Engine.Madeline.DownMoveKey,
                    Engine.Madeline.RightMoveKey,
                    Engine.Madeline.JumpKey,
                    Engine.Madeline.DashKey,
                    Engine.Madeline.ClimbKey,
                };
            }

            return new[]
            {
                Keys.W,
                Keys.A,
                Keys.S,
                Keys.D,
                Keys.Space,
                Keys.LeftShift,
                Keys.LeftControl,
            };
        }

        public void CreateButtons()
        {
            float buttonScale = 0.5f;
            Vector2 shiftedOrigin = Position + new Vector2(100, 100);
            Vector2 buttonShift = new Vector2(0, 150 * buttonScale);

            string fullScreenButtonText = "FullScreen: " + (graphicsManager.IsFullScreen ? "On" : "Off");
            string returnMenuText = "Return to " + (prevGameState == Engine.GameStates.Paused ? "Pause Menu" : "Main Menu");

            FullScreenButton = new Button(fullScreenButtonText, shiftedOrigin, buttonScale);
            ReturnToPrevMenu = new Button(returnMenuText, shiftedOrigin + (11f * buttonShift), buttonScale);
            ChangeUpMoveKey = new Button("UpMoveKey: " + KeyBinds[0].ToString(), shiftedOrigin + buttonShift, buttonScale);
            ChangeLeftMoveKey = new Button("LeftMoveKey: " + KeyBinds[1].ToString(), shiftedOrigin + 2 * buttonShift, buttonScale);
            ChangeDownMoveKey = new Button("DownMoveKey: " + KeyBinds[2].ToString(), shiftedOrigin + 3 * buttonShift, buttonScale);
            ChangeRightMoveKey = new Button("RightMoveKey: " + KeyBinds[3].ToString(), shiftedOrigin + 4 * buttonShift, buttonScale);
            ChangeJumpKey = new Button("JumpKey: " + KeyBinds[4].ToString(), shiftedOrigin + 5 * buttonShift, buttonScale);
            ChangeDashKey = new Button("DashKey: " + KeyBinds[5].ToString(), shiftedOrigin + 6 * buttonShift, buttonScale);
            ChangeClimbKey = new Button("ClimbKey: " + KeyBinds[6].ToString(), shiftedOrigin + 7 * buttonShift, buttonScale);
        }

        public void CheckIfButtonIsPressed()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && !WaitingForInput)
            {
                if (FullScreenButton.IsHovering)
                {
                    graphicsManager.IsFullScreen = !graphicsManager.IsFullScreen;
                    FullScreenButton.Text = graphicsManager.IsFullScreen ? "Fullscreen: On" : "Fullscreen: Off";
                    graphicsManager.ApplyChanges();
                }
                else if (ReturnToPrevMenu.IsHovering)
                {
                    Engine.GameState = prevGameState;
                    SaveChanges();
                }
                else
                {
                    if (ChangeUpMoveKey.IsHovering) { SetTargetKeyIndex(0); }
                    else if (ChangeLeftMoveKey.IsHovering) { SetTargetKeyIndex(1); }
                    else if (ChangeDownMoveKey.IsHovering) { SetTargetKeyIndex(2); }
                    else if (ChangeRightMoveKey.IsHovering) { SetTargetKeyIndex(3); }
                    else if (ChangeJumpKey.IsHovering) { SetTargetKeyIndex(4); }
                    else if (ChangeDashKey.IsHovering) { SetTargetKeyIndex(5); }
                    else if (ChangeClimbKey.IsHovering) { SetTargetKeyIndex(6); }
                }
            }
            else if (WaitingForInput)
            {
                GetInputKey();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Mask, new Rectangle(graphicsDevice.Viewport.X, graphicsDevice.Viewport.Y, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.Black * 0.97f);
            FullScreenButton.Draw(spriteBatch);
            ReturnToPrevMenu.Draw(spriteBatch);
            ChangeUpMoveKey.Draw(spriteBatch);
            ChangeLeftMoveKey.Draw(spriteBatch);
            ChangeDownMoveKey.Draw(spriteBatch);
            ChangeRightMoveKey.Draw(spriteBatch);
            ChangeJumpKey.Draw(spriteBatch);
            ChangeDashKey.Draw(spriteBatch);
            ChangeClimbKey.Draw(spriteBatch);
        }

        private void SetTargetKeyIndex(int targetNum)
        {
            WaitingForInput = true;
            TargetKeyIndex = targetNum;
        }

        private void GetInputKey()
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.GetPressedKeyCount() > 0 && !keyboardState.IsKeyDown(Keys.Escape))
            {
                Keys[] pressedKeys = keyboardState.GetPressedKeys();
                KeyBinds[TargetKeyIndex] = pressedKeys[0];
                WaitingForInput = false;
                CreateButtons();
            }
        }

        private void SaveChanges()
        {
            string[] keybindIds = new string[KeyBinds.Length];
            for (int i = 0; i < KeyBinds.Length; i++)
            {
                keybindIds[i] = Convert.ToString((int)KeyBinds[i]);
            }

            File.WriteAllLines("Keybinds.txt", keybindIds);
            if (Engine.Madeline != null)
            {
                Engine.Madeline.LoadControls();
            }
        }
    }
}
