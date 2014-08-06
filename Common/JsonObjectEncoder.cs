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
            string s = data.FromUtf8Bytes();

            // ServiceStack unescapes JSON data and returns a string without quotes around property and data names
            // when you call FromJson<object> or FromJson<string> on a JSON string. So in that case we should just
            // return the data as is.
            if (typeof(TObject) == typeof(object) || typeof(TObject) == typeof(string))
            {
                return (TObject)(object)s;
            }

            return s.FromJson<TObject>();
        }
    }
}
