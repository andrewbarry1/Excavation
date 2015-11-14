using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;


namespace TheGame
{
    class GameStateShop : GameState
    {

        private Game1 Game;
        private ContentManager Content;
        private InputHandler inputHandler;

        private Sprite background;

        private Sprite backButton;
        private Sprite purchaseButton;
        private Sprite ownedButton;
        private Sprite energyButton;
        private Sprite hpButton;
        private Sprite sellButton;

        private Sprite yesButton;
        private Sprite noButton;

        private Sprite poorButton1;
        private Sprite poorButton2;

        private TextSystem headerSystem;
        private TextSystem descriptionSystem;
        private TextSystem confirmSystem;
        private TextSystem[] itemSystems;

        private Vector2 descriptionLocation;

        private string[] descriptions;
        private string[] itemNames;
        private int[] costs;

        private int selectedItem;
        private int action;
        private float charge;

        private SoundEffect boop;

        private Inventory inventory;

        public GameStateShop(Game1 g, ContentManager c, Inventory inv)
        {
            Game = g;
            Content = c;
            inventory = inv;
            inputHandler = new InputHandler();
            background = new Sprite("shop-bg.png", Content, Vector2.Zero, false);
            itemSystems = new TextSystem[8];
            headerSystem = new TextSystem("profont.png", Content);
            setHeader();
            descriptionSystem = new TextSystem("profont.png", Content);
            descriptionLocation = new Vector2(350, 75);

            boop = Content.Load<SoundEffect>("Boop");

            backButton = new Sprite("shop_exit.png", Content, new Vector2(25, 675), false);
            purchaseButton = new Sprite("shop_purchase.png", Content, new Vector2(575, 675), false);
            ownedButton = new Sprite("shop_alreadypurchased.png", Content, new Vector2(575, 675), false);

            energyButton = new Sprite("shop_energyrestore.png", Content, new Vector2(105, 675), false);
            hpButton = new Sprite("shop_healthrestore.png", Content, new Vector2(259, 675), false);
            sellButton = new Sprite("shop_sellcargo.png", Content, new Vector2(413, 675), false);

            yesButton = new Sprite("shop_yes.png", Content, new Vector2(25, 550), false);
            noButton = new Sprite("shop_no.png", Content, new Vector2(80, 550), false);

            poorButton1 = new Sprite("shop_poor1.png", Content, Vector2.Zero, false);
            poorButton2 = new Sprite("shop_poor2.png", Content, Vector2.Zero, false);

            confirmSystem = new TextSystem("profont.png", Content);
            action = -1;

            itemNames = new string[] {
                "Power Drill",
                "Marsium Drill",
                "Reinforced Hull",
                "Impenetrable Hull",
                "Cargo Bay Expansion",
                "Cargo Teleportation Bay",
                "TESLA Battery",
                "Fusion Reactor"};
            costs = new int[] {
                350,
                5000,
                150,
                750,
                750,
                1500,
                750,
                1250
            };
            descriptions = new string[] {
                "\\1DRILLS UP TO 2x FASTER THAN\\nYOUR STANDARD DRILL!\\0\\nMade from a lightweight alloy,\\nthis drill generates double\\nthe power from the same\\namount of energy as a\\nregular drill. Great\\nfor tearing through tough\\nbedrock and solid chunks\\nof diamond!",
                "\\1THE DRILL YOUR MOTHER WARNED\\nYOU ABOUT!\\0\\nState-of-the-art drill technology\\nand chemically treated Marsium\\nstrength combine to bring\\nto you the most powerful\\ndrill ever made. Performs\\nthe work of eight standard drills\\nand without using a single\\ndrop extra of energy.\\nBuy now! What are you, poor?",
                "\\1NEVER FEAR A HIGH FALL AGAIN!\\0\\nStandard \\3Marstek\\0(tm)\\nEXC-07 hull reinforced with\\ndiamonds to provide enhanced\\nprotection against rough\\nlandings, pockets of lava, and\\neverything inbetween.",
                "\\1THE BEST ARMOR MONEY CAN BUY!\\0\\nAfter decades of advanced\\nresearch, top \\3Marstek\\0(tm)\\nscientists have yet to find a\\nprojectile that can damage this\\nhull! Hypertrains, space\\nelevators, and even FTL particles\\nfail to leave a dent!",
                "\\1WHAT'RE YOU GONNA DO WITH ALL\\nTHAT JUNK?\\0\\nStore it in this expanded\\ncargo bay! Double the\\ncapacity of the standard\\nmodel, this cargo expansion\\nprovides all the space you\\nneed to store Marsium on\\nlong mining expeditions.",
                "\\1A WHOLE NEW MEANING OF\\nCLOUD STORAGE!\\0\\nWhy store cargo on your ship,\\nwhen you could store it\\nin our warehouse! Just\\nthrow your cargo in and\\nhit the button, and\\nyour precious cargo will be\\ninstantly transported to\\nsecure \\3MarsTek\\0(tm) storage\\nfacilities. Don't worry, we\\npromise we'll give it back!",
                "\\1THE BATTERY OF THE FUTURE!\\0\\nEver since the days of the 21st\\ncentury, men have strived to\\nbuild the ultimate battery. Now,\\n\\3MarsTek\\0(tm) has done it! This\\nbattery harnesses the power of\\nquantum entanglement to deliver\\n5x the power of any other\\nbattery on the market.",
                "\\1IS THIS THING EVEN SAFE?\\0\\nOf course it is! Why carry\\naround a bulky inefficient\\nbattery when you could suck\\npower from your very own\\nsupergiant star! Provides\\nnear-limitless energy, and\\nwill only create a black hole if\\nyou forget to clean the\\nexhaust pipe."
            };

            selectedItem = -1;

            for (int x = 0; x < 8; x++)
            {
                itemSystems[x] = new TextSystem("profont.png", Content);
                itemSystems[x].resetString(new Vector2(25, 50 + (x * 50)));
                itemSystems[x].addString(itemNames[x]);
            }

        }

