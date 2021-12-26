using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Znet.Multiplexer
{
    interface IMultiplexer
    {
        void Queue(byte[] _message);
        int Serialize(ref byte[] _buffer, int _bufferSize);
    }
}
