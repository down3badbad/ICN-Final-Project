using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    class Player
    {
        public int id;
        // public string username;

        //public Vector3 position;
        //public Quaternion rotation;
        public Vector2 position;
        public int status;

        private float moveSpeed = 3f / Constants.TICKS_PER_SEC;
        // private bool[] inputs;

        public Player(int _id)
        {
            id = _id;
            position = MyLocation(_id);
            // inputs = new bool[4];
        }
        public Player(int _id, Vector2 _spawnPosition)
        {
            id = _id;
            position = _spawnPosition;
            //inputs = new bool[4];
        }

        public Vector2 MyLocation(int _id)
        {

            Vector2 offset = new Vector2(2f, 0f);
            Vector2 Init = new Vector2(-8f, 1f);
            return Init + _id * offset;
        }
        public void Update()
        {
            /*
            Vector2 _inputDirection = Vector2.Zero;
            if (inputs[0])
            {
                _inputDirection.Y += 1;
            }
            if (inputs[1])
            {
                _inputDirection.Y -= 1;
            }
            if (inputs[2])
            {
                _inputDirection.X -= 1;
            }
            if (inputs[3])
            {
                _inputDirection.X += 1;
            } */

            Move(position);
        }

        private void Move(Vector2 _inputDirection)
        {
            // Vector2 _forward = Vector2.Transform(new Vector2(0, 0, 1), rotation);
            // Vector2 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, 1, 0)));

            //Vector2 _moveDirection = _right * _inputDirection.X + _forward * _inputDirection.Y;
            // += _moveDirection * moveSpeed;
            // position += _inputDirection * moveSpeed;
            // Console.WriteLine($"id = {id}, pos = {position}");
            // ServerSend.PlayerPosition(this);
            // ServerSend.PlayerRotation(this);
        }

        public void SetInput(bool[] _inputs)
        {
            // inputs = _inputs;
            //rotation = _rotation;
        }

        public void SetInput( Vector2 _position, int _s)
        {
            position = _position;
            status = _s;
        }

    }

}
