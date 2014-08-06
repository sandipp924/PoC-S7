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
    public class RabbitMqReadMessageQueue<TMessage> : RabbitMqMessageQueueBase<TMessage>
    {
        private readonly QueueingBasicConsumer _consumer;

        public RabbitMqReadMessageQueue(ConnectionFactory connectionFactory, MqConfiguration mqConfiguration, bool useTempQueue = false)
            : base(connectionFactory, mqConfiguration, useTempQueue)
        {
            _consumer = new QueueingBasicConsumer(this.Channel);
            this.Channel.BasicConsume(_queueName, mqConfiguration.NoAck, _consumer);
        }

        public bool Get(TimeSpan? timeOut, out TMessage message, out BasicDeliverEventArgs messageEventArgs)
        {
            if (!timeOut.HasValue)
            {
                messageEventArgs = _consumer.Queue.Dequeue();
            }
            else
            {
                if (!_consumer.Queue.Dequeue(timeOut.HasValue ? (int)timeOut.Value.TotalMilliseconds : 0, out messageEventArgs))
                {
                    message = default(TMessage);
                    return false;
                }
            }

            message = _messageEncoder.Decode(messageEventArgs.Body);
            return true;
        }
    }
}
