using Microsoft.Xna.Framework;

namespace Celeste
{
    internal struct Tile
    {
        public Vector2 Position;
        public Rectangle TileRectangle;
        public int TileID;
        public int TilesetID;

        public Tile(Vector2 position, Rectangle tileRectangle, int tileID, int tilesetID)
        {
            Position = position;
            TileRectangle = tileRectangle;
            TileID = tileID;
            TilesetID = tilesetID;
        }
    }
}
