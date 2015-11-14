using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheGame
{
    class Rock : Sprite
    {

        public enum RockType { DIRT, STONE, IRON, DIAMOND, MARSIUM };

        public float mineTime;
        public RockType rockType;

        public Rock(string filename, ContentManager Content, Vector2 vec, int rType, bool animated=true) : base(filename, Content, vec, animated)
        {
            rockType = (RockType)rType;
            switch (rType)
            {
                case ((int)RockType.DIRT):
                    mineTime = 1.0f;
                    break;
                case ((int)RockType.STONE):
                    mineTime = 2.5f;
                    break;
                case ((int)RockType.IRON):
                    mineTime = 10.0f;
                    break;
                case ((int)RockType.DIAMOND):
                    mineTime = 20.0f;
                    break;
                case ((int)RockType.MARSIUM):
                    mineTime = 60.0f;
                    break;
            }
        }

    }
}
