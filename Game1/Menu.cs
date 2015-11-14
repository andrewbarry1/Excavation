using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace TheGame
{
	public class Menu
	{

		public bool inUse;
		public int cursor;
		public delegate void menuAction ();

		private menuAction[] actions;
		private int rows;
		private int cols;
		private TextSystem[] textSystems;
		private Vector2 location;
		private Vector2 bufferedLocation;
		private Vector2 size;
		private Texture2D blackTexture;
		private Texture2D yellowTexture;

        private SoundEffect boop;

		public Menu (Vector2 loc, string[] options, menuAction[] a, int r, int c, string font, ContentManager Content)
		{
			actions = a;
			cursor = 0;
			rows = r;
			cols = c;
			location = loc;
            boop = Content.Load<SoundEffect>("boop");
			bufferedLocation = new Vector2 (loc.X + 4, loc.Y + 4);
			textSystems = new TextSystem[options.Length];
			for (int x = 0; x < options.Length; x++) {
				textSystems [x] = new TextSystem (font, Content);
				foreach (char ch in options[x].ToCharArray()) {
					textSystems[x].addLetter(ch);
				}
			}
			for (int x = 0; x < textSystems.Length; x++) {
				TextSystem ts = textSystems[x];
				ts.location = getTrueLocation (getVectorFromIndex (x));
			}
			int farthestRightOption = textSystems.Length - 1;
			for (int x = 0; x < textSystems.Length; x++) {
				if (x % cols == cols - 1 && textSystems [x].getWidth () > textSystems [farthestRightOption].getWidth ()) {
					farthestRightOption = x;
				}
			}
			size = new Vector2(textSystems[farthestRightOption].location.X + textSystems[farthestRightOption].getWidth() - location.X - 8,
			                   textSystems[textSystems.Length - 1].location.Y + textSystems[textSystems.Length - 1].getHeight() - location.Y - 8);
			blackTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
			Color[] blackTextureData = new Color[1];
			blackTextureData[0] = Color.Black;
			blackTexture.SetData(blackTextureData);
			yellowTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
			Color[] yellowTextureData = new Color[1];
			yellowTextureData[0] = Color.Gold;
			yellowTexture.SetData(yellowTextureData);
		}

		// helper functions for finding the true locations of textSystems in a 2D matrix represented as a 1D array
		private Vector2 getVectorFromIndex(int index) {
			return new Vector2 (index % cols, (int)(index / cols));
		}
		private int getIndexFromVector(Vector2 index) {
			return ((int)index.Y * cols) + (int)index.X;
		}
		private Vector2 getTrueLocation(Vector2 indexLocation) {
			int totalX = 0, totalY = 0;
			for (int x = 0; x < indexLocation.X; x++) {
				Vector2 previousXVector = new Vector2(x, indexLocation.Y);
				totalX += textSystems [getIndexFromVector (previousXVector)].getWidth();
			}
			for (int y = 0; y < indexLocation.Y; y++) {
				Vector2 previousYVector = new Vector2 (indexLocation.X, y);
				totalY += textSystems [getIndexFromVector (previousYVector)].getHeight ();
			}
			return new Vector2 (bufferedLocation.X + totalX, bufferedLocation.Y + totalY);
		}

		public void update(double dt) {
			foreach (TextSystem ts in textSystems) {
				ts.update (dt);
			}
		}

		// NOTE: as a pseudo-GameState, Menu.cs pigybacks off its parent GameState's inputHandler.
		public void handleInput(InputHandler inputHandler) {
			if (inputHandler.allowSinglePress (Keys.Up)) {
				if (cursor >= cols) {
                    boop.Play();
					cursor -= cols;
				}
			}
			if (inputHandler.allowSinglePress (Keys.Down)) {
				if (cursor + cols < textSystems.Length) {
                    boop.Play();
					cursor += cols;
				}
			}
			if (inputHandler.allowSinglePress (Keys.Right)) {
				if (cursor % cols != cols - 1) {
                    boop.Play();
					cursor++;
				}
			}
			if (inputHandler.allowSinglePress (Keys.Left)) {
				if (cursor % cols != 0) {
                    boop.Play();
					cursor--;
				}
			}
			if (inputHandler.allowSinglePress (Keys.Enter)) {
                boop.Play();
				inUse = false;
				actions [cursor] ();
				cursor = 0;
			}

		}

		public void draw(SpriteBatch spriteBatch) {
			spriteBatch.Draw (blackTexture, location, null, Color.White * 0.5f, 0f, Vector2.Zero, size, SpriteEffects.None, 0f);
			foreach (TextSystem ts in textSystems) {
				ts.draw (spriteBatch);
			}
			spriteBatch.Draw (yellowTexture, textSystems[cursor].location, null, 
			                  Color.White * 0.25f, 0f, Vector2.Zero, new Vector2(textSystems[cursor].getWidth() - textSystems[cursor].letterSize.X, textSystems[cursor].getHeight() - textSystems[cursor].letterSize.Y),
                              SpriteEffects.None, 0f);
		}

	}
}

