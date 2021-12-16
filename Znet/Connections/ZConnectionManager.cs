using System;
using System.Collections.Generic;

namespace Znet.Connections
{
    public class ZConnectionManager
    {
        public int ConnectionCount => _connections.Count;

        private List<ZConnection> _connections;

        public ZConnectionManager()
        {
            _connections = new List<ZConnection>();
        }

        public void AddConnection(ZConnection _item)
        {
            Console.WriteLine("Adding connection");

            if (!_connections.Contains(_item))
            {
                _connections.Add(_item);
                _item.ID = (ushort)ConnectionCount;
            }
        }

        public ZConnection GetConnectionByID(int _ID)
        {
            foreach(ZConnection _conn in _connections)
            {
                if(_conn.ID == _ID)
                {
                    return _conn;
                }
            }
            throw new ArgumentNullException($"Couldn't find ZConnection with id {_ID}.");
        }
    }
}