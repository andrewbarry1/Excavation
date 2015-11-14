using System;
using Microsoft.Xna.Framework.Graphics;


namespace TheGame
{
	public interface GameState
	{

		void update(double dt);
		void handleInput();
		void draw(SpriteBatch spriteBatch);


	}
}