        private void setHeader()
        {
            headerSystem.resetString(new Vector2(5, 5));
            headerSystem.addString("Welcome To \\3MARSMART\\0 - your balance is \\3$" + inventory.MONEY);
            headerSystem.update(1);
        }

        public void update(double dt)
        {
            descriptionSystem.update(dt);
        }

        public void handleInput()
        {
            inputHandler.updateMouse(Mouse.GetState());
            if (inputHandler.allowSingleClick())
            {
                Vector2 mousePosition = inputHandler.mousePosition;
                for (int x = 0; x < 8; x++)
                {
                    TextSystem ts = itemSystems[x];
                    Rectangle tsRect = new Rectangle((int)ts.location.X, (int)ts.location.Y, ts.getWidth(), ts.getHeight());
                    if (tsRect.Contains(mousePosition))
                    {
                        boop.Play();
                        selectedItem = x;
                        descriptionSystem.resetString(descriptionLocation);
                        descriptionSystem.addString(descriptions[x] + "\\n\\3Costs $" + costs[x]);
                        descriptionSystem.update(1);
                        return;
                    }
                }
                if (backButton.boundingRectangle.Contains(mousePosition))
                {
                    boop.Play();
                    Game.removeState();
                }
                else if (purchaseButton.boundingRectangle.Contains(mousePosition) && costs[selectedItem] <= inventory.MONEY)
                {
                    boop.Play();
                    action = 0;
                    confirmSystem.resetString(new Vector2(25, 500));
                    charge = (float)Math.Round((double)costs[selectedItem],2);
                    confirmSystem.addString("Purchase \\3" + itemNames[selectedItem] + "\\0? Cost: \\3$" + charge + "\\0");
                    confirmSystem.update(1);
                }
                else if (energyButton.boundingRectangle.Contains(mousePosition) && (float)Math.Round((double)(inventory.MAX_ENERGY - inventory.ENERGY) / 5, 2) <= inventory.MONEY)
                {
                    boop.Play();
                    action = 1;
                    confirmSystem.resetString(new Vector2(25, 500));
                    charge = (float)Math.Round((double)(inventory.MAX_ENERGY - inventory.ENERGY) / 5, 2);
                    confirmSystem.addString("Restore energy? Cost: \\3$" + charge + "\\0");
                    confirmSystem.update(1);
                }
                else if (hpButton.boundingRectangle.Contains(mousePosition) && (float)Math.Round((double)(inventory.MAX_HP - inventory.HP), 2) <= inventory.MONEY)
                {
                    boop.Play();
                    action = 2;
                    confirmSystem.resetString(new Vector2(25, 500));
                    charge = (float)Math.Round((double)(inventory.MAX_HP - inventory.HP), 2);
                    confirmSystem.addString("Repair hull? Cost: \\3$" + charge + "\\0");
                    confirmSystem.update(1);
                }
                else if (sellButton.boundingRectangle.Contains(mousePosition) && inventory.getWeight() > 0)
                {
                    boop.Play();
                    action = 3;
                    confirmSystem.resetString(new Vector2(25, 500));
                    charge = (float)Math.Round((double)inventory.getWeight(), 2);
                    confirmSystem.addString("Sell cargo? Profit: \\3$" + charge + "\\0");
                    confirmSystem.update(1);
                }
                else if (action != -1 && yesButton.boundingRectangle.Contains(mousePosition))
                {
                    boop.Play();
                    switch (action)
                    {
                        case 0:
                            purchaseItem(selectedItem);
                            inventory.MONEY -= charge;
                            break;
                        case 1:
                            inventory.MONEY -= charge;
                            inventory.ENERGY = inventory.MAX_ENERGY;
                            break;
                        case 2:
                            inventory.MONEY -= charge;
                            inventory.HP = inventory.MAX_HP;
                            break;
                        case 3:
                            inventory.MONEY += charge;
                            inventory.CARGO = new int[] { 0, 0, 0, 0, 0 };
                            break;
                        default:
                            break;
                    }
                    action = -1;
                    setHeader();
                }
                else if (action != -1 && noButton.boundingRectangle.Contains(mousePosition))
                {
                    boop.Play();
                    action = -1;
                }
            }
            
        }

