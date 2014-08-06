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
    public class RabbitMqWriteMessageQueue<TMessage> : RabbitMqMessageQueueBase<TMessage>
    {
        public RabbitMqWriteMessageQueue(ConnectionFactory connectionFactory, MqConfiguration mqConfiguration, bool useTempQueue = false)
            : base(connectionFactory, mqConfiguration, useTempQueue)
        {
        }

        public void Write(TMessage message, string replyQueueName)
        {
            var publishProps = this.Channel.CreateBasicProperties();
            publishProps.ReplyTo = replyQueueName;
            publishProps.SetPersistent(this.MqConfiguration.MessageDurable);

            this.Channel.BasicPublish(ExchangeName, _queueName, publishProps, _messageEncoder.Encode(message));
        }
    }
}
