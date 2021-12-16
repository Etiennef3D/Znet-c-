namespace Znet.Messages
{
    public interface IMessageHandler
    {
        void HandleMessage(byte[] _buffer);
    }
}