        void purchaseItem(int inum)
        {
            inventory.INVENTORY[inum] = true;
            switch (inum)
            {
                case 0:
                    inventory.DRILL_POWER = 2;
                    break;
                case 1:
                    inventory.DRILL_POWER = 8;
                    break;
                case 2:
                    inventory.MAX_HP = 200;
                    break;
                case 3:
                    inventory.MAX_HP = int.MaxValue;
                    inventory.HP = int.MaxValue;
                    break;
                case 4:
                    inventory.MAX_CAPACITY = 500;
                    break;
                case 5:
                    inventory.MAX_CAPACITY = int.MaxValue;
                    break;
                case 6:
                    inventory.MAX_ENERGY = 300;
                    break;
                case 7:
                    inventory.MAX_ENERGY = int.MaxValue;
                    inventory.ENERGY = int.MaxValue;
                    break;
                default:
                    break;
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {
            background.draw(spriteBatch);
            headerSystem.draw(spriteBatch);
            foreach (TextSystem ts in itemSystems)
            {
                ts.draw(spriteBatch);
            }
            descriptionSystem.draw(spriteBatch);
            backButton.draw(spriteBatch);
            if (selectedItem != -1 && inventory.INVENTORY[selectedItem]) {
                ownedButton.draw(spriteBatch);
            }
            else if (selectedItem != -1 && !inventory.INVENTORY[selectedItem])
            {
                if (costs[selectedItem] <= inventory.MONEY)
                {
                    purchaseButton.draw(spriteBatch);
                }
                else
                {
                    poorButton2.draw(spriteBatch, purchaseButton.position);
                }
            }
            if (inventory.ENERGY != inventory.MAX_ENERGY && !inventory.INVENTORY[7])
            {
                if ((float)Math.Round((double)(inventory.MAX_ENERGY - inventory.ENERGY) / 5, 2) <= inventory.MONEY)
                {
                    energyButton.draw(spriteBatch);
                }
                else
                {
                    poorButton1.draw(spriteBatch, energyButton.position);
                }
            }
            if (inventory.HP != inventory.MAX_HP && !inventory.INVENTORY[3])
            {
                if ((float)Math.Round((double)(inventory.MAX_HP - inventory.HP), 2) <= inventory.MONEY)
                {
                    hpButton.draw(spriteBatch);
                }
                else
                {
                    poorButton1.draw(spriteBatch, hpButton.position);
                }
            }
            if (action != -1)
            {
                confirmSystem.draw(spriteBatch);
                yesButton.draw(spriteBatch);
                noButton.draw(spriteBatch);
            }
            if (inventory.getWeight() > 0)
            {
                sellButton.draw(spriteBatch);
            }

        }



    }
}
