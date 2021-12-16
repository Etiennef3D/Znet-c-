using System;
using System.Collections.Generic;
using Znet.Messages;

namespace Znet.Queue
{
    /// <summary>
    /// Receives messages and treat them one by one in the order they are received
    /// </summary>
    public class DatagramReceivingQueue : IQueue<Datagram>
    {
        public int Count => m_queue.Count;


        private const int QUEUE_MAX_SIZE = 100;
        private Stack<Datagram> m_queue;
        private IMessageHandler m_MessageHandler;

        public DatagramReceivingQueue()
        {
            m_queue = new Stack<Datagram>(QUEUE_MAX_SIZE);
            m_MessageHandler = new MessageHandler();
        }

        public void AddToTheQueue(Datagram item)
        {
            m_queue.Push(item);
            Process();
        }

        public void Start() => Process();

        /// <summary>
        /// Read the header of every datagrams in the queue and 
        /// rearrange them to organize them in the right order.
        /// </summary>
        public void Process()
        {
            while(m_queue.Count > 0)
            {
                Sort(ref m_queue);
                m_queue.Pop();
            }
        }

        private void Sort(ref Stack<Datagram> _queue)
        {
            Stack<Datagram> _sortedQueue = new Stack<Datagram>();

            while (_queue.Count > 0)
            {
                Datagram element = _queue.Pop();

                while(_sortedQueue.Count > 0 && element.header.ID < _sortedQueue.Peek().header.ID)
                {
                    _queue.Push(_sortedQueue.Pop());
                }
                _sortedQueue.Push(element);
            }
            _queue = _sortedQueue;
        }
    }
}