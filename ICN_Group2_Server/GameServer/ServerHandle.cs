using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    class ServerHandle
    {
        public static void UpdateAlive(int _fromClient)
        {
            Server.clients[_fromClient].setAlive();
        }
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            UpdateAlive(_fromClient);
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();
            // Console.WriteLine("get welcome receive ............");
            // Console.WriteLine($"A new {_username}");
            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            Server.clients[_fromClient].SendIntoGame(_username);
        }
        
        public static void PlayerMovement(int _fromClient, Packet _packet)
        {
            // UpdateAlive(_fromClient);
            Vector2 _pos = _packet.ReadVector2();
            int characterstatus = _packet.ReadInt();
            // Console.WriteLine($"receive pos from id{_fromClient} with status = {characterstatus}");
            if (Server.clients[_fromClient].player != null)
            {
                Server.clients[_fromClient].player.SetInput(_pos, characterstatus);
            }
        }


        public static void RenewProgressBar(int _fromClient, Packet _packet)
        {
            int resource = _packet.ReadInt();
            Console.WriteLine($"Info: id{_fromClient} get resource = {resource}...");
            Server.progressBar[resource] = Math.Min(100, Server.progressBar[resource]+10);
            ServerSend.UpdataProgress(_fromClient, Server.progressBar);
            if (Server.teamCwin())
            {
                Server.GameOver(true);// team C win
            }
            
        }

        public static void SpawnBullet(int _fromClient, Packet _packet)
        {
            Vector2 _pos = _packet.ReadVector2();
            Quaternion _rotation = _packet.ReadQuaternion();
            // Console.WriteLine($"Fire: player{_fromClient} shoots");// with pos = {_pos}  and q = {_rotation}");

            ServerSend.SpawnBullet(_fromClient, _pos, _rotation);
        }
        public static void SpawnBomb(int _fromClient, Packet _packet)
        {
            Vector2 _pos = _packet.ReadVector2();
            // Console.WriteLine($"Fire: player{_fromClient} loads a bomb");
            ServerSend.SpawnBomb(_fromClient, _pos);
        }

        public static void RotateGun(int _fromClient, Packet _packet)
        {
            Vector3 _pos = _packet.ReadVector3();
            float f = _packet.ReadFloat();
            // Console.WriteLine($"receive gun from id{_fromClient} with pos = {_pos} and float = {f}");
            ServerSend.RotateGun(_fromClient, _pos, f);
        }
    }
}
