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
    public class JsonObjectEncoder<TObject> : IObjectEncoder<TObject>
    {
        public byte[] Encode(TObject value)
        {
            return value.ToJson().ToUtf8Bytes();
        }

        public TObject Decode(byte[] data)
        {
            return data.FromUtf8Bytes().FromJson<TObject>();
        }
    }
}
