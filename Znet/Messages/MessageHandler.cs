using System;
using System.Collections.Generic;
using Znet.Serialization;
using Znet.Utils;
using static Znet.Messages.SystemMessages;

namespace Znet.Messages
{
    public class MessageHandler : IMessageHandler
    {
        private Dictionary<int, Action<ZReader>> _messageCache = new Dictionary<int, Action<ZReader>>();

        public MessageHandler()
        {
            RegisterHandler<WelcomeMessage>(OnWelcomeMessage);
        }

        private void OnWelcomeMessage(ZReader _reader) 
        {
            WelcomeMessage _message = new WelcomeMessage();
            _message.Deserialize(ref _reader);
            Console.WriteLine("Welcome message received: " + _message);
        }

        public void HandleMessage(byte[] _buffer)
        {
            ZReader _reader = new ZReader();

            //if(MessagePacker.UnPack(ref _reader, _buffer, out int _hash))
            //{
            //    if (_messageCache.ContainsKey(_hash))
            //    {
            //        _messageCache[_hash].Invoke(_reader);
            //    }
            //}
        }

        private void RegisterHandler<T>(Action<ZReader> _action) where T : ZNetworkMessage
        {
            int _hash = typeof(T).ToString().GetFixedHashCode();

            if (_messageCache.ContainsKey(_hash))
            {
                throw new Exception($"Key already exists in the dictionary : {_hash}. Please use unique message name.");
            }
            _messageCache.Add(_hash, _action);
        }
    }
}