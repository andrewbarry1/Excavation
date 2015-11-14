using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace TheGame
{

    public class TextSystem
	{

		public enum TextEffect { NONE, SHAKE, RAINBOW, RED };

		public Vector2 location;
        public Vector2 letterSize;

        private Texture2D font;
		private string text;
		private List<TextLetter> letters;
		private TextEffect currentEffect;
		private Random ra;
		private Boolean escaping;
		private int currentLine;


		public TextSystem (string fontName, ContentManager Content)
		{
			letters = new List<TextLetter> ();
			font = Content.Load<Texture2D> ("Fonts/" + fontName);
            StreamReader fontMeta = new StreamReader("Content/Fonts/" + fontName + "_meta");
            letterSize = new Vector2(int.Parse(fontMeta.ReadLine()), int.Parse(fontMeta.ReadLine()));
            fontMeta.Close();
			ra = new Random ();
			currentEffect = TextEffect.NONE;
			escaping = false;
			currentLine = 0;
			text = "";
		}

		public void resetString(Vector2 startingLocation) {
            text = "";
            currentLine = 0;
            letters = new List<TextLetter>();
            currentEffect = TextEffect.NONE;
            escaping = false;
			location = startingLocation;
		}

        public void setPosition(Vector2 loc)
        {
            location = loc;
        }

        public void addString(string s)
        {
            foreach (char c in s)
            {
                addLetter(c);
            }
        }

		public void addLetter(char c) {
            if (c == '\\') // next character will be escape
            {
                escaping = true;
            } else if (escaping && c == 'n')
            {
                currentLine++;
                escaping = false;
            } else if (escaping && c == 'c') // new color!
            {

			} else if (escaping)
            {
				int newEffect = int.Parse (c.ToString ());
				switchEffect ((TextEffect)newEffect);
				escaping = false;
			}
            else {
				TextLetter tl = new TextLetter (c, currentEffect, letterSize, ra, currentLine);
				letters.Add (tl);
			}
		}

		// sets all future letters' effects
		public void switchEffect(TextEffect newEffect) {
			currentEffect = newEffect;
		}

		// sets all letters' effects
		public void setEffect(TextEffect newEffect) {
			foreach (TextLetter tl in letters) {
				tl.effect = newEffect;
			}

		}

		public void update(double dt) {
			foreach (TextLetter tl in letters) {
				tl.update (dt);
			}
		}

		public void draw(SpriteBatch spriteBatch) {
			Vector2 texLocation = location;
			int lastLine = 0;
			foreach (TextLetter tl in letters) {
				if (tl.line != lastLine) {
					texLocation = new Vector2 (location.X, location.Y + ((letterSize.Y + 2) * tl.line));
					lastLine = tl.line;
				}
				tl.draw (spriteBatch, texLocation, font);
				texLocation = new Vector2 (texLocation.X + letterSize.X, texLocation.Y);
			}
		}

		public int getWidth() {
			return (int)letterSize.X * (letters.Count + 1);
		}
		public int getHeight() {
			return (int)letterSize.Y * (currentLine + 2);
		}

	}

	public class TextLetter
	{
		public int line;
		public TextSystem.TextEffect effect;

        private Vector2 letterSize;
		private char letter;
		private Random ra;

		// effect-specific stuff.
		private Color currentColor;
		private Vector2 offset;
		private float diffX;
		private float diffY;

		public TextLetter (char c, TextSystem.TextEffect te, Vector2 ls, Random r, int l)
		{
            letterSize = ls;
			line = l;
			letter = c;
			effect = te;
			ra = r;
			offset = Vector2.Zero;
			currentColor = Color.White;
		}

		public void update(double dt) {
			switch (effect) {
			case TextSystem.TextEffect.NONE: // reset other effects if necessary
				if (!currentColor.Equals (Color.White)) {
					currentColor = Color.White;
				}
				if (!offset.Equals (Vector2.Zero)) {
					offset = Vector2.Zero;
				}
				break;
			case TextSystem.TextEffect.RAINBOW: // change letter color
				Vector3 current = currentColor.ToVector3 ();
				float chgR = .005f * (float)dt, chgG = .005f * (float)dt, chgB = .005f * (float)dt;
				if (ra.NextDouble () > 0.5) {
					chgR *= -1;
				}
				if (ra.NextDouble () > 0.5) {
					chgG *= -1;
				}
				if (ra.NextDouble () > 0.5) {
					chgB *= -1;
				}
				currentColor = new Color (current.X + chgR, current.Y + chgG, current.Z + chgB);
				break;
			case TextSystem.TextEffect.SHAKE: // alter offset vector, considering current offset
				int offX = -1, offY = -1;
				if (ra.NextDouble () > 0.5*diffX) {
					offX *= -1;
				}
				if (ra.NextDouble () > 0.5*diffY) {
					offY *= -1;
				}
				offset += new Vector2 (offX, offY);
				break;
            case TextSystem.TextEffect.RED: // make the text red
                currentColor = new Color(255, 0, 0);
                break;
			default:
				break;
			}
		}

		public void draw(SpriteBatch spriteBatch, Vector2 drawLocation, Texture2D font) {
			spriteBatch.Draw (font, drawLocation + offset,
                new Rectangle ((letter-1) * (int)letterSize.X, 0, (int)letterSize.X, (int)letterSize.Y), currentColor);
			if (effect == TextSystem.TextEffect.SHAKE) {
				diffX = (drawLocation.X + offset.X) - drawLocation.X;
				diffY = (drawLocation.Y + offset.Y) - drawLocation.Y;
			}
		}

	}

}

