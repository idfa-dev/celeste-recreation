using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste
{
    internal class Map
    {
        private readonly ContentManager Content;
        public Level prevLevel;
        public Level currentLevel;
        public Level nextLevel;
        public int LevelNum;
        private readonly int NumberOfLevels;
        private readonly float scale;

        public Map(ContentManager content, float scale)
        {
            Content = content;
            this.scale = scale;

            if (File.Exists("Leveldata.txt"))
            {
                NumberOfLevels = File.ReadAllLines("Leveldata.txt").Length;
                if (NumberOfLevels > 0)
                {
                    LevelNum = 0;
                    LoadLevel(LevelNum);
                    return;
                }
            }

            NumberOfLevels = 0;
            CreateDefaultLevel();
        }

        public void Next()
        {
            if (nextLevel != null)
            {
                prevLevel = currentLevel;
                currentLevel = nextLevel;
            }

            if (LevelNum + 1 < NumberOfLevels - 1)
            {
                LevelNum++;
                var levelData = File.ReadAllLines("Leveldata.txt")[LevelNum + 1].Split('|');
                LoadFromFile(levelData);
            }
            else
            {
                nextLevel = null;
            }
        }

        public void Prev()
        {
            if (prevLevel != null)
            {
                nextLevel = currentLevel;
                currentLevel = prevLevel;
            }

            if (LevelNum > 0)
            {
                LevelNum--;
                var levelData = File.ReadAllLines("Leveldata.txt")[LevelNum - 1].Split('|');
                LoadFromFile(levelData);
            }
            else
            {
                prevLevel = null;
            }
        }

        private void LoadLevel(int levelIndex)
        {
            if (!File.Exists("Leveldata.txt"))
            {
                CreateDefaultLevel();
                return;
            }

            var lines = File.ReadAllLines("Leveldata.txt");
            if (lines.Length == 0)
            {
                CreateDefaultLevel();
                return;
            }

            var levelData = lines[levelIndex].Split('|');
            LoadFromFile(levelData);
        }

        private void LoadFromFile(string[] levelData)
        {
            var origin = new Vector2(float.Parse(levelData[0]), float.Parse(levelData[1]));
            int levelWidth = int.Parse(levelData[2]);
            int levelHeight = int.Parse(levelData[3]);
            char levelExitBorder = char.Parse(levelData[4]);

            var tileSets = new List<Texture2D>();
            var tileArrayLists = new List<int[]>();

            for (int i = 5; i < levelData.Length; i += 2)
            {
                tileSets.Add(Content.Load<Texture2D>(levelData[i]));
                string[] stringTileList = levelData[i + 1].Split(',');
                int[] intTileList = new int[stringTileList.Length];

                for (int j = 0; j < stringTileList.Length; j++)
                {
                    intTileList[j] = int.Parse(stringTileList[j]);
                }

                tileArrayLists.Add(intTileList);
            }

            currentLevel = new Level(tileSets, tileArrayLists, levelWidth, levelHeight, origin, levelExitBorder, scale);
        }

        private void CreateDefaultLevel()
        {
            var tileSet = new Texture2D(Engine.spriteBatch.GraphicsDevice, 1, 1);
            tileSet.SetData(new[] { Color.White });
            var tileSets = new List<Texture2D> { tileSet };
            var tileArray = new[] { new[] { -1, -1, -1, -1, -1, -1, -1, -1 } };
            var tileArrayLists = new List<int[]> { tileArray[0] };
            currentLevel = new Level(tileSets, tileArrayLists, 8, 1, Vector2.Zero, 'L', scale);
        }
    }
}
