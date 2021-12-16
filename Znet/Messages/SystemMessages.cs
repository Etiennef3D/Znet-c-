using Znet.Serialization;

namespace Znet.Messages
{
    public class SystemMessages
    {
        public struct WelcomeMessage : ZNetworkMessage
        {
            public int welcomeMessageValue;

            public ZNetworkMessage Deserialize(ref ZReader _reader)
            {
                welcomeMessageValue = _reader.ReadInt32();
                return this;
            }

            public void Serialize(ref ZWriter _writer)
            {
                _writer.WriteInt32(welcomeMessageValue);
            }

            public override string ToString()
            {
                return $"{nameof(WelcomeMessage)} - value : {welcomeMessageValue}";
            }
        }
    }
}