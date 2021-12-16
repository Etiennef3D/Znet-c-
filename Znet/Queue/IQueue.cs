namespace Znet.Queue
{
    public interface IQueue<T>
    {
        void Process();
        void Start();
        int Count { get; }
        void AddToTheQueue(T item);
    }
}