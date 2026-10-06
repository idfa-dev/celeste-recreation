using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Celeste
{
    internal class Level
    {
        public List<List<Tile>> TileMap = new();
        private readonly List<Texture2D> TileSetList = new();
        private RenderTarget2D LevelRender;

        public Vector2 Position;
        private readonly int LayerNum;
        private readonly int XTileNum;
        private readonly int YTileNum;
        public char LevelExitBorder;
        private readonly float scale;

        public readonly static int TileDimension = 8;
        public float Width;
        public float Height;

        public Level(List<Texture2D> tilesets, List<int[]> tileArrayList, int xTileNum, int yTileNum, Vector2 origin, char levelExitBorder, float scale)
        {
            XTileNum = xTileNum;
            YTileNum = yTileNum;
            this.scale = scale;
            LevelExitBorder = levelExitBorder;
            Position = origin * scale * TileDimension;
            Width = XTileNum * TileDimension * scale;
            Height = YTileNum * TileDimension * scale;
            LayerNum = tileArrayList.Count;
            TileSetList.AddRange(tilesets);
            TileMap = LoadTileMap(tileArrayList);
        }

        private List<List<Tile>> LoadTileMap(List<int[]> tileList)
        {
            var tempTileMap = LoadEmptyTileMap();
            LevelRender = new RenderTarget2D(Engine.spriteBatch.GraphicsDevice, (int)Width, (int)Height);
            Engine.spriteBatch.GraphicsDevice.SetRenderTarget(LevelRender);
            Engine.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

            Engine.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null);

            for (int layerCount = 0; layerCount < LayerNum; layerCount++)
            {
                for (int tileCount = 0; tileCount < tileList[layerCount].Length; tileCount++)
                {
                    int xTileCount = tileCount % XTileNum;
                    int yTileCount = (int)Math.Floor((decimal)tileCount / XTileNum);

                    if (tempTileMap[yTileCount][xTileCount].TileID < 0 || layerCount == 0)
                    {
                        int tempTileID = tileList[layerCount][tileCount];
                        var tempPosition = new Vector2(
                            Position.X + (xTileCount * TileDimension * scale),
                            Position.Y + (yTileCount * TileDimension * scale));

                        var tempTileRectangle = GetTilesetRectangle(tempTileID);
                        tempTileMap[yTileCount][xTileCount] = new Tile(tempPosition, tempTileRectangle, tempTileID, layerCount);
                        Engine.spriteBatch.Draw(
                            TileSetList[layerCount],
                            tempPosition - Position,
                            tempTileRectangle,
                            tempTileID == -1 ? Color.Transparent : Color.White,
                            0f,
                            Vector2.Zero,
                            scale,
                            SpriteEffects.None,
                            0);
                    }
                }
            }

            Engine.spriteBatch.End();
            Engine.spriteBatch.GraphicsDevice.SetRenderTarget(null);
            return tempTileMap;
        }

        private List<List<Tile>> LoadEmptyTileMap()
        {
            var tempTileMap = new List<List<Tile>>();
            var emptyTile = new Tile();

            for (int i = 0; i < YTileNum; i++)
            {
                var emptyTileList = new List<Tile>();
                for (int j = 0; j < XTileNum; j++)
                {
                    emptyTileList.Add(emptyTile);
                }

                tempTileMap.Add(emptyTileList);
            }

            return tempTileMap;
        }

        private Rectangle GetTilesetRectangle(int tileID)
        {
            const int tileSheetWidth = 6;
            int rectanglePositionX = (tileID % tileSheetWidth) * TileDimension;
            int rectanglePositionY = (int)Math.Floor((float)tileID / tileSheetWidth) * TileDimension;
            return new Rectangle(rectanglePositionX, rectanglePositionY, TileDimension, TileDimension);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(LevelRender, Position, Color.White);
        }

        public void OldDraw(SpriteBatch spriteBatch)
        {
            int tileSetID = -1;
            int tileID;
            Texture2D tileSet;

            for (int y = 0; y < TileMap.Count; y++)
            {
                for (int x = 0; x < TileMap[y].Count; x++)
                {
                    tileSetID = TileMap[y][x].TilesetID;
                    tileSet = TileSetList[tileSetID];
                    tileID = TileMap[y][x].TileID;

                    if (tileID != -1)
                    {
                        spriteBatch.Draw(tileSet, TileMap[y][x].Position, TileMap[y][x].TileRectangle, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                }
            }
        }
    }
}
