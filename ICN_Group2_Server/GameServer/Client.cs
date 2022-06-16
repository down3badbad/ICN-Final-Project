using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace GameServer
{
    class Client
    {
        public static int dataBufferSize = 4096;
        private DateTime lastacttime;
        private DateTime Connecttime;
        public int id;
        public Player player;
        public TCP tcp;
        public UDP udp;
        private static double TimeThershold = 15f;
        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket, double timestamp)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                Console.WriteLine("send welcome message...");
                ServerSend.Welcome(id, timestamp);
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        // TODO: disconnect
                        Server.Disconnect(id);
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {_ex}");
                    // TODO: disconnect
                    Server.Disconnect(id);
                    Server.clients[id].Disconnect();
                    
                }
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData)
            {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
            }
        }

        public void SendIntoGame(string _playerName)
        {
            // player = new Player(id, new Vector3(0, 0, 0));
            player = new Player(id);
            Player[] myInfo = new Player[1];
            myInfo[0] = player;
            Player[] allInfo = new Player[Server.MaxPlayers];
            int ind = 0;
            Console.WriteLine("Announce new players!!...");
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    
                    if (_client.id != id)
                    {
                        ServerSend.SpawnPlayers(_client.id, myInfo, 1, 0, Server.getTimeStamp(), 0, Server.progressBar);
                    }
                    allInfo[ind] = _client.player;
                    ind++;
                    
                }
            }
            Console.WriteLine("Send old players to new players...");
            float RTT = computeRTT(DateTime.Now);
            ServerSend.SpawnPlayers(id, allInfo, ind, Server.getRemainTime(), Server.getTimeStamp(), RTT, Server.progressBar);
            
            /*foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    if (_client.id != id)
                    {
                        // tell new player all the other players
                        ServerSend.SpawnPlayer(id, _client.player);
                    }
                }
            }*/
            
        }

        public void Disconnect()
        {
            if (player == null) return;
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
            player = null;
            tcp.Disconnect();
            udp.Disconnect();
        }
        public void setAlive()
        {
            lastacttime = DateTime.Now;
            return;
        }
        public bool isAlive(DateTime now)
        {
            return now.Subtract(lastacttime).TotalSeconds < TimeThershold;
        }

        public void setConnectTime(DateTime c)
        {
            Connecttime = c;
        }

        private float computeRTT(DateTime c)
        {
            return (float)c.Subtract(Connecttime).TotalSeconds/2;
        }
    }
}
