using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Util;
using ServiceStack;
using ServiceStack.Messaging;
using RabbitMQ.Client.Events;
using System;

namespace Common
{
    /// <summary>
    /// Encodes an object into byte array and decodes it from a byte array.
    /// </summary>
    /// <typeparam name="TObject">Type of object to encode or decode.</typeparam>
    public interface IObjectEncoder<TObject>
    {
        byte[] Encode(TObject value);
        TObject Decode(byte[] data);
    }
}
