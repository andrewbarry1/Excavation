using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace TheGame
{
    class GameStateMars : GameState
    {
        private ContentManager Content;
        private Game1 Game;
        private InputHandler inputHandler;

        private Sprite bg;
        private Player player;
        public Dictionary<String, Player> otherPlayers;
        private Sprite shop;
        public RockMap rockMap;
        private Vector2 mapOffset;

        private bool moveLeft;
        private bool moveRight;
        private bool thrusters;

        private float transY;
        private float transX;
        private Vector2 playerWindowLocation;

        private bool stableGround;
        private bool drilling;
        private float drillTime;
        private Vector2 drillRock;

        private bool dead;
        private Sprite deadOverlay;

        private Rectangle windowRect;

        private NetworkInterface network;

        private SoundEffect drillSound;
        private SoundEffectInstance drillSoundInstance;
        private SoundEffect thrustSound;
        private SoundEffectInstance thrustSoundInstance;
        private SoundEffect boomSound;

        Texture2D redTexture;
        Texture2D greenTexture;
        Texture2D blueTexture;
        Texture2D blackTexture;


        TextSystem sdSystem;

        public GameStateMars(ContentManager c, Game1 g)
        {
            Game = g;
            Content = c;

            rockMap = new RockMap(Content);

            windowRect = new Rectangle(0, 0, 1500, int.MaxValue);

            inputHandler = new InputHandler();

            drillSound = Content.Load<SoundEffect>("Drill");
            thrustSound = Content.Load<SoundEffect>("Thrust");
            drillSoundInstance = Content.Load<SoundEffect>("Drill").CreateInstance();
            drillSoundInstance.IsLooped = true;
            thrustSoundInstance = Content.Load<SoundEffect>("Thrust").CreateInstance();
            thrustSoundInstance.IsLooped = true;
            boomSound = Content.Load<SoundEffect>("Boom");

            sdSystem = new TextSystem("profont.png", Content);
            sdSystem.resetString(new Vector2(5, 725));
            sdSystem.addString("Out of energy. Press \\3ESC to self-destruct");

            bg = new Sprite("mars-bg.png", Content, new Vector2(0, 0), false);
            player = new Player("miner.png", Content, new Vector2(windowRect.Width/2, 25));
            shop = new Sprite("shop.png", Content, new Vector2(500, 25));
            deadOverlay = new Sprite("dead.png", Content, new Vector2(0, 0), false);

            otherPlayers = new Dictionary<string, Player>();

            mapOffset = new Vector2(0,-125);

            thrusters = false;
            transY = 0f;
            transX = 0f;

            drillTime = 0f;

            redTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            redTexture.SetData(new Color[] { Color.Red });
            blueTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            blueTexture.SetData(new Color[] { Color.Blue });
            greenTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            greenTexture.SetData(new Color[] { Color.Green });
            blackTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            blackTexture.SetData(new Color[] { Color.Black });
        }

        public void update(double dt)
        {
            rockMap.update(dt);
            player.update(dt);
            doPlayerPhysics(dt);
            if (drilling)
            {
                drill(dt);
            }
            shop.update(dt);
            foreach (Player p in otherPlayers.Values)
            {
                p.update(dt);
            }
        }

        public void setNetworkInterface(NetworkInterface ni)
        {
            network = ni;
            player.animHandler.doNetworkedAnimationUpdates(ni);
        }

        void drill(double dt)
        {
            if (drillRock.Y < 0 || drillRock.X < 0 || rockMap.map[(int)drillRock.X, (int)drillRock.Y] == null)
            {
                drillTime = 0;
                drilling = false;
                drillSoundInstance.Stop();
                player.animHandler.setAnimation(0, true);
                return;
            }
            drillTime += (float)(dt / 1000);
            if (drillTime >= rockMap.map[(int)drillRock.X, (int)drillRock.Y].mineTime / player.inventory.DRILL_POWER)
            {
                player.inventory.ENERGY -= rockMap.map[(int)drillRock.X, (int)drillRock.Y].mineTime * 1.5f;
                Rock.RockType rockType = rockMap.map[(int)drillRock.X, (int)drillRock.Y].rockType;
                player.inventory.CARGO[(int)rockType]++;
                rockMap.map[(int)drillRock.X, (int)drillRock.Y] = null;
                if (network.connected)
                {
                    network.sendDrillUpdate(drillRock);
                }
                drillTime = 0;
                drilling = false;
                drillSoundInstance.Stop();
                player.animHandler.setAnimation(0, true);
            }
        }

        void doPlayerPhysics(double dt)
        {
            player.velocity.Y += 9.81f * ((float)dt / 1000);
            if (thrusters)
            {
                player.velocity.Y -= 18f * ((float)dt / 1000);
            }
            if (moveLeft)
            {
                player.velocity.X -= 5f * ((float)dt / 1000);
            }
            else if (!moveLeft && player.velocity.X < 0 && stableGround)
            {
                player.velocity.X += Math.Min(10f * ((float)dt / 1000), -1 * player.velocity.X);
            }
            if (moveRight)
            {
                player.velocity.X += 5f * ((float)dt / 1000);
            }
            else if (!moveRight && player.velocity.X > 0 && stableGround)
            {
                player.velocity.X -= Math.Min(10f * ((float)dt / 1000), player.velocity.X);
            }
            int xLoc = (int)player.position.X / 50;
            Rectangle xRect = new Rectangle((int)(player.position.X + player.velocity.X), (int)player.position.Y, 50, 50);
            Rectangle yRect = new Rectangle((int)player.position.X, (int)(player.position.Y + player.velocity.Y), 50, 50);
            bool willMoveX = true;
            bool willMoveY = true;
            foreach (Rock r in rockMap.getVisibleRocks(mapOffset))
            {
                if (xRect.Intersects(r.boundingRectangle) || !windowRect.Contains(xRect))
                {
                    willMoveX = false;
                    player.inventory.HP -= Math.Max(10, player.velocity.X) - 10;
                    if (player.inventory.HP <= 0)
                    {
                        killPlayer();
                    }
                    player.velocity.X = 0;
                }
                if (yRect.Intersects(r.boundingRectangle) || !windowRect.Contains(yRect))
                {
                    willMoveY = false;
                    player.inventory.HP -= Math.Max(10, player.velocity.Y) - 10;
                    if (player.inventory.HP <= 0)
                    {
                        killPlayer();
                    }
                    player.velocity.Y = 0;
                    if (!thrusters)
                    {
                        stableGround = true;
                    }
                }
            }
            if (willMoveY)
            {
                player.position.Y += player.velocity.Y;
                if (thrusters)
                {
                    player.inventory.ENERGY -= .03f;
                    if (player.inventory.ENERGY <= 0)
                    {
                        player.inventory.ENERGY = 0;
                        thrusters = false;
                        thrustSoundInstance.Stop();
                        player.animHandler.setAnimation(0, true);
                    }
                }
            }
            if (willMoveX)
            {
                player.position.X += player.velocity.X;
            }

            if (((moveLeft || moveRight) || willMoveY) && network.mapLoaded)
            {
                network.sendLocationUpdate(player);
            }
        }

        void killPlayer()
        {
            player.velocity = Vector2.Zero;
            player.animHandler.setAnimation(4,true);
            player.lockAnimation();
            dead = true;
        }

        // find player (x,y) coordinates in rockMap
        Vector2 findPlayerMapPosition()
        {
            int xPos = (int)(player.boundingRectangle.Center.X / 50);
            int yPos = (int)(((player.boundingRectangle.Center.Y - 125) / 50.0) - 0.5);
            Vector2 pos = new Vector2(xPos,yPos);
            if (yPos >= rockMap.cols - 20)
            {
                if (network.mapLoaded)
                {
                    network.requestMoreMap();
                }
                else
                {
                    network.generateMoreMap(rockMap);
                }
            }
            return pos;
        }

        public void handleInput()
        {
            inputHandler.update(Keyboard.GetState());
            inputHandler.updateMouse(Mouse.GetState());
            if (dead)
            {
                if (inputHandler.allowSinglePress(Keys.Space))
                {
                    dead = false;
                    player.animHandler.unlockAnimation();
                    player.animHandler.setAnimation(0, true);
                    player.inventory.MONEY = (float)Math.Round(player.inventory.MONEY / 2, 2);
                    player.inventory.CARGO = new int[] { 0, 0, 0, 0, 0 };
                    player.inventory.HP = player.inventory.MAX_HP;
                    player.inventory.ENERGY = player.inventory.MAX_ENERGY;
                    player.position = new Vector2(windowRect.Width/2, 10);
                }
                return;
            }
            if (inputHandler.allowSinglePress(Keys.Escape))
            {
                killPlayer();
            }
            if (inputHandler.allowSinglePress(Keys.OemTilde))
            {
                player.inventory.MONEY += 15000;
            }
            if (inputHandler.allowSinglePress(Keys.Up) && !thrusters && player.inventory.ENERGY >= 0.3)
            {
                thrusters = true;
                thrustSoundInstance.Play();
                player.animHandler.setAnimation(2, true);
                drilling = false;
                drillSoundInstance.Stop();
            }
            else if (!inputHandler.allowMultiPress(Keys.Up) && thrusters)
            {
                thrusters = false;
                thrustSoundInstance.Stop();
                player.animHandler.setAnimation(0, true);
                drilling = false;
                drillSoundInstance.Stop();
            }
            if (inputHandler.allowMultiPress(Keys.Up))
            {
                stableGround = false;
            }

            if (inputHandler.allowSinglePress(Keys.Left) && !moveLeft)
            {
                Console.WriteLine("Enabling moveLeft, flipping sprite");
                network.sendLeftTurn();
                moveLeft = true;
                player.facingLeft = true;
                drilling = false;
                drillSoundInstance.Stop();
                if (!thrusters)
                {
                    player.animHandler.setAnimation(0, true);
                }
            }
            else if (!inputHandler.allowMultiPress(Keys.Left) && moveLeft)
            {
                Console.WriteLine("Disabling moveLeft");
                moveLeft = false;
            }

            if (inputHandler.allowSinglePress(Keys.Right) && !moveRight)
            {
                Console.WriteLine("Enabling moveRight");
                network.sendRightTurn();
                moveRight = true;
                player.facingLeft = false;
                drilling = false;
                drillSoundInstance.Stop();
                if (!thrusters)
                {
                    player.animHandler.setAnimation(0, true);
                }
            }
            else if (!inputHandler.allowMultiPress(Keys.Right) && moveRight)
            {
                Console.WriteLine("disabling moveRight");
                moveRight = false;
            }
            
            if (inputHandler.allowSinglePress(Keys.Space) && !drilling && stableGround)
            {
                Vector2 playerPosition = findPlayerMapPosition();
                drillTime = 0;
                if (inputHandler.allowMultiPress(Keys.Down))
                {
                    drillRock = RockMap.convertCoordinates(new Vector2(playerPosition.X, playerPosition.Y + 1));
                    if (drillRock.X >= 0 && drillRock.Y >= 0 &&
                        rockMap.map[(int)drillRock.X,(int)drillRock.Y] != null && 
                        (int)rockMap.map[(int)drillRock.X, (int)drillRock.Y].rockType + 1 + player.inventory.getWeight() <= player.inventory.MAX_CAPACITY)
                    {
                        drilling = true;
                        drillSoundInstance.Play();
                        player.animHandler.setAnimation(1, true);
                        player.velocity.X = 0f;
                    }
                }
                else if (player.facingLeft && playerPosition.Y != -1)
                {
                    drillRock = RockMap.convertCoordinates(new Vector2(playerPosition.X - 1, playerPosition.Y));
                    if (drillRock.X >= 0 && drillRock.Y >= 0 &&
                        rockMap.map[(int)drillRock.X, (int)drillRock.Y] != null &&
                        (int)rockMap.map[(int)drillRock.X, (int)drillRock.Y].rockType + 1 + player.inventory.getWeight() <= player.inventory.MAX_CAPACITY)
                    {
                        drilling = true;
                        drillSoundInstance.Play();
                        player.animHandler.setAnimation(1, true);
                    }
                }
                else if (!player.facingLeft && playerPosition.Y != -1)
                {
                    drillRock = RockMap.convertCoordinates(new Vector2(playerPosition.X + 1, playerPosition.Y));
                    if (drillRock.X >= 0 && drillRock.Y >= 0 &&
                        rockMap.map[(int)drillRock.X, (int)drillRock.Y] != null && 
                        (int)rockMap.map[(int)drillRock.X, (int)drillRock.Y].rockType + 1 + player.inventory.getWeight() <= player.inventory.MAX_CAPACITY)
                    {
                        drilling = true;
                        drillSoundInstance.Play();
                        player.animHandler.setAnimation(1, true);
                    }
                }
            }
            else if (!inputHandler.allowMultiPress(Keys.Space) && drilling)
            {
                drilling = false;
                drillSoundInstance.Stop();
                drillTime = 0;
                player.animHandler.setAnimation(0, true);
            }

            else if (inputHandler.allowSinglePress(Keys.U))
            {
                player.position = new Vector2(10, 10);
                mapOffset = new Vector2(0, -125);
            }

            else if (inputHandler.allowSinglePress(Keys.Enter) && player.boundingRectangle.Intersects(shop.boundingRectangle))
            {
                thrustSoundInstance.Stop();
                Game.doStateTransition(new GameStateShop(Game, Content, player.inventory));
            }
        }

        Vector2 calcuatePlayerOffset(Vector2 pwl)
        {
            if (pwl.Y >= 375)
            {
                transY = player.position.Y - 375;
            }
            else
            {
                transY = 0;
            }

            if (pwl.X < 375)
            {
                transX = 0;
            }
            else if (pwl.X == 375)
            {
                transX = player.position.X - 375;
            }
            else if (pwl.X > 375)
            {
                transX = windowRect.Width - 750;
            }
            Vector2 vec = new Vector2(transX, -125 + transY);
            return vec;
        }

        // find player location in window
        Vector2 findPWL(Vector2 ce)
        {
            Vector2 vec = new Vector2();

            if (ce.X < 375)
            {
                vec.X = ce.X;
            }
            else if (ce.X >= 375 && ce.X <= windowRect.Width - 375)
            {
                vec.X = 375;
            }
            else if (ce.X > windowRect.Width - 375 && ce.X <= windowRect.Width)
            {
                vec.X = ce.X - (windowRect.Width - 750);
            }

            vec.Y = Math.Min(ce.Y, 375); // TODO? Do we care?

            return vec;
        }

        // find other player's window position relative to main player
        Vector2 findPRWL(Vector2 p, Vector2 pwl, Vector2 other)
        {
            Vector2 rel = new Vector2(other.X - p.X, other.Y - p.Y);
            return new Vector2(pwl.X + rel.X, pwl.Y + rel.Y);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            bg.draw(spriteBatch);
            playerWindowLocation = findPWL(player.position);
            mapOffset = calcuatePlayerOffset(playerWindowLocation);
            rockMap.draw(spriteBatch, mapOffset);
            player.draw(spriteBatch, playerWindowLocation);
            foreach (String opid in otherPlayers.Keys)
            {
                otherPlayers[opid].draw(spriteBatch, findPRWL(player.position, playerWindowLocation, otherPlayers[opid].position));
            }
            shop.draw(spriteBatch, new Vector2(shop.position.X - transX, shop.position.Y - transY));
            if (dead)
            {
                deadOverlay.draw(spriteBatch);
            }
            else
            {
                drawUI(spriteBatch);
            }
        }

        private void drawUI(SpriteBatch spriteBatch)
        {
            float hpPixelTick = 100 / player.inventory.MAX_HP;
            spriteBatch.Draw(redTexture, new Vector2(5,5), null, Color.White * 0.5f, 0f, Vector2.Zero, new Vector2(player.inventory.HP * hpPixelTick,20), SpriteEffects.None, 0f);
            spriteBatch.Draw(blackTexture, new Vector2(Math.Max((player.inventory.HP * hpPixelTick) + 5,5), 5), null, Color.White * 0.5f, 0f, Vector2.Zero, new Vector2(100 - (player.inventory.HP * hpPixelTick), 20), SpriteEffects.None, 0f);

            float energyPixelTick = 100 / player.inventory.MAX_ENERGY;
            spriteBatch.Draw(blueTexture, new Vector2(5, 25), null, Color.White * 0.5f, 0f, Vector2.Zero, new Vector2(player.inventory.ENERGY * energyPixelTick, 20), SpriteEffects.None, 0f);
            spriteBatch.Draw(blackTexture, new Vector2(Math.Max((player.inventory.ENERGY * energyPixelTick) + 5,5), 25), null, Color.White * 0.5f, 0f, Vector2.Zero, new Vector2(Math.Min(100, 100 - (player.inventory.ENERGY * energyPixelTick)), 20), SpriteEffects.None, 0f);

            float weightPixelTick = 100 / player.inventory.MAX_CAPACITY;
            spriteBatch.Draw(greenTexture, new Vector2(5, 45), null, Color.White * 0.5f, 0f, Vector2.Zero, new Vector2(player.inventory.getWeight() * weightPixelTick, 20), SpriteEffects.None, 0f);
            spriteBatch.Draw(blackTexture, new Vector2((player.inventory.getWeight()*weightPixelTick) + 5, 45), null, Color.White * 0.5f, 0f, Vector2.Zero, new Vector2(100 - (player.inventory.getWeight() * weightPixelTick), 20), SpriteEffects.None, 0f);

            if (player.inventory.ENERGY <= 0.1)
            {
                sdSystem.draw(spriteBatch);
            }
        }

    }
}
 