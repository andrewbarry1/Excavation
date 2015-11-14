using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace TheGame
{
    class NetworkInterface
    {
        private WebSocket sock;
        private GameStateMars game;
        private ContentManager Content;

        public int messagesReceived;
        public DateTime startTime;

        public bool connected;
        public int roomNumber;

        public string roomId;
        public string playerId;
        public bool mapLoaded;

        private Game1.fadeDelegate onReady;

        private List<string> bufferedMap;
        private List<string> bufferedPlayers;

        public NetworkInterface(string address, ContentManager c)
        {
            Console.WriteLine("Hello");
            bufferedMap = new List<string>();
            bufferedPlayers = new List<string>();
            messagesReceived = 0;
            sock = new WebSocket(address);
            game = null;
            Content = c;
            connected = false;
            mapLoaded = false;
            roomNumber = -1;
            sock.OnMessage += (sender, e) =>
            {
                byte[] r = e.RawData;
                string s = System.Text.Encoding.UTF8.GetString(r);
                doNetworkInput(s);
            };
            sock.OnOpen += (sender, e) =>
            {
                connected = true;
                startTime = DateTime.Now;
                Console.WriteLine("Connected");
            };
            sock.OnClose += (sender, e) =>
            {
                Console.WriteLine("Ran onclose");
                connected = false;
                mapLoaded = false;
            };
            sock.OnError += (sender, e) =>
            {
                if (e.Message == "An error has occurred while connecting.")
                {
                    connected = false;
                    generateMap();
                    onReady();
                }
            };
            sock.ConnectAsync();
        }

        void generateMap()
        {
            Random ra = new Random();
            for (int x = 0; x < 30; x++)
            {
                string l = generateBufferMapLine(x, ra);
                bufferedMap.Add(l);
            }
        }

        public void generateMoreMap(RockMap map)
        {
            Random ra = new Random();
            for (int x = 0; x < 30; x++)
            {
                string l = generateBufferMapLine(map.cols + x, ra);
                map.addRow(l);
            }
        }

        public string generateBufferMapLine(int depth, Random ra)
        {
            string line = "";
            float probRock = Math.Min(100, (float)((1f / 35f) * Math.Pow(depth, 2)) + 10);
            float probIron = Math.Min(30, (float)((1f / 1000f) * Math.Pow(depth, 2)) + 10);
            float probDiam = Math.Max(0, Math.Min(15, (float)((1f / 10000f) * Math.Pow(depth + 215, 2)) - 10));
            float probMars = Math.Max(0, Math.Min(5, (float)((1f / 10000f) * Math.Pow(depth, 2)) - 2));
            for (int x = 0; x < 31; x++)
            {
                string c = "";
                if (ra.Next(100) < probRock) c = "1";
                if (ra.Next(100) < probIron) c = "2";
                if (ra.Next(100) < probDiam) c = "3";
                if (ra.Next(100) < probMars) c = "4";
                if (c == "") c = "0";
                line += c;
            }
            return line;
        }

        public void setGSM(GameStateMars gsm)
        {
            game = gsm;
            gsm.setNetworkInterface(this);
        }

        public void setOnReady(Game1.fadeDelegate del)
        {
            onReady = del;
        }

        public void sendLocationUpdate(Player p)
        {
            string m = 'm' + playerId;
            m += (int)p.position.X;
            m += "," + (int)p.position.Y;
            sendMessage(m);
        }

        public void sendAnimationUpdate(AnimationHandler a)
        {
            string m = 'u' + playerId;
            m += a.currentAnimation;
            sendMessage(m);
        }

        public void sendMessage(string msg)
        {
            byte[] mData = System.Text.Encoding.UTF8.GetBytes(msg.ToCharArray());
            sock.SendAsync(mData, null);
        }

        public void unloadBufferedMap()
        {
            Console.WriteLine("Unloading buffered map");
            for (int x = 0; x < bufferedMap.Count; x++)
            {
                Console.WriteLine("Buffered: " + bufferedMap[x]);
                game.rockMap.addRow(bufferedMap[x]);
            }
            for (int x = 0; x < bufferedPlayers.Count; x++)
            {
                game.otherPlayers[bufferedPlayers[x]] = new Player("miner.png", Content, new Vector2(375, 5));
            }
        }

        public void sendDrillUpdate(Vector2 drillRock)
        {
            string m = "d" + (int)drillRock.X + "," + (int)drillRock.Y;
            sendMessage(m);
        }

        public void sendLeftTurn()
        {
            sendMessage("l" + playerId);
        }

        public void sendRightTurn()
        {
            sendMessage("r" + playerId);
        }

        public void requestMoreMap()
        {
            sendMessage("z");
        }

        private void doNetworkInput(string message)
        {
            Console.WriteLine("RECV: " + message);
            if (message.StartsWith("o")) // room id
            {
                roomId = message.Substring(1);
            }
            else if (message.StartsWith("y")) // player id
            {
                playerId = message.Substring(1);
            }
            else if (message.StartsWith("l")) // player turned left
            {
                string opid = message.Substring(1);
                game.otherPlayers[opid].facingLeft = true;
            }
            else if (message.StartsWith("r")) // player turned right
            {
                string opid = message.Substring(1);
                game.otherPlayers[opid].facingLeft = false;
            }
            else if (message.StartsWith("a")) // add player to game
            {
                if (mapLoaded)
                {
                    string otherPlayerId = message.Substring(1);
                    game.otherPlayers.Add(otherPlayerId, new Player("miner.png", Content, new Vector2(375, 5)));
                }
                else
                {
                    bufferedPlayers.Add(message.Substring(1)); // create players after GameStateMars can be referenced
                }
            }
            else if (message.StartsWith("u")) // player changing animations
            {
                string otherPlayerId = message.Substring(1, 10);
                int otherPlayerAnim = int.Parse(message.Substring(message.Length - 1));
                game.otherPlayers[otherPlayerId].animHandler.setAnimation(otherPlayerAnim, true);
            }
            else if (message.StartsWith("m")) // other player movement
            {
                string opid = message.Substring(1, 10);
                string[] coords = message.Substring(11).Split(',');
                if (!game.otherPlayers.ContainsKey(opid))
                {
                    game.otherPlayers.Add(opid, new Player("miner.png", Content, new Vector2(375, 5)));
                }
                game.otherPlayers[opid].position = new Vector2(int.Parse(coords[0]), int.Parse(coords[1]));
                Console.WriteLine("Update other player position");
            }
            else if (message.StartsWith("d")) // drill/destroy block
            {
                int x = int.Parse(message.Substring(1).Split(',')[0]);
                int y = int.Parse(message.Substring(1).Split(',')[1]);
                game.rockMap.map[x, y] = null;
            }
            else if (message.StartsWith("z")) // new map line
            {
                if (mapLoaded)
                {
                    game.rockMap.addRow(message.Substring(1).TrimEnd());
                }
                else
                {
                    bufferedMap.Add(message.Substring(1).TrimEnd());
                }
            }
            else if (message.Equals("s")) // start game
            {
                mapLoaded = true;

                Console.WriteLine("Received end of map, players initial stuff loaded");
                
                onReady();
            }
        }


        
    }
}
