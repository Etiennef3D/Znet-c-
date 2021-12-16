using Znet.Serialization;

namespace Znet.Messages
{
    public interface ZNetworkMessage
    {
        void Serialize(ref ZWriter _writer);
        ZNetworkMessage Deserialize(ref ZReader _reader);
    }
}