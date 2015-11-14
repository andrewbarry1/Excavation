using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGame
{

    class RockMap
    {
        public Rock[,] map;

        public int rows;
        public int cols;
        private Texture2D blackTexture;

        public int width;
        public int height;
        private ContentManager Content;

        public RockMap(ContentManager c)
        {
            rows = 0;
            Content = c;
            cols = 0;
            width = cols * 50;
            height = (rows * 50) + 125;
            map = new Rock[0,0];
            blackTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Color[] blackTextureData = new Color[1];
            blackTextureData[0] = Color.Black;
            blackTexture.SetData(blackTextureData);
        }

        public void addRow(string newRow)
        {
            string[] rockFilenames = new string[] {"dirt.png","stone.png","iron.png","diamond","marsium"};
            Rock[,] newMap = new Rock[Math.Max(rows, newRow.Length),cols+1];
            for (int y = 0; y < cols+1; y++)
            {
                for (int x = 0; x < Math.Max(rows, newRow.Length); x++)
                {
                    try
                    {
                        newMap[x, y] = map[x, y];
                    }
                    catch { // oob, place new Rock here
                        int newRockNumber = int.Parse(newRow.Substring(x, 1));
                        Rock newRock = null;
                        if (newRockNumber != 9)
                        {
                            newRock = new Rock(rockFilenames[newRockNumber], Content, new Vector2(50 * x, 50 * y), newRockNumber, false);
                            newRock.boundingRectangle = new Rectangle(50 * x, (50 * y) + 125, 50, 50);
                        }
                        newMap[x, y] = newRock;
                    }
                }
            }
            map = newMap;
            cols++;
            rows = Math.Max(rows, newRow.Length);
        }

        public void generateMoreMap()
        {

        }

        public static Vector2 convertCoordinates(Vector2 coords)
        {
            return coords;
        }

        public void draw(SpriteBatch spriteBatch, Vector2 mapOffset)
        {
            int vertCutoff = Math.Max(0, (int)mapOffset.X / 50);
            int horizCutoff = Math.Max(0, (int)mapOffset.Y / 50);
            spriteBatch.Draw(blackTexture, new Vector2(0,Math.Max(0,horizCutoff * 50 - mapOffset.Y - 5)), null, Color.White, 0f, Vector2.Zero, new Vector2(750,750) , SpriteEffects.None, 0f);
            for (int x = horizCutoff; x < Math.Min(horizCutoff + 16, cols); x++)
            {
                for (int y = vertCutoff; y < Math.Min(vertCutoff + 16, rows); y++)
                {
                    if (map[y, x] == null) continue;
                    map[y, x].draw(spriteBatch, new Vector2(map[y, x].position.X - mapOffset.X, map[y, x].position.Y - mapOffset.Y));
                }
            }
        }

        public void update(double dt)
        {
            /*
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < cols; y++)
                {
                    if (map[x, y] == null) continue;
                    map[x, y].update(dt, false);
                }
            }
             */
        }

        public List<Rock> getVisibleRocks(Vector2 mapOffset)
        {
            List<Rock> vis = new List<Rock>();

            int vertCutoff = Math.Max(0, (int)mapOffset.X / 50);
            int horizCutoff = Math.Max(0, (int)mapOffset.Y / 50);
            for (int x = horizCutoff; x < Math.Min(horizCutoff + 16, cols); x++)
            {
                for (int y = vertCutoff; y < Math.Min(vertCutoff + 16, rows); y++)
                {
                    try
                    {
                        if (map[y, x] == null) continue;
                        vis.Add(map[y, x]);
                    }
                    catch { }
                }
            }
            return vis;
        }


    }
}
