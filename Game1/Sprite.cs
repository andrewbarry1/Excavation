using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TheGame
{

	// TODO subclass this for NPCs, etc
	class Sprite
	{

		public Texture2D texture;
		public Vector2 position;

		public bool hasCollision;

		public AnimationHandler animHandler;

		public Rectangle boundingRectangle;

        public bool facingLeft;

		public Sprite (string filename, ContentManager Content, Vector2 vec, bool animated=true)
		{
			texture = Content.Load<Texture2D>("Sprites/" + filename);

			hasCollision = true;

			this.position = vec;
            if (animated)
            {
                SpriteMetaInfo smi = new SpriteMetaInfo(filename);
                this.animHandler = new AnimationHandler(smi.height, smi.width, smi.frames, smi.durations, smi.loops);
                boundingRectangle = new Rectangle((int)vec.X, (int)vec.Y, smi.width, smi.height);
            }
            else
            {
                this.animHandler = new AnimationHandler(texture.Height, texture.Width, new List<int>(), new List<int>(), new List<bool>());
                boundingRectangle = new Rectangle((int)vec.X, (int)vec.Y, texture.Width, texture.Height);
            }
            this.animHandler.setAnimation(0, true);
		}

		public void update(double dt, bool updateBounds=true)
		{
            if (updateBounds)
            {
                boundingRectangle = new Rectangle((int)position.X, (int)position.Y, boundingRectangle.Width, boundingRectangle.Height);
            }
			this.animHandler.update (dt);
		}

		public void draw(SpriteBatch spriteBatch)
		{
            if (facingLeft)
            {
                spriteBatch.Draw(this.texture, position: this.position, sourceRectangle: this.animHandler.textureSection, effects: SpriteEffects.FlipHorizontally);
            }
            else
            {
                spriteBatch.Draw(this.texture, this.position, this.animHandler.textureSection, Color.White);
            }
		}

		public void draw(SpriteBatch spriteBatch, Vector2 coordinates) { // used when this.position is not in window's coordinate system
            if (facingLeft)
            {
                spriteBatch.Draw(this.texture, position: coordinates, sourceRectangle: this.animHandler.textureSection, effects: SpriteEffects.FlipHorizontally);
            }
            else
            {
                spriteBatch.Draw(this.texture, coordinates, this.animHandler.textureSection, Color.White);
            }
		}

		public void collide() {
			return;
		}

		public void interact() {
			return;
		}

		public static double getDistance(Sprite one, Sprite two) {
			return Math.Sqrt (Math.Pow (one.boundingRectangle.Center.X - two.boundingRectangle.Center.X, 2) + Math.Pow (one.boundingRectangle.Center.Y - two.boundingRectangle.Center.Y, 2));
		}

        public void lockAnimation()
        {
            animHandler.lockAnimation();
        }
        public void unlockAnimation()
        {
            animHandler.unlockAnimation();
        }

        public static double getRectangleDistance(Point one, Point two)
        {
            return Math.Sqrt(Math.Pow(one.X - two.X, 2) + Math.Pow(one.Y - two.Y, 2));
        }
    }

    public class SpriteMetaInfo
    {

        public int height;
        public int width;
        public List<int> frames;
        public List<int> durations;
        public List<bool> loops;
        public bool collides;


        public SpriteMetaInfo(String texture)
        {
            frames = new List<int>();
            durations = new List<int>();
            loops = new List<bool>();
            StreamReader sr = new StreamReader("Content/Sprites/" + texture + "_meta");
            width = int.Parse(sr.ReadLine());
            height = int.Parse(sr.ReadLine());
            int nAnimations = int.Parse(sr.ReadLine());
            string frLine = sr.ReadLine();
            string duLine = sr.ReadLine();
            string loLine = sr.ReadLine();
            string[] frStrings = frLine.Split(',');
            string[] duStrings = duLine.Split(',');
            string[] loStrings = loLine.Split(',');
            for (int x = 0; x < nAnimations; x++)
            {
                frames.Add(int.Parse(frStrings[x]));
                durations.Add(int.Parse(duStrings[x]));
                loops.Add((int.Parse(loStrings[x]) == 1));
            }
                sr.Close();
        }

    }
}

