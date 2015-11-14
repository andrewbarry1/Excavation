using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

namespace TheGame
{
    public class Game1 : Game
    {

		// CONSTANTS
		public const double FADE_TIME = 1000.0; // fade to white timing
		public const int SCREEN_WIDTH = 750; // window width
		public const int SCREEN_HEIGHT = 750; // window height

        public static GraphicsDeviceManager graphics;

        private SpriteBatch spriteBatch;
		public Stack<GameState> states;

		private Texture2D whiteTexture;
		private double whiteFade;

		public fadeDelegate queuedDelegate;

		private GameState queuedState;
		private bool fadingIn;
		public delegate void fadeDelegate ();


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			this.Window.Title = "EXCAVATION";
        }

        protected override void Initialize()
        {
			graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
			graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
			graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			graphics.ApplyChanges ();

            this.IsMouseVisible = true;

			states = new Stack<GameState> ();

			states.Push (new GameStateMainMenu (Content, this));

			whiteFade = 0.0;

            base.Initialize();

        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
			whiteTexture = new Texture2D (GraphicsDevice, 1, 1);
			Color[] whiteData = new Color[1];
			whiteData [0] = Color.White;
			whiteTexture.SetData (whiteData);
        }



		public void startFade(fadeDelegate fDel) {
			whiteFade = 0.01;
			fadingIn = false;
			queuedDelegate = fDel;
		}

		// do addState after FADE_TIME-second white fade
		public void doStateTransition(GameState newState) {
			startFade (addState);
			queuedState = newState;
		}

        public void removeState()
        {
            startFade(popState);
        }

        public void popState()
        {
            states.Pop();
        }

		// triggers when white is totally blocking current state draw
		public void whiteFadeTrigger() {
			queuedDelegate ();
			fadingIn = true;
		}

		// add GameState to stack and unfade
		private void addState() {
			states.Push (queuedState);
		}


        protected override void Update(GameTime gameTime)
        {
			if (whiteFade <= 0.0) {
				TimeSpan egt = gameTime.ElapsedGameTime;

				double dt = egt.TotalMilliseconds;

				if (states.Count != 0) {
					states.Peek ().update (dt);
					states.Peek ().handleInput ();
				}

			}
			else if (whiteFade >= 1.0) {
				whiteFadeTrigger ();
			}

        }


        protected override void Draw(GameTime gameTime)
        {

           	graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
		
			spriteBatch.Begin ();

			if (states.Count != 0) {
				states.Peek ().draw (spriteBatch);
			}

			if (whiteFade > 0.0) {
				double plusAlpha = gameTime.ElapsedGameTime.TotalMilliseconds / FADE_TIME;
				if (fadingIn)
					whiteFade -= plusAlpha;
				else
					whiteFade += plusAlpha;
				Vector2 scale = new Vector2(graphics.GraphicsDevice.PresentationParameters.BackBufferWidth, graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);
				spriteBatch.Draw (whiteTexture, Vector2.Zero, null, 
				                 Color.White * (float)whiteFade, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}

			spriteBatch.End ();

        }

    }
}

