using System;
/*using System.Collections.Generic;
using System.Text;*/
using System.Numerics;

namespace GameServer
{
    class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if ( i != _exceptClient && Server.clients[i].player != null )
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        #region Packets
        public static void Welcome(int _toClient, double startTime)
        {
            Console.WriteLine($"welcome id: {_toClient}, starting timestamp = {startTime}");
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_toClient);
                // _packet.Write(_time);
                _packet.Write(startTime);
                SendTCPData(_toClient, _packet);
            }
        }

        public static void SpawnPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                // _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.status);
                // _packet.Write(_player.rotation);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void SpawnPlayerwithInfo(int _toClient, Player _player, float time, double stamp, float RTT, int[] progress)
        {
            Console.WriteLine($"sending time = {time}...and RTT = {RTT}...");
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);
                _packet.Write(0);
                _packet.Write(time);
                _packet.Write(RTT);
                for (int i = 0; i < 3; i++)
                {
                    _packet.Write(progress[i]);
                }
                _packet.Write(stamp);
                SendTCPData(_toClient, _packet);
            }
        }
        public static void SpawnPlayers(int _toClient, Player[] _players, int ind, float time, double stamp, float RTT, int[] progress)
        {
            // Console.WriteLine($"sending players = {_players}, ind = {ind}, and RTT = {RTT}...");
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(ind);
                _packet.Write(time);
                _packet.Write(RTT);
                for (int i = 0; i < 3; i++)
                {
                    _packet.Write(progress[i]);
                }
                for (int i=0; i< ind; i++)
                {
                    _packet.Write(_players[i].id);
                    _packet.Write(_players[i].position);
                    _packet.Write(_players[i].status);
                    // Console.WriteLine($"id={_players[i].id}, pos={_players[i].position}, status={_players[i].status}");
                }                
                // _packet.Write(stamp);
                SendTCPData(_toClient, _packet);
            }
        }

        public static void PlayerPosition(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);
                _packet.Write(_player.status);

                SendUDPDataToAll(_player.id, _packet);
            }
            //Console.WriteLine($"send id={_player.id} with status = {_player.status}");
        }

        /*
        public static void PlayerRotation(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.rotation);

                SendUDPDataToAll(_player.id, _packet);
            }
        }*/

        public static void UpdataProgress(int id, int[] progress)
        {
            using (Packet _packet = new Packet((int)ServerPackets.updateResource))
            {
                _packet.Write(id);
                for (int i=0; i<3; i++)
                {
                    _packet.Write(progress[i]);
                }
                SendTCPDataToAll(_packet);
            }
            Console.WriteLine($"update resource {progress[0]} {progress[1]} {progress[2]}");
        }

        public static void GameResult(bool teamC)
        {
            using (Packet _packet = new Packet((int)ServerPackets.gameover))
            {
                _packet.Write(teamC);
                SendTCPDataToAll(_packet);
            }
        }

        public static void PlayerDisconnect(int _id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.disconnect))
            {
                _packet.Write(_id);
                SendTCPDataToAll(_id, _packet);
            }
        }

        public static void SpawnBomb(int id, Vector2 pos)
        {
            using (Packet _packet = new Packet((int)ServerPackets.Bomb))
            {
                _packet.Write(id);
                _packet.Write(pos);
                SendTCPDataToAll(id, _packet);
            }
        }

        public static void SpawnBullet(int id, Vector2 pos, Quaternion rotation)
        {
            using (Packet _packet = new Packet((int)ServerPackets.shot))
            {
                _packet.Write(pos);
                _packet.Write(rotation);
                SendTCPDataToAll(id, _packet);
            }
        }

        public static void RotateGun(int id, Vector3 pos, float f)
        {
            using (Packet _packet = new Packet((int)ServerPackets.gunpos))
            {
                _packet.Write(id);
                _packet.Write(pos);
                _packet.Write(f);
                SendUDPDataToAll(id, _packet);
            }
        }

        public static void GameStatus(Player[] _players, int num)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(num);
                for (int i=0; i< num; i++)
                {
                    _packet.Write(_players[i].id);
                    _packet.Write(_players[i].position);
                    _packet.Write(_players[i].status);
                    //Console.WriteLine($"s: id={_players[i].id}, pos={_players[i].position}, status={_players[i].status}");
                }               
                SendUDPDataToAll(_packet);
            }
        }
        #endregion

    }
}
