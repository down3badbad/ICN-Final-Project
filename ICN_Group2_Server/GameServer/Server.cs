using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>(); // socket to connect each client
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers; // set up packet handler for each client in ServerHandle.cs
        
        private static TcpListener tcpListener;
        private static UdpClient udpListener;


        private static double timestamp = 0;
        private static DateTime starttime;
        public static float TIMECOUNTDOWN = 300f;
        public static bool isGameStart = false;
        public static int[] progressBar = { 0, 0, 0 };

        public static double UnixTimeNow()
        {
            TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return timeSpan.TotalSeconds;
        }
        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting server...");
            InitializeServerData();
            // Change to "10.0.2.15"IPAddress.Any10.0.2.15
            tcpListener = new TcpListener(IPAddress.Parse("10.0.2.15"), Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on port {Port}.");
        }
        public static double getTimeStamp()
        {
            // Console.WriteLine($"timestamp={timestamp}");
            timestamp = (timestamp != 0) ? timestamp : UnixTimeNow();
            return timestamp;
        }
        public static float getRemainTime()
        {
            float passtime;
            // Console.WriteLine($"timestamp={timestamp}");
            if (timestamp != 0)
            {
                passtime = (float)DateTime.Now.Subtract(starttime).TotalSeconds;
            }
            else
            {
                passtime = 0;
                starttime = DateTime.Now;
                // Console.WriteLine($"now={starttime}");
            }
            return TIMECOUNTDOWN - passtime;
        }
        public static void GameOver(bool result)
        {
            isGameStart = false;
            timestamp = 0;
            ServerSend.GameResult(result);
            /*for (int i=1; i<=MaxPlayers; i++)
            {
                if (clients[i].player != null)
                {
                    clients[i].Disconnect();
                }
            }*/
        }
        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");
            // only render red i+=2
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    if (isGameStart)
                    {
                        // float t = getRemainTime();
                        clients[i].tcp.Connect(_client, UnixTimeNow());
                        clients[i].setConnectTime(DateTime.Now);
                    }
                    else
                    {
                        GameStart();
                        clients[i].tcp.Connect(_client, UnixTimeNow());
                        clients[i].setConnectTime(DateTime.Now);
                    }
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                    {
                        return;
                    }

                    if (clients[_clientId].udp.endPoint == null)
                    {
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving UDP data: {_ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                { (int)ClientPackets.resource, ServerHandle.RenewProgressBar },
                { (int)ClientPackets.bullet, ServerHandle.SpawnBullet },
                { (int)ClientPackets.sendBomb, ServerHandle.SpawnBomb },
                { (int)ClientPackets.gunpos, ServerHandle.RotateGun }
            };
            Console.WriteLine("Initialized packets.");
        }


        public static bool teamCwin()
        {
            return (progressBar[0] == 100 && progressBar[1] == 100 && progressBar[2] == 100);
        }

        private static void GameStart()
        {
            Console.WriteLine("Game start ..............");
            isGameStart = true;
            progressBar[0] = 0;
            progressBar[1] = 0;
            progressBar[2] = 0;
            timestamp = 0;
        }

        public static void Disconnect(int id)
        {
            Console.WriteLine($"send player{id} disconnect info to other player");
            ServerSend.PlayerDisconnect(id);
        }
        
    }

}
