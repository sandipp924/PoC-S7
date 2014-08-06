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
    public class MqConfiguration
    {
        public bool ExchangeDurable { get; set; }
        public bool QueueDurable { get; set; }
        public bool MessageDurable { get; set; }
        public bool NoAck { get; set; }
    }
}
