using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Celeste
{
    internal class Button
    {
        public string Text;
        public Vector2 Position;
        private float ButtonWidth = 600;
        private float ButtonHeight = 120;
        private float scale;

        public bool IsHovering
        {
            get
            {
                var mousePos = Mouse.GetState().Position;
                return mousePos.X > Position.X - 50
                    && mousePos.X < Position.X + ButtonWidth
                    && mousePos.Y > Position.Y - 25 * scale
                    && mousePos.Y < Position.Y + ButtonHeight;
            }
        }

        public Color Color => IsHovering ? Color.White : Color.Gray;

        public Button(string text, Vector2 position, float scale)
        {
            Text = text;
            Position = position;
            this.scale = scale;
            ButtonWidth *= scale;
            ButtonHeight *= scale;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Menu.TextFont, Text, Position, Color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
