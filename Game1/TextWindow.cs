using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace TheGame
{
	public class TextWindow
	{
		public string[] lines;
		public bool waitingForContinue;
		public bool done;

		private int currentLine;
		private int currentCharacter;
		private double totalTime;
		private double timePerChar;
        private double basicTPC;
        private double bgAlpha;
		private Texture2D blackTexture;
		private TextSystem textSystem;
        private Vector2 position;
        private Vector2 size;

		public TextWindow (string[] dialog, ContentManager Content, Vector2 pos, Vector2 siz, double tpc = 25, double bgA = 0.5)
		{
			lines = dialog;
            position = pos;
            size = siz;
			currentLine = 0;
			currentCharacter = 0;
			totalTime = 0.0;
			timePerChar = tpc;
            basicTPC = tpc;
            bgAlpha = bgA;
			waitingForContinue = false;
			blackTexture = new Texture2D (Game1.graphics.GraphicsDevice, 1, 1);
			Color[] blackData = new Color[1];
			blackData [0] = Color.Black;
			blackTexture.SetData (blackData);
			textSystem = new TextSystem ("profont.png", Content);
			textSystem.resetString(new Vector2 (position.X+5,position.Y+5));
			done = false;
		}

		public void update(double dt) {

			if (done) {
				return;
			}

			double nextCharTime = (currentCharacter + 1) * timePerChar;

			if (totalTime + dt >= nextCharTime && !waitingForContinue) {
				if (currentCharacter == lines [currentLine].Length) {
					waitingForContinue = true;
					return;
				}
				char characterToAdd = lines[currentLine].ToCharArray()[currentCharacter];
				if (characterToAdd == '\\') {
					textSystem.addLetter(characterToAdd);
					currentCharacter++;
					characterToAdd = lines [currentLine].ToCharArray () [currentCharacter];
					textSystem.addLetter(characterToAdd);
					currentCharacter++;
					characterToAdd = lines [currentLine].ToCharArray () [currentCharacter];
					totalTime += (timePerChar * 2); // make sure there are no awkward pauses
				}
				textSystem.addLetter(characterToAdd);
				currentCharacter++;

			}
			totalTime += dt;
			textSystem.update (dt);
		}

		public void fastForward() {
			timePerChar /= 2;
		}

		public void advanceLine() {
			textSystem.resetString (new Vector2(position.X + 5, position.Y + 5));
			currentCharacter = 0;
			currentLine++;
			waitingForContinue = false;
			timePerChar = basicTPC;
			totalTime = 0.0;
			if (currentLine >= lines.Length) {
				done = true;
				waitingForContinue = true;
			}
		}

		public void draw(SpriteBatch spriteBatch) {
			if (done) {
				return;
			}
			spriteBatch.Draw (blackTexture, position, null, Color.White * (float)bgAlpha, 0f, Vector2.Zero, size, SpriteEffects.None, 0f);
			textSystem.draw (spriteBatch);
		}

	}
}

