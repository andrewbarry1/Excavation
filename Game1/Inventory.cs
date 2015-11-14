using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TheGame
{
    class Inventory
    {

        public bool[] INVENTORY;

        public float MONEY;

        public int[] CARGO;

        public float ENERGY;
        public float MAX_ENERGY;

        public float HP;
        public float MAX_HP;

        public float MAX_CAPACITY;
        public float DRILL_POWER;

        public Inventory()
        {
            INVENTORY = new bool[] { false, false, false, false, false, false, false, false };
            MONEY = 0;
            CARGO = new int[] { 0, 0, 0, 0, 0 };
            ENERGY = 100;
            MAX_ENERGY = 100;
            HP = 100;
            MAX_HP = 100;
            MAX_CAPACITY = 100;
            DRILL_POWER = 1;
        }

        public int getWeight()
        {
            return (CARGO[0] + (2 * CARGO[1]) + (3 * CARGO[2]) + (4 * CARGO[3]) + (5 * CARGO[4]));
        }
        

    }
}
