using System;
using System.Collections.Generic;
using System.Text;


namespace GameServer
{
    class GameLogic
    {
 
        public static void Update()
        {
            if (Server.isGameStart)
            {
                float current = Server.getRemainTime();
                if ( current > 0)
                {
                    // Console.WriteLine($"remain time = {Server.getRemainTime()}");
                    // bool isUser = false;
                    DateTime thisloop = DateTime.Now;
                    Player[] allplayers = new Player[Server.MaxPlayers];
                    int ind = 0;
                    foreach (Client _client in Server.clients.Values)
                    {
                        // allplayers = { };
                        
                        if (_client.player != null)
                        {

                            // _client.player.Update();
                            if (true)//_client.isAlive(thisloop))
                            {
                                allplayers[ind] = _client.player;
                                ind++;
                            }
                            else
                            {
                                // disconnect
                                Console.WriteLine($"========= disconnect id{_client.id} after it idle too long...==============\n\n");
                                Server.Disconnect(_client.id);
                                _client.Disconnect();
                            }
                            //isUser = true;

                        }
                    }
                    ServerSend.GameStatus(allplayers, ind);
                    /*if (!isUser)
                    {
                        Console.WriteLine("No user, turn off the channel");
                        Server.GameOver(false);
                    }*/

                }
                else
                {
                    Console.WriteLine("============ Game Over ==============");
                    Server.GameOver(false);
                }
                
            }

            ThreadManager.UpdateMain();
        }
    }
}
