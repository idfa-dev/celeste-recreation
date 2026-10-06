using Microsoft.Xna.Framework;

namespace Celeste
{
    internal class _Hitbox
    {
        public float Width = 11;
        public float Height = 13;
        public Vector2 Center;

        #region Verticies

        public Vector2 TopLeft => new Vector2(Center.X - Width / 2, Center.Y - Height / 2);
        public Vector2 Top => new Vector2(Center.X, Center.Y - Height / 2);
        public Vector2 TopRight => new Vector2(Center.X + Width / 2, Center.Y - Height / 2);
        public Vector2 Left => new Vector2(Center.X - Width / 2, Center.Y);
        public Vector2 Right => new Vector2(Center.X + Width / 2, Center.Y);
        public Vector2 BottomLeft => new Vector2(Center.X - Width / 2, Center.Y + (Height / 2));
        public Vector2 Bottom => new Vector2(Center.X, Center.Y + (Height / 2));
        public Vector2 BottomRight => new Vector2(Center.X + Width / 2, Center.Y + (Height / 2));

        #endregion

        #region Level Border Checks

        public bool IsOutOfRightBorder
        {
            get
            {
                if (Engine.Map == null || Engine.Map.currentLevel == null)
                {
                    return false;
                }

                return Left.X - Engine.Map.currentLevel.Position.X > Engine.Map.currentLevel.Width;
            }
        }

        public bool IsOutOfLeftBorder
        {
            get
            {
                if (Engine.Map == null || Engine.Map.currentLevel == null)
                {
                    return false;
                }

                return Right.X < Engine.Map.currentLevel.Position.X;
            }
        }

        public bool IsOutOfTopBorder
        {
            get
            {
                if (Engine.Map == null || Engine.Map.currentLevel == null)
                {
                    return false;
                }

                return Bottom.Y < Engine.Map.currentLevel.Position.Y;
            }
        }

        public bool IsOutOfBottomBorder
        {
            get
            {
                if (Engine.Map == null || Engine.Map.currentLevel == null)
                {
                    return false;
                }

                return Top.Y - Engine.Map.currentLevel.Position.Y > Engine.Map.currentLevel.Height;
            }
        }

        #endregion

        #region Tile Collision Checks

        public bool IsCollidingRight = false;
        public bool IsCollidingLeft = false;

        #endregion

        public _Hitbox(Vector2 position, float scale)
        {
            Center = position;
            Width *= scale;
            Height *= scale;
        }

        public void ResetCollisionVars()
        {
            IsCollidingRight = false;
            IsCollidingLeft = false;
        }
    }
}
