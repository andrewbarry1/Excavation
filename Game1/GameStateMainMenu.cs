using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

namespace TheGame
{
	public class GameStateMainMenu : GameState
	{

		private List<Sprite> buttons;
		private int buttonCursor;
		private InputHandler inputHandler;
		private Game1 game;
		private ContentManager Content;
        private Menu m;
        private TextWindow tw;
        private Sprite bg;
        private NetworkInterface network;
        private Sprite loadingOverlay;
        private Sprite topMask;
        private TextSystem[] scrollSystems;

		public GameStateMainMenu (ContentManager c, Game1 g)
		{
			Content = c;

			inputHandler = new InputHandler ();
			buttons = new List<Sprite> ();

            network = null;

			game = g;

            m = new Menu(new Vector2(5, 5), new String[] { "NEW GAME", "CONTROLS", "CREDITS", "QUIT" }, new Menu.menuAction[] { startGame, showControls, showCredits, quitGame }, 4, 1, "profont.png", Content);

            bg = new Sprite("excav-bg.png", Content, new Vector2(0, 0), false);
            topMask = new Sprite("excav-topmask.png", Content, new Vector2(0, 0), false);
            loadingOverlay = new Sprite("loading-overlay.png", Content, new Vector2(0, 0), false);

            string[] scrollLines = new string[] {
                "In the year 22XX",
                "A new world supply of energy is discovered.",
                "",
                "Marsium.",
                "",
                "As its name suggests, it can only be found",
                "on Mars.",
                "",
                "Within 50 years of its discovery, only one",
                "corporation controls the world supply.",
                "\\3MarsTek.",
                "",
                "You are a rookie drill technician, sent to",
                "a newly-discovered Marsium deposit far from",
                "Civilization.",
                "",
                "",
                ""};
            scrollSystems = new TextSystem[scrollLines.Length];
            for (int x = 0; x < scrollLines.Length; x++) {
                scrollSystems[x] = new TextSystem("profont.png", Content);
                scrollSystems[x].resetString(new Vector2(0, 0));
                scrollSystems[x].addString(scrollLines[x]);
                scrollSystems[x].setPosition(new Vector2(375 - scrollSystems[x].getWidth()/2, 780 + (scrollSystems[x].getHeight() + 2)*x));
                scrollSystems[x].update(1);
            }
		}

        public void doGameTransition()
        {
            GameStateMars gsm = new GameStateMars(Content, game);
            network.setGSM(gsm);
            network.unloadBufferedMap();
            game.doStateTransition(gsm);
        }

        private void startGame()
        {
            network = new NetworkInterface("ws://game2.andrewbarry.me/ws", Content);
            network.setOnReady(doGameTransition);
        }
        private void showCredits()
        {
            m = null;
            tw = new TextWindow(new string[] { "DEVELOPED BY\\n\\2ANDREW BARRY","MARS PHOTOS:\\n\\2NASA" }, Content, new Vector2(150,125), new Vector2(200,50));
        }
        private void showControls()
        {
            m = null;
            tw = new TextWindow(new string[] { "\\3WASD\\0 - Move. \\3Space+WASD\\0 - Drill in direction.\\n\\3ESC\\0 - Self-destruct. \\3Enter\\0 - Enter shop." }, Content, new Vector2(150, 125), new Vector2(570, 50));
        }

        private void quitGame()
        {
            m = null;
            game.Exit();
        }


        public void update(double dt)
		{
            if (tw != null)
            {
                tw.update(dt);
            }
            else if (m != null)
            {
                m.update(dt);
            }
            if (network == null)
            {
                foreach (TextSystem ts in scrollSystems)
                {
                    ts.setPosition(new Vector2(375 - ts.getWidth() / 2, ts.location.Y - 0.2f));
                }
            }
		}

		public void draw(SpriteBatch spriteBatch)
		{
            bg.draw(spriteBatch);
            if (network == null)
            {
                foreach (TextSystem ts in scrollSystems)
                {
                    ts.draw(spriteBatch);
                }
                topMask.draw(spriteBatch);
            }
            if (tw != null)
            {
                tw.draw(spriteBatch);
            }
            else if (m != null)
            {
                m.draw(spriteBatch);
            }
            if (network != null)
            {
                loadingOverlay.draw(spriteBatch);
            }
		}

		public void handleInput()
		{
			inputHandler.update (Keyboard.GetState ());

            if (m != null)
            {
                m.handleInput(inputHandler);
            }

    		else if (inputHandler.allowSinglePress(Keys.Down) && buttonCursor != 2) {
				buttons [buttonCursor].animHandler.setAnimation (0,false);
				buttonCursor++;
				buttons [buttonCursor].animHandler.setAnimation (1,false);
			}
			else if (inputHandler.allowSinglePress(Keys.Up) && buttonCursor != 0) {
				buttons [buttonCursor].animHandler.setAnimation(0,false);
				buttonCursor--;
				buttons[buttonCursor].animHandler.setAnimation(1,false);
			}

            if (inputHandler.allowSinglePress(Keys.Enter) && tw != null)
            {
                if (tw != null)
                {
                    if (tw.waitingForContinue)
                    {
                        tw.advanceLine();
                    }
                    else
                    {
                        tw.fastForward();
                    }
                    if (tw.done)
                    {
                        tw = null;
                        m = new Menu(new Vector2(5, 5), new String[] { "NEW GAME", "CONTROLS", "CREDITS", "QUIT" }, new Menu.menuAction[] { startGame, showControls, showCredits, quitGame }, 4, 1, "profont.png", Content);
                    }
                }
            }
		}




	}
}

