using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TheGame
{
    class Player : Sprite
    {

        public Vector2 velocity;
        public Inventory inventory;

        public Player(string filename, ContentManager Content, Vector2 vec, bool animated = true)
            : base(filename, Content, vec, animated)
        {
            velocity = new Vector2(0, 0);
            inventory = new Inventory();
        }

    }
}
